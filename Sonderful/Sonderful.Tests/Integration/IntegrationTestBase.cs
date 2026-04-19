using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sonderful.API.DTOs.Auth;

namespace Sonderful.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<IntegrationTestFactory>
{
    protected readonly HttpClient _client;

    protected static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected IntegrationTestBase(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Helper to register a new user and return the auth response
    protected async Task<AuthResponse> RegisterAsync(
        string? email = null,
        string password = "Password123!",
        string? username = null)
    {
        email ??= $"user_{Guid.NewGuid():N}@test.com";
        username ??= $"user_{Guid.NewGuid():N}"[..16];

        var response = await _client.PostAsJsonAsync("api/auth/register",
            new { username, email, password });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOpts))!;
    }

    protected void Authenticate(string token) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    protected void ClearAuth() =>
        _client.DefaultRequestHeaders.Authorization = null;
}
