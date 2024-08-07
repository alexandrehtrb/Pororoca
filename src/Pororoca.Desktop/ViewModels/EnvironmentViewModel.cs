using System.Reactive;
using Pororoca.Desktop.Behaviors;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class EnvironmentViewModel : CollectionOrganizationItemViewModel
{
    #region COLLECTION ORGANIZATION

    public ReactiveCommand<Unit, Unit> ExportEnvironmentCmd { get; }

    #endregion

    #region ENVIRONMENT

    private readonly Guid envId;
    private readonly DateTimeOffset envCreatedAt;

    [Reactive]
    public VariablesDataGridViewModel VariablesTableVm { get; set; }

    [Reactive]
    public bool IsCurrentEnvironment { get; set; }

    public ReactiveCommand<Unit, Unit> ToggleEnabledEnvironmentCmd { get; }

    public ExportEnvironmentViewModel ExportEnvironmentVm { get; }

    #endregion


    public EnvironmentViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                PororocaEnvironment env,
                                Action<EnvironmentViewModel> onToggleEnabledEnvironment) : base(parentVm, env.Name)
    {
        #region COLLECTION ORGANIZATION

        ExportEnvironmentCmd = ReactiveCommand.Create(GoToExportEnvironment);

        #endregion

        #region ENVIRONMENT

        this.envId = env.Id;
        this.envCreatedAt = env.CreatedAt;
        VariablesTableVm = new(env.Variables);
        IsCurrentEnvironment = env.IsCurrent;
        ToggleEnabledEnvironmentCmd = ReactiveCommand.Create(() => onToggleEnabledEnvironment(this));
        ExportEnvironmentVm = new(this);
        #endregion
    }

    #region COLLECTION ORGANIZATION

    protected override void CopyThis() =>
        ClipboardArea.Instance.PushToCopy(ToEnvironment(forExporting: false));

    protected override void OnNameUpdated(string newName)
    {
        base.OnNameUpdated(newName);
        ((EnvironmentsGroupViewModel)Parent).UpdateSelectedEnvironmentName();
    }

    public override void DeleteThis()
    {
        base.DeleteThis();
        if (IsCurrentEnvironment)
        {
            ((EnvironmentsGroupViewModel)Parent).UpdateSelectedEnvironmentName();
        }
    }

    #region EXPORT ENVIRONMENT

    private void GoToExportEnvironment()
    {
        var mwvm = (MainWindowViewModel)MainWindow.Instance!.DataContext!;
        mwvm.SwitchVisiblePage(ExportEnvironmentVm);
    }

    #endregion

    #endregion

    #region ENVIRONMENT

    public PororocaEnvironment ToEnvironment(bool forExporting = false)
    {
        bool includeSecretVariables = !forExporting || ExportEnvironmentVm.IncludeSecretVariables;
        bool markAsCurrentEnvironment = !forExporting && IsCurrentEnvironment;

        return new(this.envId,
                   this.envCreatedAt,
                   Name,
                   markAsCurrentEnvironment,
                   VariablesTableVm.GetVariables(includeSecretVariables));
    }

    #endregion
}