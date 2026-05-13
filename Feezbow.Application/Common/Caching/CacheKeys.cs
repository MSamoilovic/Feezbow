using Feezbow.Domain.Enums;

namespace Feezbow.Application.Common.Caching;

public static class CacheKeys
{
    public static string Board(long boardId) => $"board:{boardId}";
    public static string BoardColumns(long boardId) => $"board:{boardId}:columns";
    public static string BoardTasks(long boardId) => $"board:{boardId}:tasks";
    public static string BoardActivity(long boardId) => $"activity:board:{boardId}";

    public static string Project(long projectId) => $"project:{projectId}";
    public static string ProjectMembers(long projectId) => $"project:{projectId}:members";
    public static string ProjectBoards(long projectId) => $"project:{projectId}:boards";

    public static string UserProfile(long userId) => $"user:{userId}:profile";
    public static string UserProjects(long userId) => $"user:{userId}:projects";
    public static string UserBoards(long userId) => $"user:{userId}:boards";
    public static string UserTasks(long userId) => $"user:{userId}:tasks";

    public static string Task(long taskId) => $"task:{taskId}";
    public static string TaskComments(long taskId) => $"task:{taskId}:comments";

    public static string Column(long columnId) => $"column:{columnId}";

    public static string BoardLabels(long boardId) => $"board:{boardId}:labels";

    public static string HouseholdProfile(long projectId) => $"household:{projectId}:profile";
    public static string HouseholdMembers(long projectId) => $"household:{projectId}:members";

    public static string ProjectChores(long projectId) => $"project:{projectId}:chores";
    public static string Chore(long choreId) => $"chore:{choreId}";

    public static string ProjectShoppingLists(long projectId) => $"project:{projectId}:shopping-lists";
    public static string ShoppingList(long listId) => $"shopping-list:{listId}";

    public static string ProjectBills(long projectId) => $"project:{projectId}:bills";
    public static string Bill(long billId) => $"bill:{billId}";

    public static string ProjectBudgetSummary(long projectId, DateTime from, DateTime to, int upcomingDays) =>
        $"project:{projectId}:budget:summary:{from:yyyyMMdd}:{to:yyyyMMdd}:u{upcomingDays}";

    public static string ProjectBudgetSummaryPrefix(long projectId) =>
        $"project:{projectId}:budget:";

    public static string ProjectBudgetTimeline(long projectId, DateTime from, DateTime to) =>
        $"project:{projectId}:budget:timeline:{from:yyyyMMdd}:{to:yyyyMMdd}";

    public static string UserBudgetSummary(long userId, DateTime from, DateTime to) =>
        $"user:{userId}:budget:{from:yyyyMMdd}:{to:yyyyMMdd}";

    public static string UserBudgetPrefix(long userId) =>
        $"user:{userId}:budget:";

    public static string AttachmentsByOwner(AttachmentOwnerType ownerType, long ownerId) =>
        $"attachments:{ownerType.ToString().ToLowerInvariant()}:{ownerId}";

    public static string ProjectMealPlansPrefix(long projectId) =>
        $"project:{projectId}:meal-plans:";

    public static string ProjectMealPlanByWeek(long projectId, DateTime weekStart) =>
        $"project:{projectId}:meal-plans:week:{weekStart:yyyyMMdd}";

    public static string ProjectMealPlansRecent(long projectId, int count) =>
        $"project:{projectId}:meal-plans:recent:{count}";

    public static string ProjectRecipesPrefix(long projectId) =>
        $"project:{projectId}:recipes:";

    public static string ProjectRecipesList(long projectId, int skip, int take) =>
        $"project:{projectId}:recipes:list:s{skip}:t{take}";

    public static string Recipe(long recipeId) =>
        $"recipe:{recipeId}";

    public static string ProjectPantryPrefix(long projectId) =>
        $"project:{projectId}:pantry:";

    public static string ProjectPantryList(long projectId, string? search, string? location, int? expiringWithinDays) =>
        $"project:{projectId}:pantry:list:s={search ?? "*"}:l={location ?? "*"}:e={expiringWithinDays?.ToString() ?? "*"}";

    public static string PantryItem(long pantryItemId) =>
        $"pantry-item:{pantryItemId}";

    public static string ProjectCalendar(long projectId, DateTime from, DateTime to) =>
        $"project:{projectId}:calendar:{from:yyyyMMdd}:{to:yyyyMMdd}";

    public static string ProjectCalendarPrefix(long projectId) =>
        $"project:{projectId}:calendar:";

    public static string ProjectHouseholdEvents(long projectId) =>
        $"project:{projectId}:household-events";

    public static string HouseholdEvent(long eventId) =>
        $"household-event:{eventId}";

    public static string ProjectDashboard(long projectId) =>
        $"project:{projectId}:dashboard";

    public static string UserNotifications(long userId, bool unreadOnly, int page, int pageSize) =>
        $"user:{userId}:notifications:u={unreadOnly}:p={page}:s={pageSize}";

    public static string UserNotificationsPrefix(long userId) =>
        $"user:{userId}:notifications:";

    public static string UserUnreadCount(long userId) =>
        $"user:{userId}:notifications:unread-count";
}
