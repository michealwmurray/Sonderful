using System.Net;
using System.Net.Http.Json;

namespace Sonderful.Tests.Integration;

public class UsersIntegrationTests : IntegrationTestBase
{
    public UsersIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task GetScore_ForNewUser_ReturnsZero()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);

        var response = await _client.GetAsync($"api/users/{auth.UserId}/score");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var score = await response.Content.ReadFromJsonAsync<double>();
        Assert.Equal(0, score);
    }

    [Fact]
    public async Task GetScore_Unauthenticated_Returns401()
    {
        ClearAuth();

        var response = await _client.GetAsync("api/users/1/score");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBio_SavesAndReturns()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);

        var response = await _client.PutAsJsonAsync("api/users/me", new { bio = "Love hiking and coffee" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal("Love hiking and coffee", body.GetProperty("bio").GetString());
    }

    [Fact]
    public async Task GetMyProfile_ReturnsUpdatedBio()
    {
        var auth = await RegisterAsync();
        Authenticate(auth.Token);

        await _client.PutAsJsonAsync("api/users/me", new { bio = "Weekend cyclist" });
        var response = await _client.GetAsync("api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal("Weekend cyclist", body.GetProperty("bio").GetString());
    }
}
