using System.Diagnostics;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonderful.App.Services;

namespace Sonderful.App.ViewModels;

public partial class CreatePlanViewModel : ObservableObject
{
    private readonly IApiService _api;

    public CreatePlanViewModel(IApiService api)
    {
        _api = api;
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private List<string> _categories = [.. Enum.GetNames<DTOs.PlanCategory>()];

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private List<string> _counties = DiscoverViewModel.IrishCounties;

    [ObservableProperty]
    private string? _selectedCounty;

    [ObservableProperty]
    private string _capacity = string.Empty;

    [ObservableProperty]
    private DateTime _scheduledDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan _scheduledTime = TimeSpan.FromHours(12);

    [ObservableProperty]
    private string _locationLabel = "No location selected";

    [ObservableProperty]
    private double _latitude;

    [ObservableProperty]
    private double _longitude;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task UseCurrentLocation()
    {
        try
        {
            var location = await Geolocation.Default.GetLastKnownLocationAsync()
                           ?? await Geolocation.Default.GetLocationAsync(
                               new GeolocationRequest(GeolocationAccuracy.Medium,
                                                      TimeSpan.FromSeconds(10)));
            if (location is not null)
            {
                Latitude = location.Latitude;
                Longitude = location.Longitude;
                LocationLabel = "Getting location...";

                // Reverse geocode to a readable name
                var label = await ReverseGeocodeAsync(location.Latitude, location.Longitude);
                if (label is { } loc)
                {
                    LocationLabel = loc.DisplayName;

                    if (SelectedCounty is null && loc.AdminArea is not null)
                    {
                        var match = DiscoverViewModel.IrishCounties
                            .FirstOrDefault(c => loc.AdminArea.Contains(c, StringComparison.OrdinalIgnoreCase));
                        if (match is not null)
                            SelectedCounty = match;
                    }
                }
                else
                {
                    LocationLabel = $"{location.Latitude:F4}, {location.Longitude:F4}";
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Location error", ex.Message, "OK");
        }
    }

    private static async Task<(string DisplayName, string? AdminArea)?> ReverseGeocodeAsync(double lat, double lon)
    {
        if (DeviceInfo.Platform != DevicePlatform.WinUI)
        {
            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(lat, lon);
                var place = placemarks?.FirstOrDefault();
                if (place is not null)
                {
                    var label = place.Locality is not null ? place.Locality : place.AdminArea;
                    return ($"{label}, {place.CountryName}", place.AdminArea);
                }
            }
            catch (Exception ex) when (ex is FeatureNotSupportedException or PermissionException)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Sonderful/1.0");
            var url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lon}&format=json";
            var json = await http.GetStringAsync(url);
            var doc = JsonDocument.Parse(json);
            var addr = doc.RootElement.GetProperty("address");
            var city = addr.TryGetProperty("city", out var c) ? c.GetString()
                     : addr.TryGetProperty("town", out var t) ? t.GetString()
                     : addr.TryGetProperty("village", out var v) ? v.GetString() : null;
            var county = addr.TryGetProperty("county", out var co) ? co.GetString() : null;
            var country = addr.TryGetProperty("country", out var ct) ? ct.GetString() : null;
            var display = city ?? county ?? country;
            if (display is not null)
                return ($"{display}, {country}", county);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            Debug.WriteLine(ex.Message);
        }

        return null;
    }

    [RelayCommand]
    private async Task CreatePlan()
    {
        if (IsBusy)
            return;
        IsBusy = true;
        try
        {
            var scheduledAt = ScheduledDate.Date + ScheduledTime;
            await _api.CreatePlanAsync(
                Title, Description,
                SelectedCategory ?? DTOs.PlanCategory.Other.ToString(),
                int.TryParse(Capacity, out var cap) ? cap : 10,
                Latitude, Longitude, SelectedCounty, scheduledAt);

            await Shell.Current.DisplayAlertAsync("Plan created", $"\"{Title}\" is live!", "OK");
            Title = string.Empty;
            Description = null;
            SelectedCategory = null;
            SelectedCounty = null;
            Capacity = string.Empty;
            LocationLabel = "No location selected";
            await Shell.Current.GoToAsync("//DiscoverPage");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
