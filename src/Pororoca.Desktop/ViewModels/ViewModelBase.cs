using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    public Localizer i18n { get; } = Localizer.Instance;

    public KeyboardShortcuts HotKeys { get; } = KeyboardShortcuts.Instance;

    public ClipboardArea ClipboardArea { get; } = ClipboardArea.Instance;

    protected static MainWindowViewModel MainWindowVm =>
        (MainWindowViewModel)MainWindow.Instance!.DataContext!;
}