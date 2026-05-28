using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
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
        Version? appVersion = null;
        DirectoryInfo? userDataDir = null;

        try
        {
            appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            userDataDir = UserDataManager.GetUserDataFolder();

            PororocaLogger.Instance = new(appVersion: appVersion, userDataDir: userDataDir);

            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            PororocaLogger.Instance?.Log(PororocaLogLevel.Fatal, "Program crashed!", ex);
            // Line below doesn't throw exceptions, however,
            // if there's an error whilst logging,
            // let's print an error on the Console and Debug.
            var logMsg = PororocaLogger.BuildExceptionMessage(DateTime.Now,
                appVersion?.ToString(3),
                PororocaLogLevel.Fatal,
                "Program crashed!",
                ex,
                $"User data dir: {userDataDir?.FullName}");
            Console.WriteLine(logMsg);
            Debug.WriteLine(logMsg);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI()
            .With(new FontManagerOptions
            {
                // https://github.com/AvaloniaUI/Avalonia/issues/4427#issuecomment-1303697872
                DefaultFamilyName = "avares://Pororoca.Desktop/Assets/Fonts#Cabin"
            })
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
