using MediatR;
using Feezbow.Domain.Events.Bill_Events;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Events.Bill;

internal class BillPaidNotificationHandler(INotificationDomainService notificationService)
    : INotificationHandler<BillPaidEvent>
{
    public Task Handle(BillPaidEvent notification, CancellationToken cancellationToken)
        => notificationService.CreateForProjectMembersAsync(
            notification.ProjectId,
            "bill.paid",
            "Bill Paid",
            $"A bill of {notification.Amount:F2} has been paid.",
            notification.PaidBy,
            cancellationToken);
}
