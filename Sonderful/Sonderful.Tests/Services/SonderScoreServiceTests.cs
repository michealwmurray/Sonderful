using Moq;
using Sonderful.API.Models;
using Sonderful.API.Repositories;
using Sonderful.API.Services;

namespace Sonderful.Tests.Services;

public class SonderScoreServiceTests
{
    private readonly Mock<ISonderScoreRepository> _repoMock = new();
    private readonly Mock<IPlanRepository> _planMock = new();
    private readonly Mock<IRsvpRepository> _rsvpMock = new();
    private readonly SonderScoreService _sut;

    // A plan in the past whose creator is userId=2
    private static Plan PastPlan(int creatorId = 2) => new()
    {
        Id = 1,
        CreatorId = creatorId,
        ScheduledAt = DateTime.UtcNow.AddDays(-1)
    };

    public SonderScoreServiceTests()
    {
        _sut = new SonderScoreService(_repoMock.Object, _planMock.Object, _rsvpMock.Object);
    }

    [Fact]
    public async Task SubmitScore_ValidInput_CreatesScoreWithCorrectFields()
    {
        // Arrange
        SonderScore? captured = null;
        _planMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PastPlan(creatorId: 2));
        _rsvpMock.Setup(r => r.GetAsync(1, 3)).ReturnsAsync(new Rsvp { PlanId = 1, UserId = 3 });
        _repoMock.Setup(r => r.ExistsAsync(1, 2, 3)).ReturnsAsync(false);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<SonderScore>()))
                 .Callback<SonderScore>(s => captured = s)
                 .ReturnsAsync(new SonderScore());

        // Act
        await _sut.SubmitScoreAsync(planId: 1, raterId: 2, ratedUserId: 3, score: 4);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal(1, captured!.PlanId);
        Assert.Equal(2, captured.RaterId);
        Assert.Equal(3, captured.RatedUserId);
        Assert.Equal(4, captured.Score);
    }

    [Fact]
    public async Task SubmitScore_DuplicateScore_ThrowsInvalidOperationException()
    {
        // Arrange
        _planMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PastPlan(creatorId: 2));
        _rsvpMock.Setup(r => r.GetAsync(1, 3)).ReturnsAsync(new Rsvp { PlanId = 1, UserId = 3 });
        _repoMock.Setup(r => r.ExistsAsync(1, 2, 3)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitScoreAsync(planId: 1, raterId: 2, ratedUserId: 3, score: 5));
    }

    [Fact]
    public async Task SubmitScore_NotCreator_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _planMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PastPlan(creatorId: 99));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.SubmitScoreAsync(planId: 1, raterId: 2, ratedUserId: 3, score: 5));
    }

    [Fact]
    public async Task SubmitScore_PlanNotPast_ThrowsInvalidOperationException()
    {
        // Arrange
        var futurePlan = new Plan { Id = 1, CreatorId = 2, ScheduledAt = DateTime.UtcNow.AddDays(1) };
        _planMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(futurePlan);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitScoreAsync(planId: 1, raterId: 2, ratedUserId: 3, score: 5));
    }

    [Fact]
    public async Task SubmitScore_UserDidNotAttend_ThrowsInvalidOperationException()
    {
        // Arrange
        _planMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PastPlan(creatorId: 2));
        _rsvpMock.Setup(r => r.GetAsync(1, 3)).ReturnsAsync((Rsvp?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitScoreAsync(planId: 1, raterId: 2, ratedUserId: 3, score: 5));
    }

    [Fact]
    public async Task SubmitScore_RatingYourself_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitScoreAsync(planId: 1, raterId: 2, ratedUserId: 2, score: 5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task SubmitScore_ScoreOutOfRange_ThrowsArgumentOutOfRangeException(int score)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.SubmitScoreAsync(planId: 1, raterId: 2, ratedUserId: 3, score: score));
    }

    [Fact]
    public async Task GetAverageForUser_ReturnsValueFromRepository()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAverageForUserAsync(7)).ReturnsAsync(4.2);

        // Act
        var result = await _sut.GetAverageForUserAsync(7);

        // Assert
        Assert.Equal(4.2, result);
    }

    [Fact]
    public async Task GetAverageForUser_NoScoresYet_ReturnsZero()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAverageForUserAsync(99)).ReturnsAsync(0.0);

        // Act
        var result = await _sut.GetAverageForUserAsync(99);

        // Assert
        Assert.Equal(0.0, result);
    }
}
