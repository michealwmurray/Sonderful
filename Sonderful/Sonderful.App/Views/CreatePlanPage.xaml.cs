using Sonderful.App.ViewModels;

namespace Sonderful.App.Views;

public partial class CreatePlanPage : ContentPage
{
    public CreatePlanPage(CreatePlanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
