using Moq;
using Sonderful.API.DTOs.Plans;
using Sonderful.API.Models;
using Sonderful.API.Repositories;
using Sonderful.API.Services;

namespace Sonderful.Tests.Services;

public class PlanServiceTests
{
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly Mock<IRsvpRepository> _rsvpRepoMock = new();
    private readonly PlanService _sut;

    public PlanServiceTests()
    {
        _sut = new PlanService(_planRepoMock.Object, _rsvpRepoMock.Object);
    }

    private static Plan MakePlan(int id = 1, int capacity = 10, int rsvpCount = 0) => new()
    {
        Id = id,
        Title = "Test Plan",
        Description = "A test",
        Category = PlanCategory.Coffee,
        Capacity = capacity,
        RsvpCount = rsvpCount,
        Latitude = 53.3498,
        Longitude = -6.2603,
        ScheduledAt = DateTime.UtcNow.AddDays(1),
        CreatorId = 1,
        Creator = new User { Id = 1, Username = "micheal" }
    };

    [Fact]
    public async Task CreatePlan_MapsPlanFieldsFromRequest()
    {
        // Arrange
        var plan = MakePlan();
        Plan? captured = null;

        _planRepoMock.Setup(r => r.CreateAsync(It.IsAny<Plan>()))
                     .Callback<Plan>(p => captured = p)
                     .ReturnsAsync(plan);
        _planRepoMock.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);

        // Act
        await _sut.CreatePlanAsync(1, new CreatePlanRequest
        {
            Title = "Test Plan",
            Description = "A test",
            Category = PlanCategory.Coffee,
            Capacity = 10,
            Latitude = 53.3498,
            Longitude = -6.2603,
            ScheduledAt = plan.ScheduledAt
        });

