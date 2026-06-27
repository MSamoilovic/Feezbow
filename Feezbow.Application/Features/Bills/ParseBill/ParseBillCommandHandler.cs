using System.Text.Json;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using MediatR;

namespace Feezbow.Application.Features.Bills.ParseBill;

public class ParseBillCommandHandler(IAIService aiService, IPdfTextExtractor pdfExtractor)
    : IRequestHandler<ParseBillCommand, ParseBillCommandResponse>
{
    private const string SystemPrompt = """
        You are a bill and invoice OCR parser. Extract key financial information from
        the provided document — utility bills, rent invoices, subscription receipts,
        insurance statements, tax notices, and similar.

        Return ONLY valid JSON with this exact structure:
        {
          "title": "short descriptive name (e.g. 'Electric Bill', 'Rent March 2026', 'Netflix Subscription')",
          "amount": 42.50,
          "currency": "EUR",
          "dueDate": "2026-07-15",
          "category": "Utilities",
          "description": "issuer or payer name (optional, null if unclear)",
          "confidence": 0.95
        }

        Rules:
        - title: concise name for the bill, max 100 characters.
        - amount: total amount due (not already paid). Positive decimal.
        - currency: 3-letter ISO 4217 code. Default to EUR if unclear.
        - dueDate: ISO 8601 date YYYY-MM-DD. Set to null if no due date is stated.
        - category: one of exactly: Utilities | Rent | Insurance | Internet | Subscription | Tax | Medical | Other
        - description: issuer name or short context. null if not determinable.
        - confidence: 0.0–1.0 reflecting how accurately you identified all fields.
        """;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<ParseBillCommandResponse> Handle(
        ParseBillCommand request,
        CancellationToken cancellationToken
    )
    {
        var opts = new AIRequestOptions(
            MaxInputTokens: 12_000,
            MaxOutputTokens: 512,
            AllowCaching: true,
            RequiresFeature: "BillOcr"
        );

        string json;

        if (request.MediaType == "application/pdf")
        {
            var text = pdfExtractor.ExtractFirstPages(request.FileData);
            if (string.IsNullOrWhiteSpace(text))
                throw new AIResponseParseException(nameof(ParseBillCommandResponse));

            json = await aiService.CompleteAsync(
                SystemPrompt,
                $"Parse this bill. Text:\n{text}",
                opts,
                cancellationToken
            );
        }
        else
        {
            json = await aiService.CompleteWithImagesAsync(
                SystemPrompt,
                "Parse this bill.",
                [new AIImageInput(request.FileData, request.MediaType)],
                opts,
                cancellationToken
            );
        }

        json = json.Trim();
        if (json.StartsWith("```"))
            json = string.Join('\n', json.Split('\n')[1..^1]);

        RawBill raw;
        try
        {
            raw = JsonSerializer.Deserialize<RawBill>(json, JsonOptions)
                ?? throw new AIResponseParseException(nameof(ParseBillCommandResponse));
        }
        catch (JsonException)
        {
            throw new AIResponseParseException(nameof(ParseBillCommandResponse));
        }

        if (string.IsNullOrWhiteSpace(raw.Title) || raw.Amount is null or <= 0)
            throw new AIResponseParseException(nameof(ParseBillCommandResponse));

        DateTime? dueDate = null;
        if (raw.DueDate is not null && DateTime.TryParse(raw.DueDate, out var parsed))
            dueDate = DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);

        var dto = new ParsedBillDto(
            Title: raw.Title.Trim(),
            Amount: (decimal)raw.Amount.Value,
            Currency: string.IsNullOrWhiteSpace(raw.Currency)
                ? "EUR"
                : raw.Currency.Trim().ToUpperInvariant(),
            DueDate: dueDate,
            Category: string.IsNullOrWhiteSpace(raw.Category) ? null : raw.Category.Trim(),
            Description: string.IsNullOrWhiteSpace(raw.Description)
                ? null
                : raw.Description.Trim(),
            Confidence: raw.Confidence ?? 0.0
        );

        return new ParseBillCommandResponse(dto);
    }

    private record RawBill(
        string? Title,
        double? Amount,
        string? Currency,
        string? DueDate,
        string? Category,
        string? Description,
        double? Confidence);
}
