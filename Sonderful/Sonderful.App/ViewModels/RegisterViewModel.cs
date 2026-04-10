using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonderful.App.Services;

namespace Sonderful.App.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IApiService _api;
    private readonly SessionService _session;

    public RegisterViewModel(IApiService api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task Register()
    {
        if (IsBusy)
            return;
        IsBusy = true;
        try
        {
            var response = await _api.RegisterAsync(Username, Email, Password);
            _session.SetSession(response);
            await Shell.Current.GoToAsync("//DiscoverPage");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Registration failed", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToLogin()
    {
        await Shell.Current.GoToAsync("..");
    }
}
