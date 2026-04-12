using Sonderful.App.ViewModels;

namespace Sonderful.App.Views;

public partial class DiscoverPage : ContentPage
{
    public DiscoverPage(DiscoverViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DiscoverViewModel vm)
            _ = vm.RefreshAsync();
    }
}
