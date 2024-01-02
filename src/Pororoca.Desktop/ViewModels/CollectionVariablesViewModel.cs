using Pororoca.Desktop.Behaviors;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionVariablesViewModel : CollectionOrganizationItemViewModel, IVariablesDataGridOwner
{
    [Reactive]
    public VariablesDataGridViewModel VariablesTableVm { get; set; }

    public CollectionVariablesViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                        PororocaCollection col) : base(parentVm, col.Name) =>
        VariablesTableVm = new(col.Variables);

    #region COLLECTION ORGANIZATION

    protected override void CopyThis() =>
        throw new NotImplementedException();

    #endregion

    #region COLLECTION VARIABLES

    public IEnumerable<PororocaVariable> ToVariables() =>
        VariablesTableVm.Items.Select(v => v.ToVariable());

    #endregion
}