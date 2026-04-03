using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sonderful.API.DTOs.Auth;

namespace Sonderful.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<IntegrationTestFactory>
{
    protected readonly HttpClient Client;

    protected static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected IntegrationTestBase(IntegrationTestFactory factory)
    {
        Client = factory.CreateClient();
    }

    // Helper to register a new user and return the auth response
    protected async Task<AuthResponse> RegisterAsync(
        string? email = null,
        string password = "Password123!",
        string? username = null)
    {
        email ??= $"user_{Guid.NewGuid():N}@test.com";
        username ??= $"user_{Guid.NewGuid():N}"[..16];

        var response = await Client.PostAsJsonAsync("api/auth/register",
            new { username, email, password });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOpts))!;
    }

    protected void Authenticate(string token) =>
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    protected void ClearAuth() =>
        Client.DefaultRequestHeaders.Authorization = null;
}
