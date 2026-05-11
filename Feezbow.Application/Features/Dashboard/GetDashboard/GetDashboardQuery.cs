using MediatR;

namespace Feezbow.Application.Features.Dashboard.GetDashboard;

public record GetDashboardQuery(long ProjectId) : IRequest<GetDashboardQueryResponse>;
