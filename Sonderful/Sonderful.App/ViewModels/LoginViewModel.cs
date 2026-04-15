using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonderful.App.Services;

namespace Sonderful.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiService _api;
    private readonly SessionService _session;

    public LoginViewModel(IApiService api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    [ObservableProperty]
    private string _identifier = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task Login()
    {
        if (IsBusy)
            return;
        IsBusy = true;
        try
        {
            var response = await _api.LoginAsync(Identifier, Password);
            _session.SetSession(response);
            await Shell.Current.GoToAsync("//DiscoverPage");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Login failed", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToRegister()
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }
}
