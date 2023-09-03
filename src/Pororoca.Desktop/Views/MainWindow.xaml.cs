using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.TextEditorConfig;

namespace Pororoca.Desktop.Views;

public partial class MainWindow : Window
{
    // TODO: Avoid using singleton like this
    public static MainWindow? Instance;

    public MainWindow()
    {
        Instance = this;
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        TextEditorConfiguration.TextMateInstallations.ForEach(i => i.Item2.Dispose());
    }

    public void OnCloseMainWindow(object sender, RoutedEventArgs e) =>
        Close();
}