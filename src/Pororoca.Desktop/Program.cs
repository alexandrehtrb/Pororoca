using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop;

public static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        PororocaLogger.Instance = new(UserDataManager.GetUserDataFolder());
        try
        {
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            PororocaLogger.Instance?.Log(PororocaLogLevel.Fatal, "Program crashed!", ex);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI()
            .AfterSetup(ab =>
            {
                var appLifetime = ab.Instance!.ApplicationLifetime!;
                if (appLifetime is IClassicDesktopStyleApplicationLifetime classicDesktopAppLifetime)
                {
                    classicDesktopAppLifetime.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    classicDesktopAppLifetime.Exit += SaveUserData;
                }
            });

    private static void SaveUserData(object? sender, ControlledApplicationLifetimeExitEventArgs e) =>
        ((MainWindowViewModel)MainWindow.Instance!.DataContext!).SaveUserData();
}