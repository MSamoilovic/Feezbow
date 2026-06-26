namespace Feezbow.Application.Common.Interfaces;

public interface IDigestNotificationService
{
    Task BroadcastAsync(long projectId, string markdown, CancellationToken cancellationToken = default);
}
