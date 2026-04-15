using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonderful.App.Services;

namespace Sonderful.App.ViewModels;

// Location fields are not editable after creation
public partial class EditPlanViewModel : ObservableObject, IQueryAttributable
{
    private readonly IApiService _api;
    private int _planId;

    public EditPlanViewModel(IApiService api)
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
    private string _capacity = string.Empty;

    [ObservableProperty]
    private DateTime _scheduledDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan _scheduledTime = TimeSpan.FromHours(12);

    [ObservableProperty]
    private bool _isBusy;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("planId", out var id))
            _ = LoadAsync(int.Parse(id.ToString()!));
    }

    private async Task LoadAsync(int planId)
    {
        _planId = planId;
        IsBusy = true;
        try
        {
            var plan = await _api.GetPlanAsync(planId);
            Title = plan.Title;
            Description = plan.Description;
            SelectedCategory = plan.Category.ToString();
            Capacity = plan.Capacity.ToString();
            ScheduledDate = plan.ScheduledAt.Date;
            ScheduledTime = plan.ScheduledAt.TimeOfDay;
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

    [RelayCommand]
    private static async Task Cancel() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task SavePlan()
    {
        if (IsBusy)
            return;
        IsBusy = true;
        try
        {
            var scheduledAt = ScheduledDate.Date + ScheduledTime;
            await _api.UpdatePlanAsync(
                _planId, Title, Description,
                SelectedCategory ?? DTOs.PlanCategory.Other.ToString(),
                int.TryParse(Capacity, out var cap) ? cap : 10,
                scheduledAt);

            await Shell.Current.GoToAsync("..");
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
