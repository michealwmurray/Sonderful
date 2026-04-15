using Sonderful.App.ViewModels;

namespace Sonderful.App.Views;

public partial class EditPlanPage : ContentPage
{
    public EditPlanPage(EditPlanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
