using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels;
using static Pororoca.Desktop.Views.DataGridSelectionUpdater;

namespace Pororoca.Desktop.Views;

public class CollectionScopedRequestHeadersView : UserControl
{
    public CollectionScopedRequestHeadersView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void OnSelectedRequestHeadersChanged(object sender, SelectionChangedEventArgs e)
    {
        var tableVm = ((CollectionScopedRequestHeadersViewModel)DataContext!).RequestHeadersTableVm;
        UpdateVmSelectedItems(tableVm, e);
    }
}