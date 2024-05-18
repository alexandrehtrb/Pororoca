using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels.DataGrids;
using static Pororoca.Desktop.Views.DataGridSelectionUpdater;

namespace Pororoca.Desktop.Controls;

public sealed class VariablesTableView : UserControl
{
    public VariablesTableView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void OnSelectedVariablesChanged(object sender, SelectionChangedEventArgs e)
    {
        var tableVm = (VariablesDataGridViewModel)DataContext!;
        UpdateVmSelectedItems(tableVm, e);
    }
}