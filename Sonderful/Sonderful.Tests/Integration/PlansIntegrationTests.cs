using System.Net;
using System.Net.Http.Json;
using Sonderful.API.DTOs.Plans;

namespace Sonderful.Tests.Integration;

public class PlansIntegrationTests : IntegrationTestBase
{
    public PlansIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    private static object NewPlanPayload(string title = "Test Plan") => new
    {
        title,
        description = "Integration test plan",
        category = "Coffee",
        capacity = 10,
        latitude = 53.3498,
        longitude = -6.2603,
        county = "Dublin",
        scheduledAt = DateTime.UtcNow.AddDays(7).ToString("o")
    };

    [Fact]
    public async Task CreatePlan_Authenticated_Returns201WithPlan()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);

        var response = await _client.PostAsJsonAsync("api/plans", NewPlanPayload("Morning Coffee"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var plan = (await response.Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;
        Assert.Equal("Morning Coffee", plan.Title);
        Assert.True(plan.Id > 0);
    }

    [Fact]
    public async Task CreatePlan_Unauthenticated_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("api/plans", NewPlanPayload());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPlan_ExistingId_Returns200WithPlan()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);

        var created = (await (await _client.PostAsJsonAsync("api/plans", NewPlanPayload()))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;

        var response = await _client.GetAsync($"api/plans/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var plan = (await response.Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;
        Assert.Equal(created.Id, plan.Id);
        Assert.Equal(created.Title, plan.Title);
    }

    [Fact]
    public async Task GetPlan_NonExistingId_Returns404()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);

        var response = await _client.GetAsync("api/plans/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePlan_ByOwner_Returns204()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);

        var created = (await (await _client.PostAsJsonAsync("api/plans", NewPlanPayload()))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;

        var response = await _client.DeleteAsync($"api/plans/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeletePlan_ByNonOwner_Returns403()
    {
        // Owner creates the plan
        var owner = await RegisterAsync();
        Authenticate(owner.Token);
        var created = (await (await _client.PostAsJsonAsync("api/plans", NewPlanPayload()))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;

        // Different user attempts to delete
        var other = await RegisterAsync();
        Authenticate(other.Token);

        var response = await _client.DeleteAsync($"api/plans/{created.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Rsvp_WhenSpaceAvailable_Returns204()
    {
        var owner = await RegisterAsync();
        Authenticate(owner.Token);
        var created = (await (await _client.PostAsJsonAsync("api/plans", NewPlanPayload()))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;

        // Different user RSVPs
        var attendee = await RegisterAsync();
        Authenticate(attendee.Token);

        var response = await _client.PostAsync($"api/plans/{created.Id}/rsvp", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Rsvp_WhenAlreadyRsvped_Returns409()
    {
        var owner = await RegisterAsync();
        Authenticate(owner.Token);
        var created = (await (await _client.PostAsJsonAsync("api/plans", NewPlanPayload()))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;

        var attendee = await RegisterAsync();
        Authenticate(attendee.Token);

        // First RSVP succeeds
        await _client.PostAsync($"api/plans/{created.Id}/rsvp", null);

        // Second RSVP should fail
        var response = await _client.PostAsync($"api/plans/{created.Id}/rsvp", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetMyPlans_ReturnsOnlyUserPlans()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);

        // Create two plans as this user
        await _client.PostAsJsonAsync("api/plans", NewPlanPayload("My Plan A"));
        await _client.PostAsJsonAsync("api/plans", NewPlanPayload("My Plan B"));

        var response = await _client.GetAsync("api/plans/mine");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var plans = (await response.Content.ReadFromJsonAsync<List<PlanResponse>>(_jsonOpts))!;
        Assert.True(plans.Count >= 2);
        Assert.All(plans, p => Assert.Equal(auth.UserId, p.CreatorId));
    }

    [Fact]
    public async Task CancelRsvp_AfterRsvping_Returns204()
    {
        var owner = await RegisterAsync();
        Authenticate(owner.Token);
        var created = (await (await _client.PostAsJsonAsync("api/plans", NewPlanPayload()))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;

        var attendee = await RegisterAsync();
        Authenticate(attendee.Token);
        // RSVP first, then cancel
        await _client.PostAsync($"api/plans/{created.Id}/rsvp", null);

        var response = await _client.DeleteAsync($"api/plans/{created.Id}/rsvp");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CancelRsvp_WhenNotRsvped_Returns409()
    {
        var owner = await RegisterAsync();
        Authenticate(owner.Token);
        var created = (await (await _client.PostAsJsonAsync("api/plans", NewPlanPayload()))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;

        var other = await RegisterAsync();
        Authenticate(other.Token);

        var response = await _client.DeleteAsync($"api/plans/{created.Id}/rsvp");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePlan_ByOwner_ReturnsUpdatedPlan()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);
        var created = (await (await _client.PostAsJsonAsync("api/plans", NewPlanPayload("Original Title")))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;

        var response = await _client.PutAsJsonAsync($"api/plans/{created.Id}", new
        {
            title = "Updated Title",
            description = "Something changed",
            category = "Coffee",
            capacity = 8,
            scheduledAt = DateTime.UtcNow.AddDays(14).ToString("o")
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;
        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal(8, updated.Capacity);
    }

    [Fact]
    public async Task UpdatePlan_ByNonOwner_Returns403()
    {
        // Owner creates the plan
        var owner = await RegisterAsync();
        Authenticate(owner.Token);
        var created = (await (await _client.PostAsJsonAsync("api/plans", NewPlanPayload()))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;

        // Different user attempts to update
        var other = await RegisterAsync();
        Authenticate(other.Token);

        var response = await _client.PutAsJsonAsync($"api/plans/{created.Id}", new
        {
            title = "Hijacked",
            description = "Not allowed",
            category = "Coffee",
            capacity = 5,
            scheduledAt = DateTime.UtcNow.AddDays(7).ToString("o")
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetNearby_ByCounty_ReturnsPlanInCounty()
    {
        var owner = await RegisterAsync();
        Authenticate(owner.Token);

        await _client.PostAsJsonAsync("api/plans", new
        {
            title = "Kerry hike",
            description = "Up the Reeks",
            category = "Walking",
            capacity = 12,
            latitude = 52.0214,
            longitude = -9.5639,
            county = "Kerry",
            scheduledAt = DateTime.UtcNow.AddDays(5).ToString("o")
        });

        var other = await RegisterAsync();
        Authenticate(other.Token);

        var response = await _client.GetAsync("api/plans?county=Kerry");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var plans = (await response.Content.ReadFromJsonAsync<List<PlanResponse>>(_jsonOpts))!;
        Assert.Contains(plans, p => p.Title == "Kerry hike");
    }

    [Fact]
    public async Task GetNearby_WithoutCoordinatesOrCounty_Returns400()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);

        var response = await _client.GetAsync("api/plans");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
