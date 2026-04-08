using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;
#if DEBUG
using AvaloniaUI.DiagnosticsSupport;
#endif

namespace Pororoca.Desktop;

public sealed class App : Application
{
    public override void Initialize() =>
        AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        LoadInitialTheme();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

#if DEBUG
        this.AttachDeveloperTools();
#endif

        base.OnFrameworkInitializationCompleted();
    }

    private void LoadInitialTheme()
    {
        var initialTheme = UserDataManager.LoadUserPreferences()?.Theme ?? PororocaThemeManager.DefaultTheme;
        PororocaThemeManager.CurrentTheme = initialTheme;
    }
}