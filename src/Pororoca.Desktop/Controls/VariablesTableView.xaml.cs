using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.Controls;

public sealed class VariablesTableView : UserControl
{
    public VariablesTableView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void OnSelectedVariablesChanged(object sender, SelectionChangedEventArgs e) =>
        ((VariablesDataGridViewModel)DataContext!).UpdateSelectedItems(e);
}