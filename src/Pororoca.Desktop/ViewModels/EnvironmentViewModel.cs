using System.Reactive;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class EnvironmentViewModel : BaseVariablesListViewModel
{
    #region COLLECTION ORGANIZATION

    public ReactiveCommand<Unit, Unit> ExportEnvironmentCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportAsPororocaEnvironmentCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportAsPostmanEnvironmentCmd { get; }

    #endregion

    #region ENVIRONMENT

    private readonly Guid envId;
    private readonly DateTimeOffset envCreatedAt;
    private readonly Action<EnvironmentViewModel> onEnvironmentSetAsCurrent;

    [Reactive]
    public bool IsCurrentEnvironment { get; set; }

    [Reactive]
    public bool IncludeSecretVariables { get; set; }

    public ReactiveCommand<Unit, Unit> SetAsCurrentEnvironmentCmd { get; }

    #endregion

    #region OTHERS

    [Reactive]
    public bool IsOperatingSystemMacOsx { get; set; }

    #endregion

    public EnvironmentViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                PororocaEnvironment env,
                                Action<EnvironmentViewModel> onEnvironmentSetAsCurrent,
                                Func<bool>? isOperatingSystemMacOsx = null) : base(parentVm, env.Name, env.Variables)
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
        this.onEnvironmentSetAsCurrent = onEnvironmentSetAsCurrent;

        IsCurrentEnvironment = env.IsCurrent;
        SetAsCurrentEnvironmentCmd = ReactiveCommand.Create(SetAsCurrentEnvironment);
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

    private void SetAsCurrentEnvironment() =>
        this.onEnvironmentSetAsCurrent(this);

    public PororocaEnvironment ToEnvironment()
    {
        PororocaEnvironment newEnv = new(this.envId, Name, this.envCreatedAt);
        UpdateEnvironmentWithInputs(newEnv);
        return newEnv;
    }

    private void UpdateEnvironmentWithInputs(PororocaEnvironment environment)
    {
        environment.IsCurrent = IsCurrentEnvironment;
        environment.UpdateVariables(Variables.Select(v => v.ToVariable()));
    }

    #endregion
}