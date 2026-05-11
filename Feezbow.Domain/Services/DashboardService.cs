using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Models.Dashboard;

namespace Feezbow.Domain.Services;

public class DashboardService(
    IUnitOfWork unitOfWork,
    IPantryRepository pantryRepository) : IDashboardService
{
    public async Task<HouseholdDashboardData> GetAsync(
        long projectId, long userId, CancellationToken cancellationToken = default)
    {
        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project", projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var now = DateTime.UtcNow;
        var upcomingCutoff = now.AddDays(14);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);
        var thisWeekMonday = now.Date.AddDays(-(((int)now.DayOfWeek + 6) % 7)).ToUniversalTime();

        var unpaidBillsTask = unitOfWork.Bills.GetByProjectAsync(projectId, includePaid: false, cancellationToken);
        var monthBillsTask = unitOfWork.Bills.GetByProjectAndDateRangeAsync(projectId, monthStart, monthEnd, cancellationToken);
        var choresTask = unitOfWork.Chores.GetByProjectAsync(projectId, includeCompleted: false, cancellationToken);
        var mealPlanTask = unitOfWork.MealPlans.GetByProjectAndWeekAsync(projectId, thisWeekMonday, cancellationToken);
        var pantryTask = pantryRepository.GetByProjectAsync(projectId, null, null, 7, cancellationToken);
        var eventsTask = unitOfWork.HouseholdEvents.GetByProjectAndDateRangeAsync(projectId, now, upcomingCutoff, cancellationToken);

        await Task.WhenAll(unpaidBillsTask, monthBillsTask, choresTask, mealPlanTask, pantryTask, eventsTask);

        var unpaidBills = await unpaidBillsTask;

        var overdueBills = unpaidBills
            .Where(b => b.Recurrence == null && b.DueDate < now)
            .OrderBy(b => b.DueDate)
            .ToList();

        var upcomingBills = unpaidBills
            .Where(b => b.Recurrence == null && b.DueDate >= now && b.DueDate <= upcomingCutoff)
            .OrderBy(b => b.DueDate)
            .ToList();

        var chores = await choresTask;
        var activeChores = chores
            .OrderBy(c => c.DueDate == null ? 1 : 0)
            .ThenBy(c => c.DueDate)
            .ToList();

        var pantryItems = await pantryTask;
        var expiringPantry = pantryItems
            .Where(p => p.ExpirationDate.HasValue)
            .OrderBy(p => p.ExpirationDate)
            .ToList();

        return new HouseholdDashboardData(
            overdueBills,
            upcomingBills,
            activeChores,
            await mealPlanTask,
            expiringPantry,
            await monthBillsTask,
            (await eventsTask).OrderBy(e => e.StartDate).ToList(),
            now);
    }
}
