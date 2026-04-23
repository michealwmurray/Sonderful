using Microsoft.Extensions.Logging;
using Sonderful.App.Services;
using Sonderful.App.ViewModels;
using Sonderful.App.Views;

namespace Sonderful.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("PlusJakartaSans-Regular.ttf", "JakartaRegular");
                fonts.AddFont("PlusJakartaSans-Medium.ttf", "JakartaMedium");
                fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "JakartaSemiBold");
                fonts.AddFont("PlusJakartaSans-Bold.ttf", "JakartaBold");
                fonts.AddFont("PlusJakartaSans-ExtraBold.ttf", "JakartaExtraBold");
            });

        // Remove default platform borders from inputs
        RemoveNativeInputBorders();
        HideScrollBars();

        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddSingleton<IApiService>(sp =>
            new ApiService(new HttpClient(), sp.GetRequiredService<SessionService>()));

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<DiscoverPage>();
        builder.Services.AddTransient<DiscoverViewModel>();
        builder.Services.AddTransient<PlanDetailPage>();
        builder.Services.AddTransient<PlanDetailViewModel>();
        builder.Services.AddTransient<CreatePlanPage>();
        builder.Services.AddTransient<CreatePlanViewModel>();
        builder.Services.AddTransient<EditPlanPage>();
        builder.Services.AddTransient<EditPlanViewModel>();
        builder.Services.AddTransient<RatePage>();
        builder.Services.AddTransient<RateViewModel>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ProfileViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void HideScrollBars()
    {
        Microsoft.Maui.Handlers.ScrollViewHandler.Mapper.AppendToMapping("NoScrollBar", (handler, _) =>
        {
#if WINDOWS
            handler.PlatformView.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Hidden;
#endif
        });
    }

    private static void RemoveNativeInputBorders()
    {
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
        {
#if WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.BorderBrush = transparent;
            handler.PlatformView.Background = transparent;
#elif ANDROID
            handler.PlatformView.Background = null;
#elif IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
        });

        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
        {
#if WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.BorderBrush = transparent;
            handler.PlatformView.Background = transparent;
#elif ANDROID
            handler.PlatformView.Background = null;
#endif
        });

        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
        {
#if WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.BorderBrush = transparent;
            handler.PlatformView.Background = transparent;
            handler.PlatformView.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            handler.PlatformView.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            handler.PlatformView.HorizontalContentAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
            var title = ((Microsoft.Maui.Controls.Picker)view).Title ?? string.Empty;
            handler.PlatformView.PlaceholderText = title;
            handler.PlatformView.Header = null;
            handler.PlatformView.HeaderTemplate = null;
            handler.PlatformView.RegisterPropertyChangedCallback(
                Microsoft.UI.Xaml.Controls.ComboBox.HeaderProperty,
                (s, _) =>
                {
                    if (s is Microsoft.UI.Xaml.Controls.ComboBox cb && cb.Header != null)
                    {
                        cb.Header = null;
                        cb.HeaderTemplate = null;
                    }
                });
            handler.PlatformView.Loaded += (s, e) =>
            {
                if (s is Microsoft.UI.Xaml.Controls.ComboBox cb)
                {
                    cb.Header = null;
                    cb.HeaderTemplate = null;
                }
            };
#elif ANDROID
            handler.PlatformView.Background = null;
#endif
        });

        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
        {
#if WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = transparent;
            handler.PlatformView.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
#endif
        });

        Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
        {
#if WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = transparent;
            handler.PlatformView.MinHeight = 0;
            handler.PlatformView.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            handler.PlatformView.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
#endif
        });
    }
}
