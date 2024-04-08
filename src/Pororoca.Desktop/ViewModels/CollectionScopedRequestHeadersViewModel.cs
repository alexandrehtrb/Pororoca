using System.Reactive;
using Pororoca.Desktop.Behaviors;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionScopedRequestHeadersViewModel : CollectionOrganizationItemViewModel, IRequestHeadersDataGridOwner
{
    public ReactiveCommand<Unit, Unit> GoBackCmd { get; }

    public RequestHeadersDataGridViewModel RequestHeadersTableVm { get; }

    public CollectionScopedRequestHeadersViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                                   PororocaCollection col) : base(parentVm, string.Empty)
    {
        GoBackCmd = ReactiveCommand.Create(GoBack);
        RequestHeadersTableVm = new(col.CollectionScopedRequestHeaders);
    }

    #region COLLECTION ORGANIZATION

    protected override void CopyThis() =>
        throw new NotImplementedException();

    private void GoBack()
    {
        var mainWindowVm = ((MainWindowViewModel)MainWindow.Instance!.DataContext!);
        mainWindowVm.SwitchVisiblePage((ViewModelBase)Parent);
    }

    public List<PororocaKeyValueParam>? ToCollectionScopedRequestHeaders() =>
        RequestHeadersTableVm.Items.Count > 0 ?
        RequestHeadersTableVm.ConvertItemsToDomain().ToList() :
        null;

    #endregion
}