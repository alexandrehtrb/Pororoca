using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using Avalonia.Controls;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.ExportEnvironment;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Desktop.ViewModels;

public sealed class EnvironmentViewModel : CollectionOrganizationItemViewModel
{
    #region COLLECTION ORGANIZATION

    public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
    public ReactiveCommand<Unit, Unit> CopyEnvironmentCmd { get; }
    public ReactiveCommand<Unit, Unit> RenameEnvironmentCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteEnvironmentCmd { get; }
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

    public ObservableCollection<VariableViewModel> Variables { get; }
    public ReactiveCommand<Unit, Unit> AddNewVariableCmd { get; }
    public ReactiveCommand<Unit, Unit> SetAsCurrentEnvironmentCmd { get; }

    #endregion

    #region OTHERS

    [Reactive]
    public bool IsOperatingSystemMacOsx { get; set; }

    #endregion

    public EnvironmentViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                PororocaEnvironment env,
                                Action<EnvironmentViewModel> onEnvironmentSetAsCurrent,
                                Func<bool>? isOperatingSystemMacOsx = null) : base(parentVm, env.Name)
    {
        #region OTHERS
        IsOperatingSystemMacOsx = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
        #endregion

        #region COLLECTION ORGANIZATION

        MoveUpCmd = ReactiveCommand.Create(MoveThisUp);
        MoveDownCmd = ReactiveCommand.Create(MoveThisDown);
        CopyEnvironmentCmd = ReactiveCommand.Create(Copy);
        RenameEnvironmentCmd = ReactiveCommand.Create(RenameThis);
        DeleteEnvironmentCmd = ReactiveCommand.Create(Delete);
        ExportEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportEnvironmentAsync);
        ExportAsPororocaEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportAsPororocaEnvironmentAsync);
        ExportAsPostmanEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportAsPostmanEnvironmentAsync);

        #endregion

        #region ENVIRONMENT

        this.envId = env.Id;
        this.envCreatedAt = env.CreatedAt;
        this.onEnvironmentSetAsCurrent = onEnvironmentSetAsCurrent;

        Variables = new();
        foreach (var v in env.Variables) { Variables.Add(new(Variables, v)); }
        IsCurrentEnvironment = env.IsCurrent;
        AddNewVariableCmd = ReactiveCommand.Create(AddNewVariable);
        SetAsCurrentEnvironmentCmd = ReactiveCommand.Create(SetAsCurrentEnvironment);
        #endregion
    }

    #region COLLECTION ORGANIZATION

    protected override void CopyThis() =>
        CollectionsGroupDataCtx.PushToCopy(ToEnvironment());

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

    private void AddNewVariable() =>
        Variables.Add(new(Variables, true, string.Empty, string.Empty, false));

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