        // Assert
        Assert.NotNull(captured);
        Assert.Equal("Test Plan", captured!.Title);
        Assert.Equal(1, captured.CreatorId);
        Assert.Equal(PlanCategory.Coffee, captured.Category);
        Assert.Equal(53.3498, captured.Latitude);
    }

    [Fact]
    public async Task CreatePlan_ReturnsMappedPlanResponse()
    {
        // Arrange
        var plan = MakePlan();
        _planRepoMock.Setup(r => r.CreateAsync(It.IsAny<Plan>())).ReturnsAsync(plan);
        _planRepoMock.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);

        // Act
        var response = await _sut.CreatePlanAsync(1, new CreatePlanRequest
        {
            Title = "Test Plan",
            Category = PlanCategory.Coffee,
            Capacity = 10,
            Latitude = 53.3498,
            Longitude = -6.2603,
            ScheduledAt = plan.ScheduledAt
        });

        // Assert
        Assert.Equal(plan.Id, response.Id);
        Assert.Equal("Test Plan", response.Title);
        Assert.Equal("micheal", response.CreatorUsername);
    }

    [Fact]
    public async Task GetPlan_ExistingId_ReturnsPlanResponse()
    {
        // Arrange
        var plan = MakePlan(id: 5);
        _planRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(plan);

        // Act
        var response = await _sut.GetPlanAsync(5, 0);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(5, response!.Id);
        Assert.Equal("micheal", response.CreatorUsername);
    }

    [Fact]
    public async Task GetPlan_NonExistingId_ReturnsNull()
    {
        // Arrange
        _planRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Plan?)null);

        // Act
        var response = await _sut.GetPlanAsync(99, 0);

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task GetNearbyPlans_ReturnsMappedResponses()
    {
        // Arrange
        var plans = new List<Plan> { MakePlan(1), MakePlan(2) };
        plans[1].Title = "Second Plan";

        _planRepoMock.Setup(r => r.GetNearbyAsync(53.3498, -6.2603, 5.0, null, null, 0))
                     .ReturnsAsync(plans);

        // Act
        var results = (await _sut.GetNearbyPlansAsync(53.3498, -6.2603, 5.0, null, null, 0)).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Test Plan", results[0].Title);
        Assert.Equal("Second Plan", results[1].Title);
    }

    [Fact]
    public async Task Rsvp_WhenNotAlreadyRsvped_CreatesRsvp()
    {
        // Arrange
        var plan = MakePlan(capacity: 10, rsvpCount: 0);
        _planRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _rsvpRepoMock.Setup(r => r.GetAsync(1, 2)).ReturnsAsync((Rsvp?)null);
        _rsvpRepoMock.Setup(r => r.CreateAsync(It.IsAny<Rsvp>())).ReturnsAsync(new Rsvp());
        _planRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Plan>())).ReturnsAsync(plan);

        // Act
        await _sut.RsvpAsync(1, 2);

        // Assert
        _rsvpRepoMock.Verify(
            r => r.CreateAsync(It.Is<Rsvp>(rv => rv.PlanId == 1 && rv.UserId == 2)),
            Times.Once);
    }

    [Fact]
    public async Task Rsvp_WhenAlreadyRsvped_ThrowsInvalidOperationException()
    {
        // Arrange - user already has an RSVP
        var plan = MakePlan();
        _planRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _rsvpRepoMock.Setup(r => r.GetAsync(1, 2))
                     .ReturnsAsync(new Rsvp { PlanId = 1, UserId = 2 });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RsvpAsync(1, 2));
    }

    [Fact]
    public async Task Rsvp_WhenAtCapacity_ThrowsInvalidOperationException()
    {
        // Arrange - plan is full
        var plan = MakePlan(capacity: 5, rsvpCount: 5);
        _planRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _rsvpRepoMock.Setup(r => r.GetAsync(1, 2)).ReturnsAsync((Rsvp?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RsvpAsync(1, 2));
    }

    [Fact]
    public async Task CancelRsvp_WhenRsvpExists_CallsDelete()
    {
        // Arrange
        var plan = MakePlan(rsvpCount: 3);
        _planRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _rsvpRepoMock.Setup(r => r.GetAsync(1, 2))
                     .ReturnsAsync(new Rsvp { PlanId = 1, UserId = 2 });
        _planRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Plan>())).ReturnsAsync(plan);

        // Act
        await _sut.CancelRsvpAsync(1, 2);

        // Assert
        _rsvpRepoMock.Verify(r => r.DeleteAsync(1, 2), Times.Once);
    }

    [Fact]
    public async Task CancelRsvp_WhenNoRsvpExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var plan = MakePlan();
        _planRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _rsvpRepoMock.Setup(r => r.GetAsync(1, 2)).ReturnsAsync((Rsvp?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CancelRsvpAsync(1, 2));
    }

    [Fact]
    public async Task UpdatePlan_WhenCreator_UpdatesFieldsAndReturnsResponse()
    {
        // Arrange
        var plan = MakePlan(id: 1);
        var updated = MakePlan(id: 1);
        updated.Title = "Updated Title";
        updated.Category = PlanCategory.Dining;
        updated.Capacity = 20;

        _planRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _planRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Plan>())).ReturnsAsync(updated);
        // Second GetByIdAsync call returns the updated version
        _planRepoMock.SetupSequence(r => r.GetByIdAsync(1))
                     .ReturnsAsync(plan)
                     .ReturnsAsync(updated);

        var request = new UpdatePlanRequest
        {
            Title = "Updated Title",
            Category = PlanCategory.Dining,
            Capacity = 20,
            ScheduledAt = plan.ScheduledAt
        };

        // Act
        var response = await _sut.UpdatePlanAsync(1, userId: 1, request);

        // Assert
        Assert.Equal("Updated Title", response.Title);
        Assert.Equal(PlanCategory.Dining, response.Category);
        Assert.Equal(20, response.Capacity);
        _planRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Plan>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePlan_WhenNotCreator_ThrowsUnauthorizedAccessException()
    {
        // Arrange - CreatorId = 1, calling as userId 99
        var plan = MakePlan(id: 1);
        _planRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.UpdatePlanAsync(1, userId: 99, new UpdatePlanRequest
            {
                Title = "Hack",
                Category = PlanCategory.Coffee,
                Capacity = 5,
                ScheduledAt = DateTime.UtcNow.AddDays(1)
            }));
    }

    [Fact]
    public async Task UpdatePlan_WhenNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _planRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Plan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.UpdatePlanAsync(99, userId: 1, new UpdatePlanRequest
            {
                Title = "Ghost",
                Category = PlanCategory.Coffee,
                Capacity = 5,
                ScheduledAt = DateTime.UtcNow.AddDays(1)
            }));
    }

    [Fact]
    public async Task DeletePlan_WhenCreator_CallsDeleteAsync()
    {
        // Arrange
        var plan = MakePlan(id: 3);
        _planRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(plan);

        // Act
        await _sut.DeletePlanAsync(3, userId: 1);

        // Assert
        _planRepoMock.Verify(r => r.DeleteAsync(3), Times.Once);
    }

    [Fact]
    public async Task DeletePlan_WhenNotCreator_ThrowsUnauthorizedAccessException()
    {
        // Arrange - CreatorId = 1, calling as userId 99
        var plan = MakePlan(id: 3);
        _planRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(plan);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.DeletePlanAsync(3, userId: 99));
    }

    [Fact]
    public async Task DeletePlan_WhenNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _planRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Plan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.DeletePlanAsync(99, userId: 1));
    }

    [Fact]
    public async Task GetMyPlans_ReturnsMappedResponses()
    {
        // Arrange
        var plans = new List<Plan> { MakePlan(1), MakePlan(2) };
        plans[1].Title = "My Second Plan";
        _planRepoMock.Setup(r => r.GetByCreatorAsync(1)).ReturnsAsync(plans);

        // Act
        var results = (await _sut.GetMyPlansAsync(1)).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Test Plan", results[0].Title);
        Assert.Equal("My Second Plan", results[1].Title);
    }

    [Fact]
    public async Task GetMyPlans_NoPlans_ReturnsEmpty()
    {
        // Arrange
        _planRepoMock.Setup(r => r.GetByCreatorAsync(1)).ReturnsAsync(new List<Plan>());

        // Act
        var results = await _sut.GetMyPlansAsync(1);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAttendees_ReturnsMappedUserResponses()
    {
        // Arrange
        var rsvps = new List<Rsvp>
        {
            new() { PlanId = 1, UserId = 10, User = new User { Id = 10, Username = "aoife" } },
            new() { PlanId = 1, UserId = 11, User = new User { Id = 11, Username = "oisin" } }
        };
        _rsvpRepoMock.Setup(r => r.GetByPlanAsync(1)).ReturnsAsync(rsvps);

        // Act
        var results = (await _sut.GetAttendeesAsync(1)).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal(10, results[0].Id);
        Assert.Equal("aoife", results[0].Username);
        Assert.Equal(11, results[1].Id);
        Assert.Equal("oisin", results[1].Username);
    }

    [Fact]
    public async Task GetAttendees_NoPlanAttendees_ReturnsEmpty()
    {
        // Arrange
        _rsvpRepoMock.Setup(r => r.GetByPlanAsync(1)).ReturnsAsync(new List<Rsvp>());

        // Act
        var results = await _sut.GetAttendeesAsync(1);

        // Assert
        Assert.Empty(results);
    }
}
