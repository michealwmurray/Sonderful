using System.Net;
using System.Net.Http.Json;
using Sonderful.API.DTOs.Comments;
using Sonderful.API.DTOs.Plans;

namespace Sonderful.Tests.Integration;

public class CommentsIntegrationTests : IntegrationTestBase
{
    public CommentsIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    private static object NewPlanPayload() => new
    {
        title = "Park meetup",
        description = "Casual afternoon in the park",
        category = "Coffee",
        capacity = 20,
        latitude = 53.3498,
        longitude = -6.2603,
        county = "Dublin",
        scheduledAt = DateTime.UtcNow.AddDays(3).ToString("o")
    };

    private async Task<int> CreatePlanAsync()
    {
        var plan = (await (await _client.PostAsJsonAsync("api/plans", NewPlanPayload()))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;
        return plan.Id;
    }

    [Fact]
    public async Task PostComment_Authenticated_ReturnsComment()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);
        var planId = await CreatePlanAsync();

        var response = await _client.PostAsJsonAsync(
            $"api/plans/{planId}/comments",
            new { content = "Looking forward to this one!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comment = (await response.Content.ReadFromJsonAsync<CommentResponse>(_jsonOpts))!;
        Assert.Equal("Looking forward to this one!", comment.Content);
        Assert.Equal(auth.UserId, comment.UserId);
    }

    [Fact]
    public async Task PostComment_Unauthenticated_Returns401()
    {
        ClearAuth();

        var response = await _client.PostAsJsonAsync(
            "api/plans/1/comments",
            new { content = "Should not work" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetComments_AfterPosting_ReturnsComment()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);
        var planId = await CreatePlanAsync();

        await _client.PostAsJsonAsync($"api/plans/{planId}/comments",
            new { content = "See you all there" });

        var response = await _client.GetAsync($"api/plans/{planId}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comments = (await response.Content.ReadFromJsonAsync<List<CommentResponse>>(_jsonOpts))!;
        Assert.Contains(comments, c => c.Content == "See you all there");
    }

    [Fact]
    public async Task DeleteComment_ByAuthor_Returns204()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);
        var planId = await CreatePlanAsync();

        var comment = (await (await _client.PostAsJsonAsync(
            $"api/plans/{planId}/comments",
            new { content = "Sounds great, I'll be there" }))
            .Content.ReadFromJsonAsync<CommentResponse>(_jsonOpts))!;

        var response = await _client.DeleteAsync($"api/plans/{planId}/comments/{comment.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_ByOtherUser_Returns403()
    {
        // Owner posts the comment
        var owner = await RegisterAsync();
        Authenticate(owner.Token);
        var planId = await CreatePlanAsync();

        var comment = (await (await _client.PostAsJsonAsync(
            $"api/plans/{planId}/comments",
            new { content = "Can't wait!" }))
            .Content.ReadFromJsonAsync<CommentResponse>(_jsonOpts))!;

        // Different user attempts to delete it
        var other = await RegisterAsync();
        Authenticate(other.Token);

        var response = await _client.DeleteAsync($"api/plans/{planId}/comments/{comment.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_NonExisting_Returns404()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);
        var planId = await CreatePlanAsync();

        var response = await _client.DeleteAsync($"api/plans/{planId}/comments/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
