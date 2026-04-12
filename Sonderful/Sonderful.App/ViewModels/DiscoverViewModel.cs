using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonderful.App.DTOs;
using Sonderful.App.Services;

namespace Sonderful.App.ViewModels;

public partial class DiscoverViewModel : ObservableObject
{
    private readonly IApiService _api;
    private bool _myPlansMode;

    public DiscoverViewModel(IApiService api)
    {
        _api = api;
        // Auto-load on first open
        _ = SearchAsync();
    }

    public static readonly List<string> IrishCounties =
    [
        "Antrim", "Armagh", "Carlow", "Cavan", "Clare", "Cork",
        "Derry", "Donegal", "Down", "Dublin", "Fermanagh", "Galway",
        "Kerry", "Kildare", "Kilkenny", "Laois", "Leitrim", "Limerick",
        "Longford", "Louth", "Mayo", "Meath", "Monaghan", "Offaly",
        "Roscommon", "Sligo", "Tipperary", "Tyrone", "Waterford",
        "Westmeath", "Wexford", "Wicklow"
    ];

    [ObservableProperty]
    private ObservableCollection<PlanResponse> _plans = [];

    [ObservableProperty]
    private bool _isBusy;

    // Label shown below the page title describing the active search
    [ObservableProperty]
    private string _searchLabel = "Nearby (75 km)";

    [ObservableProperty]
    private bool _isRefineOpen;

    [ObservableProperty]
    private List<string> _counties = IrishCounties;

    [ObservableProperty]
    private string? _selectedCounty;

    [ObservableProperty]
    private List<string> _categories = [.. Enum.GetNames<PlanCategory>()];

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private string _radiusKm = "75";

    [ObservableProperty]
    private bool _dateFilterEnabled;

    [ObservableProperty]
    private DateTime _filterDate = DateTime.Today;

    [ObservableProperty]
    private bool _isMyPlansMode;

    [RelayCommand]
    private void OpenRefine() => IsRefineOpen = true;

    [RelayCommand]
    private void CloseRefine() => IsRefineOpen = false;

    [RelayCommand]
    private async Task ApplyRefine()
    {
        IsRefineOpen = false;
        await SearchAsync();
    }

    [RelayCommand]
    private async Task ToggleMyPlans()
    {
        _myPlansMode = !_myPlansMode;
        IsMyPlansMode = _myPlansMode;
        await SearchAsync();
    }

    [ObservableProperty]
    private PlanResponse? _selectedPlan;

    partial void OnSelectedPlanChanged(PlanResponse? value)
    {
        if (value is null) return;
        SelectedPlan = null; // reset so tapping the same item again works
        _ = Shell.Current.GoToAsync($"{nameof(Views.PlanDetailPage)}?planId={value.Id}");
    }

    public Task RefreshAsync() => SearchAsync();

    private async Task SearchAsync()
    {
        if (IsBusy)
            return;
        IsBusy = true;
        try
        {
            if (_myPlansMode)
            {
                Plans = new ObservableCollection<PlanResponse>(await _api.GetMyPlansAsync());
                SearchLabel = "My Plans";
                return;
            }

            List<PlanResponse> results;

            if (!string.IsNullOrEmpty(SelectedCounty))
            {
                results = await _api.GetPlansByCountyAsync(
                    SelectedCounty, SelectedCategory,
                    DateFilterEnabled ? FilterDate : null);

                var cat = string.IsNullOrEmpty(SelectedCategory) ? "" : $" · {SelectedCategory}";
                SearchLabel = $"{SelectedCounty}{cat}";
            }
            else
            {
                Location? location = null;
                try
                {
                    location = await Geolocation.Default.GetLastKnownLocationAsync()
                               ?? await Geolocation.Default.GetLocationAsync(
                                   new GeolocationRequest(GeolocationAccuracy.Medium,
                                                          TimeSpan.FromSeconds(8)));
                }
                catch
                {
                    // location permission denied or unavailable
                }

                if (location is null)
                {
                    SearchLabel = "Select a county in Refine ↑";
                    Plans = [];
                    return;
                }

                var radius = double.TryParse(RadiusKm, out var r) ? r : 75;
                results = await _api.GetNearbyPlansAsync(
                    location.Latitude, location.Longitude, radius, SelectedCategory,
                    DateFilterEnabled ? FilterDate : null);

                var cat = string.IsNullOrEmpty(SelectedCategory) ? "" : $" · {SelectedCategory}";
                SearchLabel = $"Nearby ({radius:0} km){cat}";
            }

            Plans = new ObservableCollection<PlanResponse>(results);
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
