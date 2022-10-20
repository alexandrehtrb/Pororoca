using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pororoca.Desktop.Views;

public class WebSocketClientMessageView : UserControl
{
    public WebSocketClientMessageView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}