using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonderful.App.DTOs;
using Sonderful.App.Services;

namespace Sonderful.App.ViewModels;

public partial class RateViewModel : ObservableObject, IQueryAttributable
{
    private readonly IApiService _api;
    private readonly SessionService _session;
    private int _planId;

    public RateViewModel(IApiService api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    [ObservableProperty]
    private ObservableCollection<AttendeeRating> _attendees = [];

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
            var users = await _api.GetAttendeesAsync(planId);
            // Exclude the current user, you can't rate yourself
            var others = users.Where(u => u.Id != _session.UserId)
                              .Select(u => new AttendeeRating(u))
                              .ToList();
            Attendees = new ObservableCollection<AttendeeRating>(others);
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
    private async Task SubmitRatings()
    {
        IsBusy = true;
        var errors = new List<string>();
        try
        {
            foreach (var attendee in Attendees.Where(a => a.SelectedScore > 0))
            {
                try
                {
                    await _api.SubmitScoreAsync(_planId, attendee.User.Id, attendee.SelectedScore);
                }
                catch (Exception ex)
                {
                    errors.Add($"{attendee.User.Username}: {ex.Message}");
                }
            }

            if (errors.Count > 0)
                await Shell.Current.DisplayAlertAsync("Some ratings failed", string.Join("\n", errors), "OK");
            else
                await Shell.Current.DisplayAlertAsync("Done", "Ratings submitted!", "OK");

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

// SelectedScore of 0 means not yet rated, skipped on submit
public partial class AttendeeRating : ObservableObject
{
    public UserResponse User { get; }

    public List<int> Scores { get; } = [1, 2, 3, 4, 5];

    public bool HasPhoto => !string.IsNullOrWhiteSpace(User.PhotoUrl);
    public bool HasNoPhoto => string.IsNullOrWhiteSpace(User.PhotoUrl);
    public string Initial => string.IsNullOrWhiteSpace(User.Username) ? "?" : User.Username[0].ToString().ToUpperInvariant();

    [ObservableProperty]
    private int _selectedScore;

    public AttendeeRating(UserResponse user)
    {
        User = user;
    }
}
