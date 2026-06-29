namespace Feezbow.Domain.Enums;

public enum AIInsightType
{
    BillsOverdue,    // Više računa kasni — potencijalni finansijski problem
    PantryDepleting, // Više stavki ispod minimuma istovremeno
    BudgetOverrun,   // Potrošnja premašuje budžet za tekući period
    ChoresBacklog,   // Nakupljeni neobavljeni taskovi domaćinstva
    MealPlanGap,     // Plan obroka nije popunjen za narednih N dana
    UnusualSpending  // Spending pattern koji odskače od historijskog prosjeka
}
