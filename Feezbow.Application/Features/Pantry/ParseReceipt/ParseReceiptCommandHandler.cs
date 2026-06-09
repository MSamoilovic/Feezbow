using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using MediatR;

namespace Feezbow.Application.Features.Pantry.ParseReceipt;

public class ParseReceiptCommandHandler(IAIService aiService)
    : IRequestHandler<ParseReceiptCommand, ParseReceiptCommandResponse>
{
    private const string _systemPrompt = """
        You are a grocery receipt parser. Analyze the receipt image and extract each
        purchased food or household item.
        For each item return:
        - name: standardized English food name (e.g. "Whole Milk", not "MLK 3.2%")
        - quantity: numeric amount purchased (default 1 if unclear)
        - unit: g | kg | ml | l | pcs | pack (null if not applicable)
        - expiresInDays: estimated shelf life in days from today
          (fridge items ~7, frozen ~90, dry goods ~365, null if unknown)
        Ignore prices, store info, totals, and non-food items.
        Return ONLY valid JSON: { "items": [{ "name": string, "quantity": number, "unit": string|null, "expiresInDays": number|null }] }
        """;

    public async Task<ParseReceiptCommandResponse> Handle(
        ParseReceiptCommand request,
        CancellationToken cancellationToken
    )
    {
        var json = await aiService.CompleteWithImagesAsync(
            _systemPrompt,
            "Parse this receipt.",
            [new AIImageInput(request.ImageData, request.MediaType)],
            new AIRequestOptions(
                MaxInputTokens: 8_000,
                MaxOutputTokens: 2_048,
                AllowCaching: true,
                RequiresFeature: "PantryReceiptParser"
            ),
            cancellationToken
        );

        // Strip markdown code fences if present
        json = json.Trim();
        if (json.StartsWith("```"))
            json = string.Join('\n', json.Split('\n')[1..^1]);

        RawReceipt raw;
        try
        {
            raw = System.Text.Json.JsonSerializer.Deserialize<RawReceipt>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
        }
        catch
        {
            throw new AIResponseParseException(nameof(ParseReceiptCommandResponse));
        }

        var today = DateTime.UtcNow.Date;

        var items = (raw.Items ?? [])
            .Where(i => !string.IsNullOrWhiteSpace(i.Name))
            .Select(i => new ParsedPantryItemDto(
                Name: i.Name!.Trim(),
                Quantity: (decimal)(i.Quantity ?? 1),
                Unit: string.IsNullOrWhiteSpace(i.Unit) ? null : i.Unit.Trim(),
                ExpirationDate: i.ExpiresInDays.HasValue
                    ? today.AddDays(i.ExpiresInDays.Value)
                    : null
            ))
            .ToList();

        return new ParseReceiptCommandResponse(items);
    }

    private record RawItem(string? Name, double? Quantity, string? Unit, int? ExpiresInDays);

    private record RawReceipt(List<RawItem>? Items);
}
