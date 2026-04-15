using Sonderful.App.ViewModels;

namespace Sonderful.App.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((ProfileViewModel)BindingContext).LoadCommand.Execute(null);
    }
}
