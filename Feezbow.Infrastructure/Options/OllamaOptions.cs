namespace Feezbow.Infrastructure.Options;

/// <summary>
/// Konekcijske opcije za lokalni Ollama provider (npr. Gemma). Koristi se u dev-u
/// kada je "Ai:Provider" = "Ollama". Feature-flag kontrole su iste kao kod Gemini-ja
/// jer je AI podsistem provider-agnostičan.
/// </summary>
public class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; init; } = "http://localhost:11434";
    public string DefaultModel { get; init; } = "gemma4:e4b";
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
