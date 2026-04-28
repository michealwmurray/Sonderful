using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonderful.App.Services;

namespace Sonderful.App.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IApiService _api;
    private readonly SessionService _session;

    public ProfileViewModel(IApiService api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string? _bio;

    [ObservableProperty]
    private double _sonderScore;

    [ObservableProperty]
    private string? _photoUrl;

    [ObservableProperty]
    private bool _isEditing;

    public bool IsNotEditing => !IsEditing;

    partial void OnIsEditingChanged(bool value) => OnPropertyChanged(nameof(IsNotEditing));

    [ObservableProperty]
    private string _draftBio = string.Empty;

    public string Initials => string.IsNullOrWhiteSpace(Username)
        ? "?"
        : Username[0].ToString().ToUpperInvariant();

    public bool HasBio => !string.IsNullOrWhiteSpace(Bio);
    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoUrl);
    public bool HasNoPhoto => string.IsNullOrWhiteSpace(PhotoUrl);

    partial void OnUsernameChanged(string value) => OnPropertyChanged(nameof(Initials));
    partial void OnBioChanged(string? value) => OnPropertyChanged(nameof(HasBio));
    partial void OnPhotoUrlChanged(string? value)
    {
        OnPropertyChanged(nameof(HasPhoto));
        OnPropertyChanged(nameof(HasNoPhoto));
    }

    [RelayCommand]
    private async Task Load()
    {
        Username = _session.Username;
        PhotoUrl = _session.PhotoUrl is { } p
            ? (p.StartsWith('/') ? ApiService.BaseUrl + p : p)
            : null;
        try
        {
            SonderScore = await _api.GetUserScoreAsync(_session.UserId);
            Bio = await _api.GetMyBioAsync();
        }
        catch { /* non-critical, don't block the profile page */ }
    }

    [RelayCommand]
    private void StartEditBio()
    {
        DraftBio = Bio ?? string.Empty;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveBio()
    {
        try
        {
            await _api.UpdateBioAsync(string.IsNullOrWhiteSpace(DraftBio) ? null : DraftBio);
            Bio = string.IsNullOrWhiteSpace(DraftBio) ? null : DraftBio;
            IsEditing = false;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void CancelEditBio() => IsEditing = false;

    [RelayCommand]
    private async Task ChangePhoto()
    {
        try
        {
            var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Choose a profile photo"
            });
            var result = photos?.FirstOrDefault();

            if (result is null)
                return;

            await using var stream = await result.OpenReadAsync();
            var contentType = ContentTypeFromFileName(result.FileName) ?? result.ContentType ?? "image/jpeg";
            PhotoUrl = await _api.UploadProfilePhotoAsync(stream, result.FileName, contentType);
            _session.PhotoUrl = PhotoUrl; // keep session in sync so Load() restores it correctly
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private static string? ContentTypeFromFileName(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null
        };

    [RelayCommand]
    private async Task LogOut()
    {
        _session.Clear();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
