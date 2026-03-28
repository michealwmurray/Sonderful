using Moq;
using Sonderful.API.DTOs.Comments;
using Sonderful.API.Models;
using Sonderful.API.Repositories;
using Sonderful.API.Services;

namespace Sonderful.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _sut = new CommentService(_commentRepoMock.Object, _userRepoMock.Object, _planRepoMock.Object);
    }

    private static Comment MakeComment(int id = 1, string content = "Hello!", string username = "aoife") => new()
    {
        Id = id,
        Content = content,
        PlanId = 10,
        UserId = 1,
        CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        User = new User { Id = 1, Username = username }
    };

    [Fact]
    public async Task GetComments_ReturnsMappedResponses()
    {
        // Arrange
        var comments = new List<Comment>
        {
            MakeComment(1, "First!",  "aoife"),
            MakeComment(2, "Second!", "oisin")
        };
        comments[1].User = new User { Id = 2, Username = "oisin" };

        _commentRepoMock.Setup(r => r.GetByPlanAsync(10)).ReturnsAsync(comments);

        // Act
        var results = (await _sut.GetCommentsAsync(10)).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("First!", results[0].Content);
        Assert.Equal("aoife", results[0].Username);
        Assert.Equal("Second!", results[1].Content);
        Assert.Equal("oisin", results[1].Username);
    }

    [Fact]
    public async Task GetComments_EmptyPlan_ReturnsEmptyList()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByPlanAsync(99))
                        .ReturnsAsync(new List<Comment>());

        // Act
        var results = await _sut.GetCommentsAsync(99);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task AddComment_MapsFieldsCorrectly()
    {
        // Arrange
        Comment? captured = null;
        var created = MakeComment();

        _commentRepoMock.Setup(r => r.CreateAsync(It.IsAny<Comment>()))
                        .Callback<Comment>(c => captured = c)
                        .ReturnsAsync(created);
        _userRepoMock.Setup(r => r.GetByIdAsync(1))
                     .ReturnsAsync(new User { Id = 1, Username = "aoife" });

        // Act
        await _sut.AddCommentAsync(10, 1, new AddCommentRequest { Content = "Hello!" });

        // Assert
        Assert.NotNull(captured);
        Assert.Equal("Hello!", captured!.Content);
        Assert.Equal(10, captured.PlanId);
        Assert.Equal(1, captured.UserId);
    }

    [Fact]
    public async Task AddComment_ReturnsMappedResponse()
    {
        // Arrange
        var created = MakeComment(id: 5, content: "Nice plan!");

        _commentRepoMock.Setup(r => r.CreateAsync(It.IsAny<Comment>())).ReturnsAsync(created);
        _userRepoMock.Setup(r => r.GetByIdAsync(1))
                     .ReturnsAsync(new User { Id = 1, Username = "aoife" });

        // Act
        var response = await _sut.AddCommentAsync(10, 1, new AddCommentRequest { Content = "Nice plan!" });

        // Assert
        Assert.Equal(5, response.Id);
        Assert.Equal("Nice plan!", response.Content);
        Assert.Equal("aoife", response.Username);
        Assert.Equal(created.CreatedAt, response.CreatedAt);
    }

    [Fact]
    public async Task DeleteComment_ByAuthor_CallsDeleteAsync()
    {
        // Arrange - UserId = 1 is the comment author
        var comment = MakeComment(id: 7);
        _commentRepoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(comment);
        _planRepoMock.Setup(r => r.GetByIdAsync(comment.PlanId))
                     .ReturnsAsync(new Plan { Id = comment.PlanId, CreatorId = 99 });

        // Act
        await _sut.DeleteCommentAsync(7, requestingUserId: 1);

        // Assert
        _commentRepoMock.Verify(r => r.DeleteAsync(7), Times.Once);
    }

    [Fact]
    public async Task DeleteComment_ByPlanCreator_CallsDeleteAsync()
    {
        // Arrange - comment author is userId=1, plan creator is userId=5
        var comment = MakeComment(id: 8);
        _commentRepoMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(comment);
        _planRepoMock.Setup(r => r.GetByIdAsync(comment.PlanId))
                     .ReturnsAsync(new Plan { Id = comment.PlanId, CreatorId = 5 });

        // Act - requestingUserId=5 is plan creator, not comment author
        await _sut.DeleteCommentAsync(8, requestingUserId: 5);

        // Assert
        _commentRepoMock.Verify(r => r.DeleteAsync(8), Times.Once);
    }

    [Fact]
    public async Task DeleteComment_ByOtherUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange - author is userId=1, plan creator is 99
        var comment = MakeComment(id: 9);
        _commentRepoMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(comment);
        _planRepoMock.Setup(r => r.GetByIdAsync(comment.PlanId))
                     .ReturnsAsync(new Plan { Id = comment.PlanId, CreatorId = 99 });

        // Act & Assert - requestingUserId=42 is neither author nor plan creator
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.DeleteCommentAsync(9, requestingUserId: 42));
    }

    [Fact]
    public async Task DeleteComment_WhenNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.DeleteCommentAsync(999, requestingUserId: 1));
    }
}
