using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels.DataGrids;
using static Pororoca.Desktop.Views.DataGridSelectionUpdater;

namespace Pororoca.Desktop.Controls;

public sealed class RequestHeadersTableView : UserControl
{
    public RequestHeadersTableView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void OnSelectedRequestHeadersChanged(object sender, SelectionChangedEventArgs e)
    {
        var tableVm = (RequestHeadersDataGridViewModel)DataContext!;
        UpdateVmSelectedItems(tableVm, e);
    }
}