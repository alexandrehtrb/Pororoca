using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.Controls;

public sealed class ResponseHeadersTableView : UserControl
{
    public ResponseHeadersTableView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void OnSelectedResponseHeadersAndTrailersChanged(object sender, SelectionChangedEventArgs e) =>
        ((KeyValueParamsDataGridViewModel)DataContext!).UpdateSelectedItems(e);
}