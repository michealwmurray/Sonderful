using Microsoft.Extensions.DependencyInjection;

namespace Sonderful.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
#if WINDOWS
            window.Created += (s, e) =>
            {
                var handle = WinRT.Interop.WindowNative.GetWindowHandle(window.Handler.PlatformView);
                var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
                if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                    presenter.Maximize();
            };
#endif
            return window;
        }
    }
}