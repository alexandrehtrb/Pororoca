using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Views;

public class EnvironmentView : UserControl
{
    public EnvironmentView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}