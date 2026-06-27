using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Models.MealPlanSuggestions;
using MediatR;

namespace Feezbow.Application.Features.MealPlans.SuggestMealPlan;

public class SuggestMealPlanQueryHandler(
    IMealPlanSuggestionService suggestionService,
    IAIService aiService,
    ICurrentUserService currentUserService) : IRequestHandler<SuggestMealPlanQuery, SuggestMealPlanQueryResponse>
{
    private const string SystemPrompt =
        """
        You are a meal planning assistant. Given:
        1. A pantry inventory (JSON array of {name, quantity, unit})
        2. A recipe library (JSON array of {id, name, ingredients: [string]})
        3. Empty meal plan slots (JSON array of {dayOfWeek, mealType})
        4. Optional dietary preferences and serving count

        Suggest recipes that best use available pantry items. Prefer recipes with high pantry overlap.
        Only suggest for slots that are in the emptySlots list.
        For each suggestion include: dayOfWeek (full name, e.g. "Monday"), mealType (e.g. "Breakfast"),
        recipeName, matchedIngredients (pantry items used by the recipe), missingIngredients (needed but absent from pantry).

        Return ONLY valid JSON with no markdown or explanation:
        { "suggestions": [{ "dayOfWeek": string, "mealType": string, "recipeName": string, "matchedIngredients": [string], "missingIngredients": [string] }] }
        """;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<SuggestMealPlanQueryResponse> Handle(
        SuggestMealPlanQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        var context = await suggestionService.GetContextAsync(
            request.ProjectId, request.MealPlanId, userId, cancellationToken);

        if (context.EmptySlots.Count == 0)
            return new SuggestMealPlanQueryResponse(request.MealPlanId, []);

        var userPrompt = BuildUserPrompt(context, request.Preferences, request.Servings);

        var json = await aiService.CompleteAsync(
            SystemPrompt,
            userPrompt,
            new AIRequestOptions(
                RequiresFeature: "MealPlanSuggest",
                MaxInputTokens: 12_000,
                MaxOutputTokens: 1_024,
                AllowCaching: true),
            cancellationToken);

        var raw = ParseOrThrow<RawSuggestionsResponse>(json);

        var recipeIndex = context.Recipes
            .ToDictionary(r => r.Name.ToLowerInvariant(), r => r.Id);

        var suggestions = raw.Suggestions
            .Select(s => new MealSuggestionDto(
                s.DayOfWeek,
                s.MealType,
                s.RecipeName,
                ResolveRecipeId(s.RecipeName, recipeIndex),
                s.MatchedIngredients ?? [],
                s.MissingIngredients ?? []))
            .ToList();

        return new SuggestMealPlanQueryResponse(request.MealPlanId, suggestions);
    }

    private static string BuildUserPrompt(
        MealPlanSuggestionContext ctx,
        string? preferences,
        int? servings)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Week starting: {ctx.WeekStart:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(preferences))
            sb.AppendLine($"Dietary preferences: {preferences}");

        if (servings.HasValue)
            sb.AppendLine($"Servings: {servings}");

        sb.AppendLine();
        sb.AppendLine("Pantry inventory:");
        sb.AppendLine(JsonSerializer.Serialize(
            ctx.PantryItems.Select(p => new { p.Name, p.Quantity, p.Unit })));

        sb.AppendLine();
        sb.AppendLine("Recipe library:");
        sb.AppendLine(JsonSerializer.Serialize(
            ctx.Recipes.Take(80).Select(r => new { r.Id, r.Name, Ingredients = r.Ingredients })));

        sb.AppendLine();
        sb.AppendLine("Empty meal plan slots:");
        sb.AppendLine(JsonSerializer.Serialize(
            ctx.EmptySlots.Select(s => new { DayOfWeek = s.DayOfWeek.ToString(), MealType = s.MealType.ToString() })));

        return sb.ToString();
    }

    private static T ParseOrThrow<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, JsonOptions)!; }
        catch { throw new AIResponseParseException(typeof(T).Name); }
    }

    private static long? ResolveRecipeId(string recipeName, Dictionary<string, long> index)
    {
        var key = recipeName.ToLowerInvariant();
        if (index.TryGetValue(key, out var id))
            return id;

        // partial match fallback
        var match = index.Keys.FirstOrDefault(k => k.Contains(key) || key.Contains(k));
        return match != null ? index[match] : null;
    }

    private record RawSuggestion(
        string DayOfWeek,
        string MealType,
        string RecipeName,
        [property: JsonPropertyName("matchedIngredients")] IReadOnlyList<string>? MatchedIngredients,
        [property: JsonPropertyName("missingIngredients")] IReadOnlyList<string>? MissingIngredients);

    private record RawSuggestionsResponse(IReadOnlyList<RawSuggestion> Suggestions);
}
