using System.Reactive;
using Pororoca.Desktop.Behaviors;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class EnvironmentViewModel : CollectionOrganizationItemViewModel, IVariablesDataGridOwner
{
    #region COLLECTION ORGANIZATION

    public ReactiveCommand<Unit, Unit> ExportEnvironmentCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportAsPororocaEnvironmentCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportAsPostmanEnvironmentCmd { get; }

    #endregion

    #region ENVIRONMENT

    private readonly Guid envId;
    private readonly DateTimeOffset envCreatedAt;

    [Reactive]
    public VariablesDataGridViewModel VariablesTableVm { get; set; }

    [Reactive]
    public bool IsCurrentEnvironment { get; set; }

    [Reactive]
    public bool IncludeSecretVariables { get; set; }

    public ReactiveCommand<Unit, Unit> ToggleEnabledEnvironmentCmd { get; }

    #endregion

    #region OTHERS

    [Reactive]
    public bool IsOperatingSystemMacOsx { get; set; }

    #endregion

    public EnvironmentViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                PororocaEnvironment env,
                                Action<EnvironmentViewModel> onToggleEnabledEnvironment,
                                Func<bool>? isOperatingSystemMacOsx = null) : base(parentVm, env.Name)
    {
        #region OTHERS
        IsOperatingSystemMacOsx = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
        #endregion

        #region COLLECTION ORGANIZATION

        ExportEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportEnvironmentAsync);
        ExportAsPororocaEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportAsPororocaEnvironmentAsync);
        ExportAsPostmanEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportAsPostmanEnvironmentAsync);

        #endregion

        #region ENVIRONMENT

        this.envId = env.Id;
        this.envCreatedAt = env.CreatedAt;
        VariablesTableVm = new(env.Variables);
        IsCurrentEnvironment = env.IsCurrent;
        ToggleEnabledEnvironmentCmd = ReactiveCommand.Create(() => onToggleEnabledEnvironment(this));
        #endregion
    }

    #region COLLECTION ORGANIZATION

    protected override void CopyThis() =>
        ClipboardArea.Instance.PushToCopy(ToEnvironment());

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

    private Task ExportEnvironmentAsync() =>
        FileExporterImporter.ExportEnvironmentAsync(this);

    private Task ExportAsPororocaEnvironmentAsync() =>
        FileExporterImporter.ExportAsPororocaEnvironmentAsync(this);

    private Task ExportAsPostmanEnvironmentAsync() =>
        FileExporterImporter.ExportAsPostmanEnvironmentAsync(this);

    #endregion

    #endregion

    #region ENVIRONMENT

    public PororocaEnvironment ToEnvironment() =>
        new(this.envId,
            this.envCreatedAt,
            Name,
            IsCurrentEnvironment,
            VariablesTableVm.ConvertItemsToDomain().ToList());

    #endregion
}