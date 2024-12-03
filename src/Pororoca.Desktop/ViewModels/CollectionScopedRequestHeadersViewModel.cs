using System.Reactive;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionScopedRequestHeadersViewModel : CollectionOrganizationItemViewModel
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

    private void GoBack() =>
        MainWindowVm.SwitchVisiblePage((ViewModelBase)Parent);

    public List<PororocaKeyValueParam>? ToCollectionScopedRequestHeaders() =>
        RequestHeadersTableVm.Items.Count > 0 ?
        RequestHeadersTableVm.ConvertItemsToDomain().ToList() :
        null;

    #endregion
}