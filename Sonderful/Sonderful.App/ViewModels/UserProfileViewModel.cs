using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonderful.App.DTOs;
using Sonderful.App.Services;

namespace Sonderful.App.ViewModels;

public partial class UserProfileViewModel : ObservableObject, IQueryAttributable
{
    private readonly IApiService _api;

    public UserProfileViewModel(IApiService api)
    {
        _api = api;
    }

    [ObservableProperty]
    private PublicUserProfile? _profile;

    [ObservableProperty]
    private bool _isBusy;

    public bool HasPhoto => !string.IsNullOrWhiteSpace(Profile?.PhotoUrl);
    public bool HasNoPhoto => string.IsNullOrWhiteSpace(Profile?.PhotoUrl);
    public bool HasBio => !string.IsNullOrWhiteSpace(Profile?.Bio);
    public string Initials => string.IsNullOrWhiteSpace(Profile?.Username) ? "?" : Profile.Username[0].ToString().ToUpperInvariant();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("userId", out var id))
            _ = LoadAsync(int.Parse(id.ToString()!));
    }

    private async Task LoadAsync(int userId)
    {
        IsBusy = true;
        try
        {
            Profile = await _api.GetUserProfileAsync(userId);
            OnPropertyChanged(nameof(HasPhoto));
            OnPropertyChanged(nameof(HasNoPhoto));
            OnPropertyChanged(nameof(HasBio));
            OnPropertyChanged(nameof(Initials));
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
    private static async Task GoBack() => await Shell.Current.GoToAsync("..");
}
