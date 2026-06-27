using Feezbow.Application.Features.Digest;
using Feezbow.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Feezbow.Infrastructure.Options;

namespace Feezbow.Infrastructure.BackgroundServices;

public class DigestScheduler(
    IServiceScopeFactory scopeFactory,
    IOptions<AnthropicOptions> options,
    ILogger<DigestScheduler> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.WeeklyDigestEnabled)
        {
            logger.LogInformation("Weekly digest scheduler is disabled via configuration.");
            return;
        }

        logger.LogInformation("Weekly digest scheduler started. Next run: {Next}", NextSundayAt(20, 0));

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = NextSundayAt(20, 0) - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);

            await RunForAllProjectsAsync(stoppingToken);

            // After run, wait at least 1 hour before next iteration to prevent double-fire
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task RunForAllProjectsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        IReadOnlyList<long> projectIds;
        try
        {
            projectIds = await unitOfWork.Projects.GetAllActiveIdsAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enumerate active projects for digest scheduler.");
            return;
        }

        logger.LogInformation("Generating weekly digest for {Count} projects.", projectIds.Count);

        foreach (var projectId in projectIds)
        {
            try
            {
                await mediator.Send(new GenerateDigestQuery(projectId), ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Digest generation failed for project {ProjectId}.", projectId);
            }
        }
    }

    private static DateTimeOffset NextSundayAt(int hour, int minute)
    {
        var now = DateTimeOffset.UtcNow;
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilSunday == 0 && now.Hour >= hour)
            daysUntilSunday = 7;
        return new DateTimeOffset(
            now.Date.AddDays(daysUntilSunday).AddHours(hour).AddMinutes(minute),
            TimeSpan.Zero
        );
    }
}
