using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Dashboard.GetDashboard;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Models.Dashboard;

namespace Feezbow.Application.Tests.Features.Dashboard;

public class GetDashboardQueryHandlerTests
{
    private readonly Mock<IDashboardService> _dashboardServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public GetDashboardQueryHandlerTests()
    {
        _cacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<GetDashboardQueryResponse>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<GetDashboardQueryResponse>>, TimeSpan?, CancellationToken>(
                (_, factory, _, _) => factory());
    }

    private GetDashboardQueryHandler CreateHandler() => new(
        _dashboardServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static HouseholdDashboardData EmptyData() =>
        new([], [], [], null, [], [], [], DateTime.UtcNow);

    [Fact]
    public async Task Handle_WhenAuthenticated_ReturnsResponse()
    {
        const long projectId = 1L;
        const long userId = 10L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _dashboardServiceMock
            .Setup(s => s.GetAsync(projectId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmptyData());

        var response = await CreateHandler().Handle(new GetDashboardQuery(projectId), CancellationToken.None);

        Assert.NotNull(response);
        Assert.Empty(response.OverdueBills);
        Assert.Empty(response.UpcomingBills);
        Assert.Empty(response.ActiveChores);
        Assert.Null(response.ThisWeekMealPlan);
        Assert.Empty(response.ExpiringPantryItems);
        Assert.Empty(response.UpcomingEvents);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ThrowsUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new GetDashboardQuery(1L), CancellationToken.None));

        _dashboardServiceMock.Verify(
            s => s.GetAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsNotFoundException_PropagatesException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _dashboardServiceMock
            .Setup(s => s.GetAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Project", 99L));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new GetDashboardQuery(99L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsAccessDeniedException_PropagatesException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _dashboardServiceMock
            .Setup(s => s.GetAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccessDeniedException("Not a member"));

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new GetDashboardQuery(1L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UsesCacheWithProjectDashboardKey()
    {
        const long projectId = 42L;
        const long userId = 10L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _dashboardServiceMock
            .Setup(s => s.GetAsync(projectId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmptyData());

        await CreateHandler().Handle(new GetDashboardQuery(projectId), CancellationToken.None);

        _cacheServiceMock.Verify(c => c.GetOrSetAsync(
            CacheKeys.ProjectDashboard(projectId),
            It.IsAny<Func<Task<GetDashboardQueryResponse>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
