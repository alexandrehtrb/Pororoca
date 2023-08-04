using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class EnvironmentsGroupViewModel : CollectionOrganizationItemParentViewModel<EnvironmentViewModel>
{
    #region COLLECTION ORGANIZATION

    public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;
    public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
    public ReactiveCommand<Unit, Unit> AddNewEnvironmentCmd { get; }

    #endregion

    #region ENVIRONMENTS GROUP

    [Reactive]
    public string? SelectedEnvironmentName { get; set; }

    public override ObservableCollection<EnvironmentViewModel> Items { get; }
    public ReactiveCommand<Unit, Unit> ImportEnvironmentsCmd { get; }

    #endregion

    #region OTHERS

    private readonly bool isOperatingSystemMacOsx;

    #endregion

    public EnvironmentsGroupViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                      IEnumerable<PororocaEnvironment> envs,
                                      Func<bool>? isOperatingSystemMacOsx = null) : base(parentVm, string.Empty)
    {
        #region OTHERS
        this.isOperatingSystemMacOsx = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
        #endregion

        #region COLLECTION ORGANIZATION
        AddNewEnvironmentCmd = ReactiveCommand.Create(AddNewEnvironment);
        ImportEnvironmentsCmd = ReactiveCommand.CreateFromTask(ImportEnvironmentsAsync);
        #endregion

        #region ENVIRONMENTS GROUP
        Items = new(envs.Select(e => new EnvironmentViewModel(this, e, SetEnvironmentAsCurrent)));
        UpdateSelectedEnvironmentName();
        RefreshSubItemsAvailableMovements();
        #endregion
    }

    #region COLLECTION ORGANIZATION

    public void AddNewEnvironment()
    {
        PororocaEnvironment newEnv = new(Localizer.Instance.Environment.NewEnvironment);
        AddEnvironment(newEnv, showItemInScreen: true);
    }

    internal void AddEnvironment(PororocaEnvironment envToAdd, bool showItemInScreen = false)
    {
        EnvironmentViewModel envToAddVm = new(this, envToAdd, SetEnvironmentAsCurrent);
        // When adding an environment, set the environment
        // as non-current, to not have two current environments
        // when pasting.
        envToAddVm.IsCurrentEnvironment = false;
        Items.Add(envToAddVm);
        ((CollectionViewModel)Parent).IsExpanded = true;
        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(envToAddVm, showItemInScreen);
    }

    protected override void CopyThis() =>
        throw new NotImplementedException();

    public override void PasteToThis()
    {
        var envsToPaste = ClipboardArea.Instance.FetchCopiesOfEnvironments();
        foreach (var envToPaste in envsToPaste)
        {
            AddEnvironment(envToPaste);
        }
    }

    #endregion

    #region ENVIRONMENTS GROUP

    private void SetEnvironmentAsCurrent(EnvironmentViewModel envVm)
    {
        foreach (var evm in Items)
        {
            evm.IsCurrentEnvironment = evm == envVm;
        }
        UpdateSelectedEnvironmentName();
    }

    public void CycleActiveEnvironment(bool trueIfNextFalseIfPrevious)
    {
        if (Items.Count == 0)
        {
            return; // if there are no environments
        }

        var selectedEnv = Items.FirstOrDefault(i => i.IsCurrentEnvironment);
        int nextIndex;
        if (selectedEnv == null)
        {
            nextIndex = 0; // if no active environments, the first one will be activated
        }
        else
        {
            int currentIndex = Items.IndexOf(selectedEnv);
            if (trueIfNextFalseIfPrevious)
            {
                // cycling forward
                nextIndex = (currentIndex + 1) % Items.Count;
            }
            else
            {
                // cycling backwards
                nextIndex = (currentIndex - 1 + Items.Count) % Items.Count;
            }
        }
        var nextEnv = Items[nextIndex];
        SetEnvironmentAsCurrent(nextEnv);
    }

    public void UpdateSelectedEnvironmentName()
    {
        var selectedEnv = Items.FirstOrDefault(i => i.IsCurrentEnvironment);
        SelectedEnvironmentName = selectedEnv is not null ?
                                  '(' + selectedEnv.Name + ')' :
                                  null;
    }

    public IEnumerable<PororocaEnvironment> ToEnvironments() =>
        Items.Select(e => e.ToEnvironment());

    public Task ImportEnvironmentsAsync() =>
        FileExporterImporter.ImportEnvironmentsAsync(this);

    #endregion
}