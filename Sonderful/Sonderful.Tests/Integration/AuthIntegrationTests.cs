using System.Net;
using System.Net.Http.Json;
using Sonderful.API.DTOs.Auth;

namespace Sonderful.Tests.Integration;

public class AuthIntegrationTests : IntegrationTestBase
{
    public AuthIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ValidRequest_Returns201WithTokenAndUserId()
    {
        var response = await _client.PostAsJsonAsync("api/auth/register", new
        {
            username = "saoirse",
            email = $"new_{Guid.NewGuid():N}@test.com",
            password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = (await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOpts))!;
        Assert.NotEmpty(body.Token);
        Assert.True(body.UserId > 0);
        Assert.Equal("saoirse", body.Username);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";

        // First registration succeeds
        await RegisterAsync(email: email);

        // Second with the same email should fail
        var response = await _client.PostAsJsonAsync("api/auth/register", new
        {
            username = "padraig",
            email,
            password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_CorrectCredentials_Returns200WithToken()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        const string password = "Password123!";
        await RegisterAsync(email: email, password: password);

        var response = await _client.PostAsJsonAsync("api/auth/login",
            new { identifier = email, password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = (await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOpts))!;
        Assert.NotEmpty(body.Token);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"wp_{Guid.NewGuid():N}@test.com";
        await RegisterAsync(email: email, password: "CorrectPassword!");

        var response = await _client.PostAsJsonAsync("api/auth/login",
            new { identifier = email, password = "WrongPassword!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("api/auth/login",
            new { email = "nobody@nowhere.com", password = "anything" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("api/plans/mine");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
