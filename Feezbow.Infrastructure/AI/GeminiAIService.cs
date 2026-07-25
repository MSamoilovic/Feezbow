using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Infrastructure.Options;
using Feezbow.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Feezbow.Infrastructure.AI;

public class GeminiAIService(
    IHttpClientFactory httpClientFactory,
    IOptions<GeminiOptions> options,
    ICurrentUserService currentUserService,
    ILogger<GeminiAIService> logger,
    ApplicationDbContext dbContext
) : IAIService
{
    private const string HttpClientName = "Gemini";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        AIRequestOptions? options = null,
        CancellationToken cancellationToken = default
    ) => SendAsync(systemPrompt, userPrompt, images: null, options, cancellationToken);

    public Task<string> CompleteWithImagesAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<AIImageInput> images,
        AIRequestOptions? options = null,
        CancellationToken cancellationToken = default
    ) => SendAsync(systemPrompt, userPrompt, images, options, cancellationToken);

    private async Task<string> SendAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<AIImageInput>? images,
        AIRequestOptions? opts,
        CancellationToken ct
    )
    {
        opts ??= new AIRequestOptions();

        EnsureFeatureEnabled(opts.RequiresFeature);

        // Token budget guard --- english => chars/4
        int estimatedTokens = (int)Math.Ceiling((systemPrompt.Length + userPrompt.Length) / 4.0);
        if (estimatedTokens > opts.MaxInputTokens)
            throw new AITokenBudgetExceededException(estimatedTokens, opts.MaxInputTokens);

        // Convention - Images before text
        var parts = new List<GeminiPart>();
        if (images != null)
        {
            foreach (AIImageInput img in images)
            {
                parts.Add(
                    new GeminiPart(
                        InlineData: new GeminiInlineData(
                            img.MediaType,
                            Convert.ToBase64String(img.Data)
                        )
                    )
                );
            }
        }

        parts.Add(new GeminiPart(Text: userPrompt));

        var request = new GeminiRequest(
            SystemInstruction: new GeminiContent([new GeminiPart(Text: systemPrompt)]),
            Contents: [new GeminiContent(parts, Role: "user")],
            GenerationConfig: new GeminiGenerationConfig(opts.MaxOutputTokens)
        );

        var model = options.Value.DefaultModel;
        var client = httpClientFactory.CreateClient(HttpClientName);

        GeminiResponse? response;
        try
        {
            using var httpResponse = await client.PostAsJsonAsync(
                $"models/{model}:generateContent",
                request,
                SerializerOptions,
                ct
            );

            if (!httpResponse.IsSuccessStatusCode)
            {
                var body = await httpResponse.Content.ReadAsStringAsync(ct);
                logger.LogError(
                    "Gemini API call failed for model {Model}. Status={Status} Body={Body}",
                    model,
                    (int)httpResponse.StatusCode,
                    body
                );
                throw new AIServiceException(
                    $"AI service call failed with status {(int)httpResponse.StatusCode}."
                );
            }

            response = await httpResponse.Content.ReadFromJsonAsync<GeminiResponse>(
                SerializerOptions,
                ct
            );
        }
        catch (AIServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini API call failed for model {Model}", model);
            throw new AIServiceException("AI service call failed.", ex);
        }

        var inputTokens = response?.UsageMetadata?.PromptTokenCount ?? 0;
        var outputTokens = response?.UsageMetadata?.CandidatesTokenCount ?? 0;

        logger.LogInformation(
            "AI call complete. Model={Model} In={In} Out={Out}",
            model,
            inputTokens,
            outputTokens
        );

        try
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userPrompt)))[
                ..16
            ];
            var log = AiAuditLog.Create(
                currentUserService.UserId,
                "GeminiAIService",
                hash,
                inputTokens,
                outputTokens,
                cacheHit: false
            );

            dbContext.AiAuditLogs.Add(log);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write AI audit log");
        }

        return ExtractText(response);
    }

    private void EnsureFeatureEnabled(string? requiresFeature)
    {
        var value = options.Value;

        if (!value.Enabled)
            throw new AIServiceException("AI service is disabled.");

        var disabled = requiresFeature switch
        {
            "RecipeParser" => !value.RecipeParserEnabled,
            "TaskParser" => !value.TaskParserEnabled,
            "PantryReceiptParser" => !value.PantryReceiptParserEnabled,
            "BillOcr" => !value.BillOcrEnabled,
            "WeeklyDigest" => !value.WeeklyDigestEnabled,
            "MealPlanSuggest" => !value.MealPlanSuggestEnabled,
            "ShoppingListCleanup" => !value.ShoppingListCleanupEnabled,
            _ => false,
        };

        if (disabled)
            throw new AIServiceException($"{requiresFeature} feature is disabled.");
    }

    private static string ExtractText(GeminiResponse? response)
    {
        var parts = response?.Candidates?.FirstOrDefault()?.Content?.Parts;
        if (parts is null)
            return string.Empty;

        var text = string.Concat(parts.Where(p => p.Text is not null).Select(p => p.Text));
        return text;
    }

    // --- Gemini REST DTOs ---

    private sealed record GeminiRequest(
        [property: JsonPropertyName("system_instruction")] GeminiContent SystemInstruction,
        [property: JsonPropertyName("contents")] IReadOnlyList<GeminiContent> Contents,
        [property: JsonPropertyName("generationConfig")] GeminiGenerationConfig GenerationConfig
    );

    private sealed record GeminiContent(
        [property: JsonPropertyName("parts")] IReadOnlyList<GeminiPart> Parts,
        [property: JsonPropertyName("role")] string? Role = null
    );

    private sealed record GeminiPart(
        [property: JsonPropertyName("text")] string? Text = null,
        [property: JsonPropertyName("inline_data")] GeminiInlineData? InlineData = null
    );

    private sealed record GeminiInlineData(
        [property: JsonPropertyName("mime_type")] string MimeType,
        [property: JsonPropertyName("data")] string Data
    );

    private sealed record GeminiGenerationConfig(
        [property: JsonPropertyName("maxOutputTokens")] int MaxOutputTokens
    );

    private sealed record GeminiResponse(
        [property: JsonPropertyName("candidates")] List<GeminiCandidate>? Candidates,
        [property: JsonPropertyName("usageMetadata")] GeminiUsage? UsageMetadata
    );

    private sealed record GeminiCandidate(
        [property: JsonPropertyName("content")] GeminiContent? Content,
        [property: JsonPropertyName("finishReason")] string? FinishReason
    );

    private sealed record GeminiUsage(
        [property: JsonPropertyName("promptTokenCount")] int PromptTokenCount,
        [property: JsonPropertyName("candidatesTokenCount")] int CandidatesTokenCount
    );
}
