using Sonderful.App.ViewModels;

namespace Sonderful.App.Views;

public partial class PlanDetailPage : ContentPage
{
    public PlanDetailPage(PlanDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
