using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Views;

public class CollectionVariablesView : UserControl
{
    public CollectionVariablesView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}