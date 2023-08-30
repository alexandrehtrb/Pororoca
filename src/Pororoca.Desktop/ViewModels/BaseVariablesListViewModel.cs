using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.HotKeys;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public abstract class BaseVariablesListViewModel : CollectionOrganizationItemViewModel
{
    #region COLLECTION VARIABLES

    private readonly VariablesClipboardArea variablesClipboardArea = VariablesClipboardArea.Instance;

    public ObservableCollection<VariableViewModel> Variables { get; }

    public ConcurrentDictionary<VariableViewModel, bool> SelectedVariables { get; }

    public ReactiveCommand<Unit, Unit> AddNewVariableCmd { get; }
    public ReactiveCommand<Unit, Unit> CutSelectedVariablesCmd { get; }
    public ReactiveCommand<Unit, Unit> CopySelectedVariablesCmd { get; }
    public ReactiveCommand<Unit, Unit> PasteVariablesCmd { get; }
    public ReactiveCommand<Unit, Unit> DuplicateSelectedVariablesCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteSelectedVariablesCmd { get; }

    #endregion

    public BaseVariablesListViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                      string name,
                                      List<PororocaVariable> vars) : base(parentVm, name)
    {
        AddNewVariableCmd = ReactiveCommand.Create(AddNewVariable);
        CutSelectedVariablesCmd = ReactiveCommand.Create(() => CutOrCopySelectedVariables(false));
        CopySelectedVariablesCmd = ReactiveCommand.Create(() => CutOrCopySelectedVariables(true));
        PasteVariablesCmd = ReactiveCommand.Create(PasteVariables);
        DuplicateSelectedVariablesCmd = ReactiveCommand.Create(DuplicateSelectedVariables);
        DeleteSelectedVariablesCmd = ReactiveCommand.Create(DeleteSelectedVariables);

        Variables = new();
        SelectedVariables = new();
        foreach (var v in vars)
        {
            Variables.Add(new(Variables, v));
        }
    }

    #region COLLECTION VARIABLES

    private void AddNewVariable() =>
        Variables.Add(new(Variables, true, string.Empty, string.Empty, false));

    private void CutOrCopySelectedVariables(bool falseIfCutTrueIfCopy)
    {
        if (SelectedVariables is not null && SelectedVariables.Count > 0)
        {
            // needs to generate a new array because of concurrency problems
            var vars = SelectedVariables.Select(x => x.Key).ToArray();
            if (falseIfCutTrueIfCopy == false)
            {
                foreach (var v in vars)
                {
                    Variables.Remove(v);
                }
            }
            var varsDomain = vars.Select(x => x.ToVariable()).ToArray();
            this.variablesClipboardArea.PushToArea(varsDomain);
        }
    }

    private void PasteVariables()
    {
        if (this.variablesClipboardArea.CanPasteVariables)
        {
            var vars = this.variablesClipboardArea.FetchCopiesOfVariables();
            foreach (var v in vars)
            {
                Variables.Add(new(Variables, v));
            }
        }
    }
    
    private void DuplicateSelectedVariables()
    {
        if (SelectedVariables is not null && SelectedVariables.Count > 0)
        {
            // needs to generate a new array because of concurrency problems
            var varsToDuplicate = SelectedVariables.Select(x => x.Key).ToArray();
            foreach (var v in varsToDuplicate)
            {
                VariableViewModel vCopy = new(Variables, v.ToVariable());
                int position = Variables.IndexOf(v);
                Variables.Insert(position, vCopy);
            }
        }
    }

    private void DeleteSelectedVariables()
    {
        if (SelectedVariables is not null && SelectedVariables.Count > 0)
        {
            // needs to generate a new array because of concurrency problems
            var varsToDelete = SelectedVariables.Select(x => x.Key).ToArray();
            foreach (var v in varsToDelete)
            {
                Variables.Remove(v);
            }
        }
    }

    #endregion
}