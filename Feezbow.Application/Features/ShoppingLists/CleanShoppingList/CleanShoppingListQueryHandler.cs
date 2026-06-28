using System.Text.Json;
using System.Text.Json.Serialization;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;
using MediatR;

namespace Feezbow.Application.Features.ShoppingLists.CleanShoppingList;

public class CleanShoppingListQueryHandler(
    IShoppingListService shoppingListService,
    IAIService aiService,
    ICurrentUserService currentUserService) : IRequestHandler<CleanShoppingListQuery, CleanShoppingListQueryResponse>
{
    private const string SystemPrompt =
        """
        You are a shopping list optimizer. Given a JSON array of shopping list items (id, name, quantity, unit),
        identify:
        1. Duplicates or near-duplicates to merge (same ingredient, different spelling or unit).
           - Convert units to the same base before summing (ml↔l, g↔kg).
           - Keep the item with the clearest name as the survivor; include the merged total quantity.
        2. Misspellings or abbreviations to rename (e.g. "mlk" → "Milk").

        Return ONLY valid JSON with no markdown or explanation:
        {
          "merges": [{ "survivors": [{ "id": int, "name": string, "quantity": float|null, "unit": string|null }], "removed": [int] }],
          "renames": [{ "id": int, "oldName": string, "newName": string }]
        }

        If nothing needs cleaning, return { "merges": [], "renames": [] }.
        """;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<CleanShoppingListQueryResponse> Handle(
        CleanShoppingListQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        var list = await shoppingListService.GetByIdAsync(request.ShoppingListId, userId, cancellationToken);

        var unpurchasedItems = list.Items
            .Where(i => !i.IsPurchased)
            .ToList();

        if (unpurchasedItems.Count < 2)
            return new CleanShoppingListQueryResponse(list.Id, [], [], IsAlreadyClean: true);

        var itemsJson = JsonSerializer.Serialize(
            unpurchasedItems.Select(i => new
            {
                i.Id,
                i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
            }));

        var json = await aiService.CompleteAsync(
            SystemPrompt,
            $"Clean this shopping list:\n{itemsJson}",
            new AIRequestOptions(
                RequiresFeature: "ShoppingListCleanup",
                MaxInputTokens: 4_000,
                MaxOutputTokens: 512,
                AllowCaching: true),
            cancellationToken);

        var raw = ParseOrThrow<RawCleanupResponse>(json);

        var merges = (raw.Merges ?? [])
            .Select(m => new MergeGroupDto(
                m.Survivors?.Select(s => new MergeSurvivorDto(s.Id, s.Name, s.Quantity, s.Unit)).ToList() ?? [],
                m.Removed ?? []))
            .Where(m => m.Removed.Count > 0)
            .ToList();

        var renames = (raw.Renames ?? [])
            .Select(r => new RenameDto(r.Id, r.OldName, r.NewName))
            .ToList();

        var isAlreadyClean = merges.Count == 0 && renames.Count == 0;

        return new CleanShoppingListQueryResponse(list.Id, merges, renames, isAlreadyClean);
    }

    private static T ParseOrThrow<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, JsonOptions)!; }
        catch { throw new AIResponseParseException(typeof(T).Name); }
    }

    private record RawSurvivor(
        long Id,
        string Name,
        [property: JsonPropertyName("quantity")] decimal? Quantity,
        [property: JsonPropertyName("unit")] string? Unit);

    private record RawMerge(
        [property: JsonPropertyName("survivors")] IReadOnlyList<RawSurvivor>? Survivors,
        [property: JsonPropertyName("removed")] IReadOnlyList<long>? Removed);

    private record RawRename(
        long Id,
        [property: JsonPropertyName("oldName")] string OldName,
        [property: JsonPropertyName("newName")] string NewName);

    private record RawCleanupResponse(
        [property: JsonPropertyName("merges")] IReadOnlyList<RawMerge>? Merges,
        [property: JsonPropertyName("renames")] IReadOnlyList<RawRename>? Renames);
}
