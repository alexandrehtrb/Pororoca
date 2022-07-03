using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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

    private void OnCloseMainWindow(object sender, RoutedEventArgs e) =>
        Close();
}