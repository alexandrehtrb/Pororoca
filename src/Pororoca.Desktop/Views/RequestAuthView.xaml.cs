using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pororoca.Desktop.Views;

public sealed class RequestAuthView : UserControl
{
    public RequestAuthView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}