namespace Feezbow.Infrastructure.Options;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; init; } = string.Empty; // placeholder — set in .env (Gemini__ApiKey)
    public string DefaultModel { get; init; } = "gemini-2.5-flash";
    public int MaxTokensDefault { get; init; } = 1_024;
    public bool Enabled { get; init; } = true;
    public bool RecipeParserEnabled { get; init; } = true;
    public bool TaskParserEnabled { get; init; } = true;
    public bool PantryReceiptParserEnabled { get; init; } = true;
    public bool BillOcrEnabled { get; init; } = true;
    public bool WeeklyDigestEnabled { get; init; } = true;
    public bool MealPlanSuggestEnabled { get; init; } = true;
    public bool ShoppingListCleanupEnabled { get; init; } = true;

    public double MinConfidenceScore { get; init; } = 0.7;
}
