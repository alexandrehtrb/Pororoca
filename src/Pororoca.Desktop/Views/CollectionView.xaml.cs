using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pororoca.Desktop.Views;

public sealed class CollectionView : UserControl
{
    public CollectionView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}