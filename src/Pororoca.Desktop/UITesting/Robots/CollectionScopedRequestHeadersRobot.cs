using Avalonia.Controls;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class CollectionScopedRequestHeadersRobot : BaseNamedRobot
{
    public CollectionScopedRequestHeadersRobot(CollectionScopedRequestHeadersView rootView)
     : base(rootView) { }

    internal Button GoBack => GetChildView<Button>("btGoBack")!;
    internal Button AddHeader => GetChildView<Button>("btAddColScopedReqHeader")!;
    internal DataGrid Headers => GetChildView<RequestHeadersTableView>("rhtvColScopedReqHeaders")!.FindControl<DataGrid>("datagrid")!;

    internal async Task SetRequestHeaders(IEnumerable<PororocaKeyValueParam> headers)
    {
        var vm = ((CollectionScopedRequestHeadersViewModel)RootView!.DataContext!).RequestHeadersTableVm;
        vm.Items.Clear();
        foreach (var header in headers)
        {
            vm.Items.Add(new(vm.Items, header));
        }
        await UITestActions.WaitAfterActionAsync();
    }
}