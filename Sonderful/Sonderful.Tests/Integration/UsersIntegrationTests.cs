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
}
