using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionVariablesViewModel : CollectionOrganizationItemViewModel
{
    #region COLLECTION VARIABLES

    public ObservableCollection<VariableViewModel> Variables { get; }
    public ReactiveCommand<Unit, Unit> AddNewVariableCmd { get; }

    #endregion

    public CollectionVariablesViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                        PororocaCollection col) : base(parentVm, col.Name)
    {
        AddNewVariableCmd = ReactiveCommand.Create(AddNewVariable);

        Variables = new();
        foreach (var v in col.Variables)
        {
            Variables.Add(new(Variables, v));
        }
    }

    #region COLLECTION ORGANIZATION

    protected override void CopyThis() =>
        throw new NotImplementedException();

    #endregion

    #region COLLECTION VARIABLES

    private void AddNewVariable() =>
        Variables.Add(new(Variables, true, string.Empty, string.Empty, false));

    public IEnumerable<PororocaVariable> ToVariables() =>
        Variables.Select(v => v.ToVariable());

    #endregion
}