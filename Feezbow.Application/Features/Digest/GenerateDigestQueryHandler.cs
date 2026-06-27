using System.Text;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Models.Dashboard;
using MediatR;

namespace Feezbow.Application.Features.Digest;

public class GenerateDigestQueryHandler(
    IDashboardService dashboardService,
    IAIService aiService,
    ICacheService cacheService,
    ICurrentUserService currentUserService,
    IDigestNotificationService digestNotification)
    : IRequestHandler<GenerateDigestQuery, GenerateDigestQueryResponse>
{
    private const string SystemPrompt = """
        You are the weekly household digest AI for the Feezbow home management app.
        Given a plain-text snapshot of a household's current state, generate a concise,
        friendly weekly digest in Markdown. Focus strictly on what the household needs
        to know or act on this week. Use this structure (skip sections with no items):

        ## 🏠 Weekly Household Digest

        ### ⚠️ Needs Attention
        Overdue bills and pantry items expiring today or tomorrow.

        ### 📅 This Week
        Bills due in the next 7 days, pending chores with due dates, upcoming calendar events.

        ### 🛒 Pantry Watch
        Items expiring in the next 7 days (beyond today/tomorrow).

        ### 🍽️ Meal Plan
        A brief summary of this week's planned meals.

        Rules:
        - Keep it under 400 words. Bullet points only — no long paragraphs.
        - Casual and helpful tone. Never invent data not present in the input.
        - Do not repeat items across sections.
        """;

    public async Task<GenerateDigestQueryResponse> Handle(
        GenerateDigestQuery request,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = CacheKeys.ProjectDigest(request.ProjectId);

        var cached = await cacheService.GetAsync<string>(cacheKey, cancellationToken);
        if (cached is not null)
            return new GenerateDigestQueryResponse(cached, FromCache: true, GeneratedAt: DateTime.UtcNow);

        HouseholdDashboardData data;
        var userId = currentUserService.UserId;
        if (userId.HasValue)
            data = await dashboardService.GetAsync(request.ProjectId, userId.Value, cancellationToken);
        else
            data = await dashboardService.GetSystemAsync(request.ProjectId, cancellationToken);

        var context = BuildContext(data);

        var markdown = await aiService.CompleteAsync(
            SystemPrompt,
            context,
            new AIRequestOptions(
                MaxInputTokens: 8_000,
                MaxOutputTokens: 1_024,
                AllowCaching: true,
                RequiresFeature: "WeeklyDigest"
            ),
            cancellationToken
        );

        await cacheService.SetAsync(cacheKey, markdown, TimeSpan.FromHours(12), cancellationToken);

        await digestNotification.BroadcastAsync(request.ProjectId, markdown, cancellationToken);

        return new GenerateDigestQueryResponse(markdown, FromCache: false, GeneratedAt: DateTime.UtcNow);
    }

    private static string BuildContext(HouseholdDashboardData d)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Date: {d.AsOf:yyyy-MM-dd}");

        if (d.OverdueBills.Count > 0)
        {
            sb.AppendLine($"Overdue bills ({d.OverdueBills.Count}):");
            foreach (var b in d.OverdueBills.Take(10))
                sb.AppendLine($"  - {b.Title} {b.Amount}{b.Currency} due {b.DueDate:yyyy-MM-dd}");
        }

        if (d.UpcomingBills.Count > 0)
        {
            sb.AppendLine($"Upcoming bills ({d.UpcomingBills.Count}):");
            foreach (var b in d.UpcomingBills.Take(10))
                sb.AppendLine($"  - {b.Title} {b.Amount}{b.Currency} due {b.DueDate:yyyy-MM-dd}");
        }

        if (d.ActiveChores.Count > 0)
        {
            sb.AppendLine($"Active chores ({d.ActiveChores.Count}):");
            foreach (var c in d.ActiveChores.Take(10))
            {
                var due = c.DueDate.HasValue ? $" due {c.DueDate.Value:yyyy-MM-dd}" : " (no due date)";
                sb.AppendLine($"  - {c.Title}{due}");
            }
        }

        if (d.ExpiringPantryItems.Count > 0)
        {
            sb.AppendLine($"Pantry items expiring soon ({d.ExpiringPantryItems.Count}):");
            foreach (var p in d.ExpiringPantryItems.Take(10))
                sb.AppendLine($"  - {p.Name} {p.Quantity}{p.Unit ?? ""} expires {p.ExpirationDate!.Value:yyyy-MM-dd}");
        }

        if (d.ThisWeekMealPlan is not null)
        {
            sb.AppendLine("This week's meal plan:");
            foreach (var item in d.ThisWeekMealPlan.Items.OrderBy(i => i.DayOfWeek))
                sb.AppendLine($"  - {item.DayOfWeek}: {item.Recipe?.Name ?? "not set"}");
        }

        if (d.UpcomingEvents.Count > 0)
        {
            sb.AppendLine($"Upcoming events ({d.UpcomingEvents.Count}):");
            foreach (var e in d.UpcomingEvents.Take(5))
                sb.AppendLine($"  - {e.Title} on {e.StartDate:yyyy-MM-dd}");
        }

        return sb.ToString();
    }
}
