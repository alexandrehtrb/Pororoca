using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pororoca.Desktop.Views;

public class WelcomeView : UserControl
{
    public WelcomeView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}