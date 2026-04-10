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

        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddSingleton<IApiService>(sp =>
            new ApiService(new HttpClient(), sp.GetRequiredService<SessionService>()));

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<RegisterViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
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

        // Picker (ComboBox on Windows)
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
        {
#if WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.BorderBrush = transparent;
            handler.PlatformView.Background = transparent;
            handler.PlatformView.Loaded += (s, e) =>
            {
                if (s is Microsoft.UI.Xaml.Controls.ComboBox cb)
                {
                    cb.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
                    cb.HorizontalContentAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                    CenterVisualTreeTextBlocks(cb);
                }
            };
#elif ANDROID
            handler.PlatformView.Background = null;
#endif
        });

        // DatePicker (CalendarDatePicker on Windows)
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
        {
#if WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = transparent;
            handler.PlatformView.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
#endif
        });

        // TimePicker (TimePicker on Windows)
        Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
        {
#if WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = transparent;
            handler.PlatformView.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
#endif
        });
    }

#if WINDOWS
    // Walks the WinUI visual tree and vertically centers any TextBlock children.
    // Used to fix ComboBox (Picker) placeholder text alignment when HeightRequest
    // forces the control taller than its natural WinUI size.
    private static void CenterVisualTreeTextBlocks(Microsoft.UI.Xaml.DependencyObject parent)
    {
        int count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is Microsoft.UI.Xaml.Controls.TextBlock tb)
                tb.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            CenterVisualTreeTextBlocks(child);
        }
    }
#endif
}
