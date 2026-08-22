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

/// <summary>
/// Lokalni AI provider preko Ollama chat API-ja (<c>/api/chat</c>) — npr. Gemma.
/// Isti <see cref="IAIService"/> ugovor kao <see cref="GeminiAIService"/>; koristi se
/// u dev okruženju kada je <c>Ai:Provider = "Ollama"</c>. Koristi imenovani HttpClient
/// "Ollama" čija je BaseAddress podešena u DI-ju.
/// </summary>
public class OllamaAIService(
    IHttpClientFactory httpClientFactory,
    IOptions<OllamaOptions> options,
    ICurrentUserService currentUserService,
    ILogger<OllamaAIService> logger,
    ApplicationDbContext dbContext
) : IAIService
{
    private const string HttpClientName = "Ollama";

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

        // Ollama /api/chat: slike idu kao base64 (bez data: prefiksa) na user poruci.
        var userImages = images is { Count: > 0 }
            ? images.Select(i => Convert.ToBase64String(i.Data)).ToList()
            : null;

        var request = new OllamaChatRequest(
            Model: options.Value.DefaultModel,
            Messages: new[]
            {
                new OllamaMessage("system", systemPrompt, Images: null),
                new OllamaMessage("user", userPrompt, userImages),
            },
            Stream: false,
            // "thinking" modeli (npr. Gemma) inače troše budžet na interno razmišljanje
            // i vraćaju prazan content; nama treba direktan (JSON) odgovor.
            Think: false,
            Options: new OllamaRuntimeOptions(opts.MaxOutputTokens)
        );

        var model = options.Value.DefaultModel;
        var client = httpClientFactory.CreateClient(HttpClientName);

        OllamaChatResponse? response;
        try
        {
            using var httpResponse = await client.PostAsJsonAsync(
                "api/chat",
                request,
                SerializerOptions,
                ct
            );

            if (!httpResponse.IsSuccessStatusCode)
            {
                var body = await httpResponse.Content.ReadAsStringAsync(ct);
                logger.LogError(
                    "Ollama API call failed for model {Model}. Status={Status} Body={Body}",
                    model,
                    (int)httpResponse.StatusCode,
                    body
                );
                throw new AIServiceException(
                    $"AI service call failed with status {(int)httpResponse.StatusCode}."
                );
            }

            response = await httpResponse.Content.ReadFromJsonAsync<OllamaChatResponse>(
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
            logger.LogError(ex, "Ollama API call failed for model {Model}", model);
            throw new AIServiceException("AI service call failed.", ex);
        }

        var inputTokens = response?.PromptEvalCount ?? 0;
        var outputTokens = response?.EvalCount ?? 0;

        logger.LogInformation(
            "AI call complete (Ollama). Model={Model} In={In} Out={Out}",
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
                "OllamaAIService",
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

        return response?.Message?.Content ?? string.Empty;
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

    // --- Ollama /api/chat DTOs ---

    private sealed record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<OllamaMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("think")] bool Think,
        [property: JsonPropertyName("options")] OllamaRuntimeOptions Options
    );

    private sealed record OllamaMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("images")] IReadOnlyList<string>? Images
    );

    private sealed record OllamaRuntimeOptions(
        [property: JsonPropertyName("num_predict")] int NumPredict
    );

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaMessage? Message,
        [property: JsonPropertyName("prompt_eval_count")] int PromptEvalCount,
        [property: JsonPropertyName("eval_count")] int EvalCount
    );
}
