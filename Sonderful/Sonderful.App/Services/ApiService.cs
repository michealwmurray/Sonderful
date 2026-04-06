using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sonderful.App.DTOs;

namespace Sonderful.App.Services;

public class ApiService : IApiService
{
    // To be updated if the API runs on a different port
    public const string BaseUrl = "http://localhost:5082";

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;
    private readonly SessionService _session;

    public ApiService(HttpClient http, SessionService session)
    {
        _http = http;
        _session = session;
        _http.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password });
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOpts))!;
    }

    public async Task<AuthResponse> RegisterAsync(string username, string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", new { username, email, password });
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOpts))!;
    }

    public async Task<List<PlanResponse>> GetNearbyPlansAsync(double lat, double lon, double radiusKm, string? category, DateTime? date = null)
    {
        SetAuthHeader();
        var url = $"api/plans?lat={lat}&lng={lon}&radius={radiusKm}";
        if (!string.IsNullOrEmpty(category))
            url += $"&category={category}";
        if (date.HasValue)
            url += $"&date={date.Value:yyyy-MM-dd}";
        var response = await _http.GetAsync(url);
        await EnsureSuccessAsync(response);
        var plans = (await response.Content.ReadFromJsonAsync<List<PlanResponse>>(_jsonOpts))!;
        foreach (var plan in plans)
            ResolvePhotoUrls(plan);
        return plans;
    }

    public async Task<PlanResponse> GetPlanAsync(int planId)
    {
        SetAuthHeader();
        var response = await _http.GetAsync($"api/plans/{planId}");
        await EnsureSuccessAsync(response);
        var plan = (await response.Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;
        ResolvePhotoUrls(plan);
        return plan;
    }

    public async Task<List<PlanResponse>> GetPlansByCountyAsync(string county, string? category, DateTime? date = null)
    {
        SetAuthHeader();
        var url = $"api/plans?county={Uri.EscapeDataString(county)}";
        if (!string.IsNullOrEmpty(category))
            url += $"&category={category}";
        if (date.HasValue)
            url += $"&date={date.Value:yyyy-MM-dd}";
        var response = await _http.GetAsync(url);
        await EnsureSuccessAsync(response);
        var plans = (await response.Content.ReadFromJsonAsync<List<PlanResponse>>(_jsonOpts))!;
        foreach (var plan in plans)
            ResolvePhotoUrls(plan);
        return plans;
    }

    public async Task<List<PlanResponse>> GetMyPlansAsync()
    {
        SetAuthHeader();
        var response = await _http.GetAsync("api/plans/mine");
        await EnsureSuccessAsync(response);
        var plans = (await response.Content.ReadFromJsonAsync<List<PlanResponse>>(_jsonOpts))!;
        foreach (var plan in plans)
            ResolvePhotoUrls(plan);
        return plans;
    }

    public async Task<PlanResponse> CreatePlanAsync(string title, string? description, string category,
        int capacity, double lat, double lon, string? county, DateTime scheduledAt)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/plans", new
        {
            title,
            description,
            category,
            capacity,
            latitude = lat,
            longitude = lon,
            county,
            scheduledAt
        });
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;
    }

    public async Task<PlanResponse> UpdatePlanAsync(int planId, string title, string? description,
        string category, int capacity, DateTime scheduledAt)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/plans/{planId}", new
        {
            title,
            description,
            category,
            capacity,
            scheduledAt
        });
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<PlanResponse>(_jsonOpts))!;
    }

    public async Task DeletePlanAsync(int planId)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/plans/{planId}");
        await EnsureSuccessAsync(response);
    }

    public async Task RsvpAsync(int planId)
    {
        SetAuthHeader();
        var response = await _http.PostAsync($"api/plans/{planId}/rsvp", null);
        await EnsureSuccessAsync(response);
    }

    public async Task CancelRsvpAsync(int planId)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/plans/{planId}/rsvp");
        await EnsureSuccessAsync(response);
    }

    public async Task<List<CommentResponse>> GetCommentsAsync(int planId)
    {
        SetAuthHeader();
        var response = await _http.GetAsync($"api/plans/{planId}/comments");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<List<CommentResponse>>(_jsonOpts))!;
    }

    public async Task<CommentResponse> AddCommentAsync(int planId, string content)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/plans/{planId}/comments", new { content });
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<CommentResponse>(_jsonOpts))!;
    }

    public async Task DeleteCommentAsync(int planId, int commentId)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/plans/{planId}/comments/{commentId}");
        await EnsureSuccessAsync(response);
    }

    public async Task<List<UserResponse>> GetAttendeesAsync(int planId)
    {
        SetAuthHeader();
        var response = await _http.GetAsync($"api/plans/{planId}/attendees");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<List<UserResponse>>(_jsonOpts))!;
    }

    public async Task<double> GetUserScoreAsync(int userId)
    {
        SetAuthHeader();
        var response = await _http.GetAsync($"api/users/{userId}/score");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<double>();
    }

    public async Task SubmitScoreAsync(int planId, int ratedUserId, int score)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/plans/{planId}/scores",
            new { ratedUserId, score });
        await EnsureSuccessAsync(response);
    }

    public async Task<string> UploadProfilePhotoAsync(Stream photoStream, string fileName, string contentType)
    {
        SetAuthHeader();
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(photoStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "photo", fileName);
        var response = await _http.PostAsync("api/users/me/photo", content);
        await EnsureSuccessAsync(response);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return AbsoluteUrl(doc.RootElement.GetProperty("photoUrl").GetString()!);
    }

    public async Task<(int photoId, string url)> UploadPlanPhotoAsync(int planId, Stream photoStream, string fileName, string contentType)
    {
        SetAuthHeader();
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(photoStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "photo", fileName);
        var response = await _http.PostAsync($"api/plans/{planId}/photos", content);
        await EnsureSuccessAsync(response);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        int id = doc.RootElement.GetProperty("photoId").GetInt32();
        string url = AbsoluteUrl(doc.RootElement.GetProperty("url").GetString()!);
        return (id, url);
    }

    public async Task DeletePlanPhotoAsync(int planId, int photoId)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/plans/{planId}/photos/{photoId}");
        await EnsureSuccessAsync(response);
    }

    private void SetAuthHeader() =>
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _session.Token);

    // Converts a server-relative path to an absolute URL
    private static string AbsoluteUrl(string relativeOrAbsolute) =>
        relativeOrAbsolute.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? relativeOrAbsolute
            : BaseUrl + relativeOrAbsolute;

    private static void ResolvePhotoUrls(PlanResponse plan)
    {
        foreach (var p in plan.Photos)
            p.Url = AbsoluteUrl(p.Url);
        if (plan.CreatorPhotoUrl is not null)
            plan.CreatorPhotoUrl = AbsoluteUrl(plan.CreatorPhotoUrl);
    }

    // Throws with the best available error message from the response body
    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                foreach (var key in new[] { "message", "title" })
                {
                    if (doc.RootElement.TryGetProperty(key, out var val))
                    {
                        var text = val.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                            throw new InvalidOperationException(text);
                    }
                }
            }
            catch (JsonException) { /* not JSON */ }

            if (!string.IsNullOrWhiteSpace(body))
                throw new InvalidOperationException(body);
        }

        throw new InvalidOperationException($"Request failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
    }
}
