using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonderful.App.DTOs;
using Sonderful.App.Services;

namespace Sonderful.App.ViewModels;

public partial class PlanDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IApiService _api;
    private readonly SessionService _session;
    private bool _isRsvped;

    public PlanDetailViewModel(IApiService api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    [ObservableProperty]
    private PlanResponse? _plan;

    [ObservableProperty]
    private ObservableCollection<CommentResponse> _comments = [];

    [ObservableProperty]
    private ObservableCollection<PlanPhotoItem> _photos = [];

    [ObservableProperty]
    private string _newComment = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _rsvpButtonText = "Join";

    public string RsvpLabel => Plan is null ? string.Empty : $"{Plan.RsvpCount}/{Plan.Capacity}";
    public bool IsCreator => Plan is not null && Plan.CreatorId == _session.UserId;
    public bool IsPast => Plan is not null && Plan.ScheduledAt < DateTime.Now;
    public bool IsCreatorAndPast => IsCreator && IsPast;
    public bool HasPhotos => Photos.Count > 0;
    public bool HasNoPhotos => Photos.Count == 0;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("planId", out var id))
            _ = LoadAsync(int.Parse(id.ToString()!));
    }

    private async Task LoadAsync(int planId)
    {
        IsBusy = true;
        try
        {
            Plan = await _api.GetPlanAsync(planId);
            _isRsvped = Plan.IsRsvped;
            RsvpButtonText = _isRsvped ? "Leave" : "Join";
            OnPropertyChanged(nameof(RsvpLabel));
            OnPropertyChanged(nameof(IsCreator));
            OnPropertyChanged(nameof(IsPast));
            OnPropertyChanged(nameof(IsCreatorAndPast));

            Photos = new ObservableCollection<PlanPhotoItem>(
                Plan.Photos.Select(p => new PlanPhotoItem(p.Id, p.Url)));
            OnPropertyChanged(nameof(HasPhotos));
            OnPropertyChanged(nameof(HasNoPhotos));

            var list = await _api.GetCommentsAsync(planId);
            Comments = new ObservableCollection<CommentResponse>(list);
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

    [RelayCommand]
    private async Task ToggleRsvp()
    {
        if (Plan is null || IsBusy)
            return;
        IsBusy = true;
        try
        {
            if (!_isRsvped)
            {
                await _api.RsvpAsync(Plan.Id);
                _isRsvped = true;
                RsvpButtonText = "Leave";
                Plan.RsvpCount++;
            }
            else
            {
                await _api.CancelRsvpAsync(Plan.Id);
                _isRsvped = false;
                RsvpButtonText = "Join";
                Plan.RsvpCount--;
            }
            // Notify bindings on Plan.RsvpCount, Plan.GoingLabel, Plan.SpotsLeft, Plan.IsFull
            OnPropertyChanged(nameof(Plan));
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
    private async Task RateAttendees()
    {
        if (Plan is null)
            return;
        await Shell.Current.GoToAsync($"{nameof(Views.RatePage)}?planId={Plan.Id}");
    }

    [RelayCommand]
    private async Task EditPlan()
    {
        if (Plan is null)
            return;
        await Shell.Current.GoToAsync(
            $"{nameof(Views.EditPlanPage)}?planId={Plan.Id}");
    }

    [RelayCommand]
    private async Task DeletePlan()
    {
        if (Plan is null)
            return;
        var confirm = await Shell.Current.DisplayAlertAsync(
            "Delete Plan", $"Delete \"{Plan.Title}\"? This cannot be undone.", "Delete", "Cancel");
        if (!confirm)
            return;

        IsBusy = true;
        try
        {
            await _api.DeletePlanAsync(Plan.Id);
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

    private static string? ContentTypeFromFileName(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null
        };

    [RelayCommand]
    private async Task ViewPhoto(PlanPhotoItem photo) =>
        await Shell.Current.Navigation.PushModalAsync(new Views.PhotoViewerPage(photo.Url));

    public bool CanDeleteComment(CommentResponse comment) =>
        comment.UserId == _session.UserId || IsCreator;

    [RelayCommand]
    private async Task DeleteComment(CommentResponse comment)
    {
        if (Plan is null)
            return;
        try
        {
            await _api.DeleteCommentAsync(Plan.Id, comment.Id);
            Comments.Remove(comment);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task PostComment()
    {
        if (string.IsNullOrWhiteSpace(NewComment) || Plan is null || IsBusy)
            return;
        IsBusy = true;
        try
        {
            var comment = await _api.AddCommentAsync(Plan.Id, NewComment);
            Comments.Add(comment);
            NewComment = string.Empty;
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
    private async Task UploadPhoto()
    {
        if (Plan is null)
            return;
        try
        {
            var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Add a photo to this plan"
            });
            var result = photos?.FirstOrDefault();

            if (result is null)
                return;

            IsBusy = true;
            await using var stream = await result.OpenReadAsync();
            var contentType = ContentTypeFromFileName(result.FileName) ?? result.ContentType ?? "image/jpeg";
            var (photoId, url) = await _api.UploadPlanPhotoAsync(Plan.Id, stream, result.FileName, contentType);
            Photos.Add(new PlanPhotoItem(photoId, url));
            OnPropertyChanged(nameof(HasPhotos));
            OnPropertyChanged(nameof(HasNoPhotos));
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
    private async Task DeletePhoto(PlanPhotoItem photo)
    {
        if (Plan is null)
            return;
        var confirmed = await Shell.Current.DisplayAlertAsync("Remove Photo", "Remove this photo from the plan?", "Remove", "Cancel");
        if (!confirmed)
            return;
        try
        {
            await _api.DeletePlanPhotoAsync(Plan.Id, photo.Id);
            Photos.Remove(photo);
            OnPropertyChanged(nameof(HasPhotos));
            OnPropertyChanged(nameof(HasNoPhotos));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
}

public record PlanPhotoItem(int Id, string Url);
