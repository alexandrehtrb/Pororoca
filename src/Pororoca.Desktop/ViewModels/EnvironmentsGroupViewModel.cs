using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.ExportImport;
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
    public ReactiveCommand<Unit, Unit> PasteToEnvironmentsCmd { get; }

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
        PasteToEnvironmentsCmd = ReactiveCommand.Create(PasteToThis);
        ImportEnvironmentsCmd = ReactiveCommand.CreateFromTask(ImportEnvironmentsAsync);
        #endregion

        #region ENVIRONMENTS GROUP
        Items = new(envs.Select(e => new EnvironmentViewModel(this, e, SetEnvironmentAsCurrent)));
        UpdateSelectedEnvironmentName();
        RefreshSubItemsAvailableMovements();
        #endregion
    }

    #region COLLECTION ORGANIZATION

    public override void RefreshSubItemsAvailableMovements()
    {
        for (int x = 0; x < Items.Count; x++)
        {
            var envVm = Items[x];
            bool canMoveUp = x > 0;
            bool canMoveDown = x < Items.Count - 1;
            envVm.CanMoveUp = canMoveUp;
            envVm.CanMoveDown = canMoveDown;
        }
    }

    public void AddNewEnvironment()
    {
        PororocaEnvironment newEnv = new(Localizer.Instance["Environment/NewEnvironment"]);
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
        var envsToPaste = CollectionsGroupDataCtx.FetchCopiesOfEnvironments();
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