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
                LocationLabel = $"{location.Latitude:F4}, {location.Longitude:F4}";

                // Try reverse geocode to get a readable name
                try
                {
                    var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var place = placemarks?.FirstOrDefault();
                    if (place is not null)
                    {
                        LocationLabel = $"{place.Locality ?? place.AdminArea}, {place.CountryName}";

                        if (SelectedCounty is null && place.AdminArea is not null)
                        {
                            var match = DiscoverViewModel.IrishCounties
                                .FirstOrDefault(c => place.AdminArea.Contains(c, StringComparison.OrdinalIgnoreCase));
                            if (match is not null)
                                SelectedCounty = match;
                        }
                    }
                }
                catch
                {
                    // Geocoding unavailable on this platform, coordinates already set above
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Location error", ex.Message, "OK");
        }
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
