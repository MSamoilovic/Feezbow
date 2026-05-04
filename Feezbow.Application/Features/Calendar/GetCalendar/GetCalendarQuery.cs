using Feezbow.Application.Features.Calendar.Common;
using MediatR;

namespace Feezbow.Application.Features.Calendar.GetCalendar;

public record GetCalendarQuery(long ProjectId, DateTime From, DateTime To)
    : IRequest<GetCalendarQueryResponse>;
