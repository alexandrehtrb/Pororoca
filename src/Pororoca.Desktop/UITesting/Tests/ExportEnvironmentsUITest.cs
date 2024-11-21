using System.Collections.ObjectModel;
using System.Text;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Domain.Features.ExportEnvironment;
using Pororoca.Domain.Features.ImportEnvironment;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class ExportEnvironmentsUITest : SaveAndRestoreCollectionUITest
{
    public override async Task RunAsync()
    {
        await TestExportAsPororocaEnvWithoutSecrets();
        await TestExportAsPororocaEnvWithSecrets();
        await TestExportAsPostmanEnvWithoutSecrets();
        await TestExportAsPostmanEnvWithSecrets();
    }

    private Task TestExportAsPororocaEnvWithoutSecrets() =>
        RunBaseExportTest(
            format: ExportEnvironmentFormat.Pororoca,
            includeEnvSecrets: false,
            expectedEnvVars: GenerateEnvironmentVariables("env", secretsCleared: true));

    private Task TestExportAsPororocaEnvWithSecrets() =>
        RunBaseExportTest(
            format: ExportEnvironmentFormat.Pororoca,
            includeEnvSecrets: true,
            expectedEnvVars: GenerateEnvironmentVariables("env", secretsCleared: false));

    private Task TestExportAsPostmanEnvWithoutSecrets() =>
        RunBaseExportTest(
            format: ExportEnvironmentFormat.Postman,
            includeEnvSecrets: false,
            expectedEnvVars: GenerateEnvironmentVariables("env", secretsCleared: true));

    private Task TestExportAsPostmanEnvWithSecrets() =>
        RunBaseExportTest(
            format: ExportEnvironmentFormat.Postman,
            includeEnvSecrets: true,
            expectedEnvVars: GenerateEnvironmentVariables("env", secretsCleared: false));

    #region TEST STEPS

    private async Task RunBaseExportTest(
        ExportEnvironmentFormat format,
        bool includeEnvSecrets,
        ObservableCollection<VariableViewModel> expectedEnvVars)
    {
        var envVars = GenerateEnvironmentVariables("env");

        await CreateCollectionWithEnv(envVars);

        await SetupExporting(format, includeEnvSecrets);

        string json = await ExportEnvironment(format);

        await ClearAllEnvironments();

        await RestoreEnvironment(format, json);

        await AssertEnvironmentReimportedSuccessfully(expectedEnvVars);

        ClearAllCollections();
    }

    private async Task CreateCollectionWithEnv(ObservableCollection<VariableViewModel> envVars)
    {
        await TopMenuRobot.CreateNewCollection();

        await ColRobot.Name.Edit("COL1");
        await TreeRobot.Select("COL1");
        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV");
        await EnvRobot.SetVariables(envVars);
        await EnvRobot.SetAsCurrentEnvironment.ClickOn();
    }

    private async Task SetupExporting(
        ExportEnvironmentFormat format,
        bool exportWithSecrets)
    {
        await TreeRobot.Select("COL1/ENVS/ENV");
        await EnvRobot.ExportEnvironment.ClickOn();

        await ExportEnvRobot.SetIncludeSecretVars(exportWithSecrets);
        await ExportEnvRobot.SelectExportFormat(format);
    }

    private async Task<string> ExportEnvironment(ExportEnvironmentFormat format)
    {
        await TreeRobot.Select("COL1/ENVS/ENV");
        var env = ((EnvironmentViewModel)EnvRobot.RootView!.DataContext!).ToEnvironment(forExporting: true);
        return format switch
        {
            ExportEnvironmentFormat.Pororoca => Encoding.UTF8.GetString(PororocaEnvironmentExporter.ExportAsPororocaEnvironment(env)),
            ExportEnvironmentFormat.Postman => PostmanEnvironmentExporter.ExportAsPostmanEnvironment(env),
            _ => string.Empty
        };
    }

    private async Task ClearAllEnvironments()
    {
        await TreeRobot.Select("COL1");
        var cvm = ((CollectionViewModel)ColRobot.RootView!.DataContext!);
        cvm.EnvironmentsGroupVm.Items.Clear();
        MainWindowVm.CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = null;
    }

    private async Task RestoreEnvironment(ExportEnvironmentFormat format, string json)
    {
        await TreeRobot.Select("COL1");
        var cvm = ((CollectionViewModel)ColRobot.RootView!.DataContext!);
        if (format == ExportEnvironmentFormat.Pororoca)
        {
            Assert(PororocaEnvironmentImporter.TryImportPororocaEnvironment(json, out var reimportedEnv));
            cvm.EnvironmentsGroupVm.AddEnvironment(reimportedEnv!);
        }
        else if (format == ExportEnvironmentFormat.Postman)
        {
            Assert(PostmanEnvironmentImporter.TryImportPostmanEnvironment(json, out var reimportedEnv));
            cvm.EnvironmentsGroupVm.AddEnvironment(reimportedEnv!);
        }
    }

    private async Task AssertEnvironmentReimportedSuccessfully(ObservableCollection<VariableViewModel> expectedEnvVarsVms)
    {
        AssertTreeItemExists(CollectionsGroup, "COL1/ENVS/ENV");

        await TreeRobot.Select("COL1/ENVS/ENV");
        AssertIsVisible(EnvRobot.RootView);
        // imported env should be marked as not current
        AssertIsVisible(EnvRobot.SetAsCurrentEnvironment);
        var expectedEnvVars = expectedEnvVarsVms.Select(x => x.ToVariable()).ToList();
        var envVars = EnvRobot.VariablesVm.GetVariables(true);
        Assert(Enumerable.SequenceEqual(expectedEnvVars, envVars));
    }

    private void ClearAllCollections()
    {
        MainWindowVm.CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = null;
        MainWindowVm.CollectionsGroupViewDataCtx.Items.Clear();
        AssertTreeItemNotExists(CollectionsGroup, "COL1");
    }

    #endregion

}