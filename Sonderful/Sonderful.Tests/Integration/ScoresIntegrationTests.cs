using System.Net;
using System.Net.Http.Json;
using Sonderful.API.DTOs.Auth;
using Sonderful.API.DTOs.Plans;

namespace Sonderful.Tests.Integration;

public class ScoresIntegrationTests : IntegrationTestBase
{
    public ScoresIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    private static object PastPlanPayload() => new
    {
        title = "Tuesday run",
        description = "5k around the park",
        category = "Sports",
        capacity = 15,
        latitude = 52.2593,
        longitude = -7.1101,
        county = "Waterford",
        scheduledAt = DateTime.UtcNow.AddDays(-1).ToString("o")
    };

    private async Task<(int PlanId, AuthResponse Creator, AuthResponse Attendee)> SetupScoringScenarioAsync()
    {
        var creator = await RegisterAsync();
        Authenticate(creator.Token);
        var plan = (await (await _client.PostAsJsonAsync("api/plans", PastPlanPayload()))
            .Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;

        var attendee = await RegisterAsync();
        Authenticate(attendee.Token);
        await _client.PostAsync($"api/plans/{plan.Id}/rsvp", null);

        return (plan.Id, creator, attendee);
    }

    [Fact]
    public async Task SubmitScore_AsCreatorAfterEvent_Returns204()
    {
        var (planId, creator, attendee) = await SetupScoringScenarioAsync();
        Authenticate(creator.Token);

        var response = await _client.PostAsJsonAsync($"api/plans/{planId}/scores", new
        {
            ratedUserId = attendee.UserId,
            score = 4
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SubmitScore_Unauthenticated_Returns401()
    {
        ClearAuth();

        var response = await _client.PostAsJsonAsync("api/plans/1/scores", new
        {
            ratedUserId = 2,
            score = 3
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
