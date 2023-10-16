using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pororoca.Desktop.Views;

public class CollectionScopedAuthView : UserControl
{
    public CollectionScopedAuthView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}