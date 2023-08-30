using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionVariablesViewModel : BaseVariablesListViewModel
{

    public CollectionVariablesViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                        PororocaCollection col) : base(parentVm, col.Name, col.Variables)
    {
    }

    #region COLLECTION ORGANIZATION

    protected override void CopyThis() =>
        throw new NotImplementedException();

    #endregion

    #region COLLECTION VARIABLES

    public IEnumerable<PororocaVariable> ToVariables() =>
        Variables.Select(v => v.ToVariable());

    #endregion
}