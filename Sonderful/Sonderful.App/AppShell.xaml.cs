using Sonderful.App.Views;

namespace Sonderful.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(PlanDetailPage), typeof(PlanDetailPage));
        Routing.RegisterRoute(nameof(EditPlanPage), typeof(EditPlanPage));
        Routing.RegisterRoute(nameof(RatePage), typeof(RatePage));
    }
}
