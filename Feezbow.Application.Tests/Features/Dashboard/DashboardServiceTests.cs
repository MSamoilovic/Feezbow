using Moq;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Services;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Application.Tests.Features.Dashboard;

public class DashboardServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IBillRepository> _billRepoMock = new();
    private readonly Mock<IHouseholdChoreRepository> _choreRepoMock = new();
    private readonly Mock<IMealPlanRepository> _mealPlanRepoMock = new();
    private readonly Mock<IHouseholdEventRepository> _eventRepoMock = new();
    private readonly Mock<IPantryRepository> _pantryRepoMock = new();

    public DashboardServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Bills).Returns(_billRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Chores).Returns(_choreRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.MealPlans).Returns(_mealPlanRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.HouseholdEvents).Returns(_eventRepoMock.Object);

        SetupDefaultEmptyReturns();
    }

    private DashboardService CreateService() =>
        new(_unitOfWorkMock.Object, _pantryRepoMock.Object);

    private void SetupDefaultEmptyReturns()
    {
        _billRepoMock
            .Setup(r => r.GetByProjectAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _billRepoMock
            .Setup(r => r.GetByProjectAndDateRangeAsync(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _choreRepoMock
            .Setup(r => r.GetByProjectAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mealPlanRepoMock
            .Setup(r => r.GetByProjectAndWeekAsync(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MealPlan?)null);
        _pantryRepoMock
            .Setup(r => r.GetByProjectAsync(It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _eventRepoMock
            .Setup(r => r.GetByProjectAndDateRangeAsync(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    private static Project BuildProjectWithMember(long projectId, long userId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), userId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers),
            new List<ProjectMember> { member });
        return project;
    }

    private static Bill BuildBill(long projectId, DateTime dueDate, RecurrenceRule? recurrence = null)
    {
        var bill = ApplicationTestUtils.CreateInstanceWithoutConstructor<Bill>();
        ApplicationTestUtils.SetPrivatePropertyValue(bill, nameof(Bill.ProjectId), projectId);
        ApplicationTestUtils.SetPrivatePropertyValue(bill, nameof(Bill.Title), "Test bill");
        ApplicationTestUtils.SetPrivatePropertyValue(bill, nameof(Bill.Amount), 100m);
        ApplicationTestUtils.SetPrivatePropertyValue(bill, nameof(Bill.Currency), "EUR");
        ApplicationTestUtils.SetPrivatePropertyValue(bill, nameof(Bill.DueDate), dueDate);
        ApplicationTestUtils.SetPrivatePropertyValue(bill, nameof(Bill.Recurrence), recurrence);
        return bill;
    }

    private static HouseholdChore BuildChore(long projectId, DateTime? dueDate = null) =>
        HouseholdChore.Create(projectId, "Test chore", createdBy: 1L, dueDate: dueDate);

    private static PantryItem BuildPantryItem(long projectId, DateTime? expirationDate) =>
        PantryItem.Create(projectId, "Milk", 1m, "L", null, expirationDate, null, createdBy: 1L);

    [Fact]
    public async Task GetAsync_WhenProjectNotFound_ThrowsNotFoundException()
    {
        _projectRepoMock
            .Setup(r => r.GetProjectWithMembersAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetAsync(99L, 1L, CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_WhenUserNotMember_ThrowsAccessDeniedException()
    {
        const long projectId = 1L;

        _projectRepoMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId: 999L));

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateService().GetAsync(projectId, userId: 10L, CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_WhenValidRequest_ReturnsData()
    {
        const long projectId = 1L;
        const long userId = 10L;

        _projectRepoMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));

        var data = await CreateService().GetAsync(projectId, userId, CancellationToken.None);

        Assert.NotNull(data);
        Assert.Empty(data.OverdueBills);
        Assert.Empty(data.UpcomingBills);
        Assert.Empty(data.ActiveChores);
        Assert.Null(data.ThisWeekMealPlan);
        Assert.Empty(data.ExpiringPantryItems);
        Assert.Empty(data.UpcomingEvents);
    }

    [Fact]
    public async Task GetAsync_BillWithPastDueDate_AppearsInOverdueBills()
    {
        const long projectId = 1L;
        const long userId = 10L;
        var overdueBill = BuildBill(projectId, DateTime.UtcNow.AddDays(-3));

        _projectRepoMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));
        _billRepoMock
            .Setup(r => r.GetByProjectAsync(projectId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([overdueBill]);

        var data = await CreateService().GetAsync(projectId, userId, CancellationToken.None);

        Assert.Single(data.OverdueBills);
        Assert.Equal(overdueBill.Id, data.OverdueBills[0].Id);
        Assert.Empty(data.UpcomingBills);
    }

    [Fact]
    public async Task GetAsync_BillDueWithin14Days_AppearsInUpcomingBills()
    {
        const long projectId = 1L;
        const long userId = 10L;
        var upcomingBill = BuildBill(projectId, DateTime.UtcNow.AddDays(5));

        _projectRepoMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));
        _billRepoMock
            .Setup(r => r.GetByProjectAsync(projectId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([upcomingBill]);

        var data = await CreateService().GetAsync(projectId, userId, CancellationToken.None);

        Assert.Single(data.UpcomingBills);
        Assert.Equal(upcomingBill.Id, data.UpcomingBills[0].Id);
        Assert.Empty(data.OverdueBills);
    }

    [Fact]
    public async Task GetAsync_RecurringTemplateBill_ExcludedFromOverdueAndUpcoming()
    {
        const long projectId = 1L;
        const long userId = 10L;
        var template = BuildBill(projectId, DateTime.UtcNow.AddDays(-1),
            recurrence: RecurrenceRule.Create(RecurrenceFrequency.Monthly));

        _projectRepoMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));
        _billRepoMock
            .Setup(r => r.GetByProjectAsync(projectId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([template]);

        var data = await CreateService().GetAsync(projectId, userId, CancellationToken.None);

        Assert.Empty(data.OverdueBills);
        Assert.Empty(data.UpcomingBills);
    }

    [Fact]
    public async Task GetAsync_WhenNoMealPlanThisWeek_ThisWeekMealPlanIsNull()
    {
        const long projectId = 1L;
        const long userId = 10L;

        _projectRepoMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));
        _mealPlanRepoMock
            .Setup(r => r.GetByProjectAndWeekAsync(projectId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MealPlan?)null);

        var data = await CreateService().GetAsync(projectId, userId, CancellationToken.None);

        Assert.Null(data.ThisWeekMealPlan);
    }

    [Fact]
    public async Task GetAsync_PantryItemWithoutExpirationDate_ExcludedFromAlerts()
    {
        const long projectId = 1L;
        const long userId = 10L;
        var withExpiry = BuildPantryItem(projectId, DateTime.UtcNow.AddDays(3));
        var withoutExpiry = BuildPantryItem(projectId, expirationDate: null);

        _projectRepoMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));
        _pantryRepoMock
            .Setup(r => r.GetByProjectAsync(projectId, null, null, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync([withExpiry, withoutExpiry]);

        var data = await CreateService().GetAsync(projectId, userId, CancellationToken.None);

        Assert.Single(data.ExpiringPantryItems);
        Assert.Equal(withExpiry.Id, data.ExpiringPantryItems[0].Id);
    }

    [Fact]
    public async Task GetAsync_ActiveChores_SortedByDueDateNullsLast()
    {
        const long projectId = 1L;
        const long userId = 10L;
        var choreWithoutDate = BuildChore(projectId, dueDate: null);
        var choreWithDate = BuildChore(projectId, dueDate: DateTime.UtcNow.AddDays(2));

        _projectRepoMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));
        _choreRepoMock
            .Setup(r => r.GetByProjectAsync(projectId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([choreWithoutDate, choreWithDate]);

        var data = await CreateService().GetAsync(projectId, userId, CancellationToken.None);

        Assert.Equal(2, data.ActiveChores.Count);
        Assert.Equal(choreWithDate.Id, data.ActiveChores[0].Id);
        Assert.Equal(choreWithoutDate.Id, data.ActiveChores[1].Id);
    }
}
