namespace Sonderful.App.Views;

public partial class PhotoViewerPage : ContentPage
{
    public PhotoViewerPage(string imageUrl)
    {
        InitializeComponent();
        PhotoImage.Source = imageUrl;
    }

    private async void OnCloseClicked(object? sender, EventArgs e) =>
        await Navigation.PopModalAsync();
}
