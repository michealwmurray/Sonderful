using Sonderful.App.ViewModels;

namespace Sonderful.App.Views;

public partial class RatePage : ContentPage
{
    public RatePage(RateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
