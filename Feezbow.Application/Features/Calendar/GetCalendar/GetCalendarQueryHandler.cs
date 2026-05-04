using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Calendar.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using MediatR;

namespace Feezbow.Application.Features.Calendar.GetCalendar;

public class GetCalendarQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetCalendarQuery, GetCalendarQueryResponse>
{
    public async Task<GetCalendarQueryResponse> Handle(
        GetCalendarQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var from = request.From.Date;
        var to = request.To.Date.AddDays(1); // exclusive upper bound

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectCalendar(request.ProjectId, from, to),
            () => AggregateAsync(request.ProjectId, from, to, cancellationToken),
            CacheExpiration.Short,
            cancellationToken);

        return new GetCalendarQueryResponse(cached ?? []);
    }

    private async Task<List<CalendarItemDto>> AggregateAsync(
        long projectId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var billsTask = unitOfWork.Bills.GetByProjectAndDateRangeAsync(projectId, from, to, cancellationToken);
        var choresTask = unitOfWork.Chores.GetByProjectAndDateRangeAsync(projectId, from, to, cancellationToken);
        var mealPlansTask = unitOfWork.MealPlans.GetByProjectAndDateRangeAsync(projectId, from, to, cancellationToken);
        var eventsTask = unitOfWork.HouseholdEvents.GetByProjectAndDateRangeAsync(projectId, from, to, cancellationToken);

        await Task.WhenAll(billsTask, choresTask, mealPlansTask, eventsTask);

        var items = new List<CalendarItemDto>();

        foreach (var bill in await billsTask)
            items.Add(CalendarItemDto.FromBill(bill));

        foreach (var chore in await choresTask)
            items.Add(CalendarItemDto.FromChore(chore));

        foreach (var plan in await mealPlansTask)
        {
            foreach (var item in plan.Items)
            {
                var itemDate = plan.WeekStart.AddDays(((int)item.DayOfWeek + 6) % 7);
                if (itemDate >= from && itemDate < to)
                    items.Add(CalendarItemDto.FromMealItem(plan, item));
            }
        }

        foreach (var ev in await eventsTask)
            items.Add(CalendarItemDto.FromHouseholdEvent(ev));

        return [.. items.OrderBy(i => i.Date).ThenBy(i => i.Type)];
    }
}
