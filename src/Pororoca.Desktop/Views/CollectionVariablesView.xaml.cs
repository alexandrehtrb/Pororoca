using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels;
using static Pororoca.Desktop.Views.DataGridSelectionUpdater;

namespace Pororoca.Desktop.Views;

public class CollectionVariablesView : UserControl
{
    public CollectionVariablesView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void OnSelectedVariablesChanged(object sender, SelectionChangedEventArgs e)
    {
        var tableVm = ((CollectionVariablesViewModel)DataContext!).VariablesTableVm;
        UpdateVmSelectedItems(tableVm, e);
    }
}