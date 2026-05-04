using Feezbow.Application.Features.Calendar.Common;

namespace Feezbow.Application.Features.Calendar.GetCalendar;

public record GetCalendarQueryResponse(IReadOnlyList<CalendarItemDto> Items);
