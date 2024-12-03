using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.Controls;

public sealed class RequestHeadersTableView : UserControl
{
    public RequestHeadersTableView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void OnSelectedRequestHeadersChanged(object sender, SelectionChangedEventArgs e) =>
        ((RequestHeadersDataGridViewModel)DataContext!).UpdateSelectedItems(e);
}