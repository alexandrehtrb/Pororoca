using Avalonia.Markup.Xaml;

namespace Pororoca.Desktop.Views;

public class CollectionVariablesView : BaseVariablesListView
{
    public CollectionVariablesView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}