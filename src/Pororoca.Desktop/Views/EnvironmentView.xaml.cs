using Avalonia.Markup.Xaml;

namespace Pororoca.Desktop.Views;

public class EnvironmentView : BaseVariablesListView
{
    public EnvironmentView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}