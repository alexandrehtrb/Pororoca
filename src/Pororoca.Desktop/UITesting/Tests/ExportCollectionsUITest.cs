using System.Collections.ObjectModel;
using System.Text;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.ExportCollection;
using Pororoca.Domain.Features.ImportCollection;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class ExportCollectionsUITest : SaveAndRestoreCollectionUITest
{
    public override async Task RunAsync()
    {
        await TestExportAsPostmanCollectionWithSecrets();
        await TestExportAsPostmanCollectionHidingSecrets();
        await TestExportAsPororocaCollectionWithSecretsNoEnvs();
        await TestExportAsPororocaCollectionHidingSecretsOnlyEnv1WithSecrets();
        await TestExportAsPororocaCollectionWithSecretsOnlyEnv2HidingSecrets();
        await TestExportAsPororocaCollectionWithSecretsBothEnvsWithSecrets();
    }

    private Task TestExportAsPostmanCollectionWithSecrets()
    {
        var expectedColVars = GenerateCollectionVariables(secretsCleared: false);
        foreach (var v in expectedColVars)
        {
            // Postman collection variables have no marking of secretness;
            // only Postman environment variables have that
            v.IsSecret = false;
        }

        return RunBaseExportTest(
            exportWithColVarsSecrets: true,
            format: ExportCollectionFormat.Postman,
            includeEnv1InExport: false,
            includeEnv1Secrets: false,
            includeEnv2InExport: false,
            includeEnv2Secrets: false,
            expectedColVars: expectedColVars,
            expectedEnv1Vars: null,
            expectedEnv2Vars: null);
    }

    private Task TestExportAsPostmanCollectionHidingSecrets()
    {
        var expectedColVars = GenerateCollectionVariables(secretsCleared: true);
        foreach (var v in expectedColVars)
        {
            // Postman collection variables have no marking of secretness;
            // only Postman environment variables have that
            v.IsSecret = false;
        }

        return RunBaseExportTest(
            exportWithColVarsSecrets: false,
            format: ExportCollectionFormat.Postman,
            includeEnv1InExport: false,
            includeEnv1Secrets: false,
            includeEnv2InExport: false,
            includeEnv2Secrets: false,
            expectedColVars: expectedColVars,
            expectedEnv1Vars: null,
            expectedEnv2Vars: null);
    }

    private Task TestExportAsPororocaCollectionWithSecretsNoEnvs() =>
        RunBaseExportTest(
            exportWithColVarsSecrets: true,
            format: ExportCollectionFormat.Pororoca,
            includeEnv1InExport: false,
            includeEnv1Secrets: true,
            includeEnv2InExport: false,
            includeEnv2Secrets: true,
            expectedColVars: GenerateCollectionVariables(secretsCleared: false),
            expectedEnv1Vars: null,
            expectedEnv2Vars: null);

    private Task TestExportAsPororocaCollectionHidingSecretsOnlyEnv1WithSecrets() =>
        RunBaseExportTest(
            exportWithColVarsSecrets: false,
            format: ExportCollectionFormat.Pororoca,
            includeEnv1InExport: true,
            includeEnv1Secrets: true,
            includeEnv2InExport: false,
            includeEnv2Secrets: true,
            expectedColVars: GenerateCollectionVariables(secretsCleared: true),
            expectedEnv1Vars: GenerateEnvironmentVariables("env1", secretsCleared: false),
            expectedEnv2Vars: null);

    private Task TestExportAsPororocaCollectionWithSecretsOnlyEnv2HidingSecrets() =>
        RunBaseExportTest(
            exportWithColVarsSecrets: true,
            format: ExportCollectionFormat.Pororoca,
            includeEnv1InExport: false,
            includeEnv1Secrets: false,
            includeEnv2InExport: true,
            includeEnv2Secrets: false,
            expectedColVars: GenerateCollectionVariables(secretsCleared: false),
            expectedEnv1Vars: null,
            expectedEnv2Vars: GenerateEnvironmentVariables("env2", secretsCleared: true));

    private Task TestExportAsPororocaCollectionWithSecretsBothEnvsWithSecrets() =>
        RunBaseExportTest(
            exportWithColVarsSecrets: true,
            format: ExportCollectionFormat.Pororoca,
            includeEnv1InExport: true,
            includeEnv1Secrets: true,
            includeEnv2InExport: true,
            includeEnv2Secrets: true,
            expectedColVars: GenerateCollectionVariables(secretsCleared: false),
            expectedEnv1Vars: GenerateEnvironmentVariables("env1", secretsCleared: false),
            expectedEnv2Vars: GenerateEnvironmentVariables("env2", secretsCleared: false));

    #region TEST STEPS

    private async Task RunBaseExportTest(
        bool exportWithColVarsSecrets,
        ExportCollectionFormat format,
        bool includeEnv1InExport,
        bool includeEnv1Secrets,
        bool includeEnv2InExport,
        bool includeEnv2Secrets,
        ObservableCollection<VariableViewModel> expectedColVars,
        ObservableCollection<VariableViewModel>? expectedEnv1Vars,
        ObservableCollection<VariableViewModel>? expectedEnv2Vars)
    {
        var colVars = GenerateCollectionVariables();
        var env1Vars = GenerateEnvironmentVariables("env1");
        var env2Vars = GenerateEnvironmentVariables("env2");

        await CreateCollectionWithDifferentItems(colVars, env1Vars, env2Vars);

        await SetupExporting(
            format,
            exportWithColVarsSecrets,
            includeEnv1InExport,
            includeEnv1Secrets,
            includeEnv2InExport,
            includeEnv2Secrets);

        string json = await ExportCollection(format);

        ClearAllCollections();

        RestoreCollection(format, json);

        await AssertCollectionReimportedSuccessfully(expectedColVars, expectedEnv1Vars, expectedEnv2Vars);

        ClearAllCollections();
    }

    private async Task CreateCollectionWithDifferentItems(ObservableCollection<VariableViewModel> colVars, ObservableCollection<VariableViewModel> env1Vars, ObservableCollection<VariableViewModel> env2Vars)
    {
        await TopMenuRobot.CreateNewCollection();

        await ColRobot.Name.Edit("COL1");
        await TreeRobot.Select("COL1/VARS");
        await ColVarsRobot.SetVariables(colVars);

        await TreeRobot.Select("COL1/AUTH");
        await ColAuthRobot.Auth.SetBearerAuth("token");

        await TreeRobot.Select("COL1");
        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV1");
        await EnvRobot.SetVariables(env1Vars);
        await EnvRobot.SetAsCurrentEnvironment.ClickOn();

        await TreeRobot.Select("COL1");
        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV2");
        await EnvRobot.SetVariables(env2Vars);

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPHEADERS");
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/headers");
        await HttpRobot.SetHttpVersion(1.1m);
        await HttpRobot.SetRequestHeaders(headers);
        await HttpRobot.SetEmptyBody();
    }

    private async Task SetupExporting(
        ExportCollectionFormat format,
        bool exportWithColVarsSecrets,
        bool includeEnv1InExport,
        bool includeEnv1Secrets,
        bool includeEnv2InExport,
        bool includeEnv2Secrets)
    {
        await TreeRobot.Select("COL1");
        await ColRobot.ExportCollection.ClickOn();

        ExportColRobot.CollectionName.AssertHasText("COL1");

        await ExportColRobot.SetIncludeColSecretVars(exportWithColVarsSecrets);
        await ExportColRobot.SelectExportFormat(format);
        await ExportColRobot.SetEnvironmentToExport(includeEnv1InExport, "ENV1", includeEnv1Secrets);
        await ExportColRobot.SetEnvironmentToExport(includeEnv2InExport, "ENV2", includeEnv2Secrets);
    }

    private async Task<string> ExportCollection(ExportCollectionFormat format)
    {
        await TreeRobot.Select("COL1");
        var collection = ((CollectionViewModel)ColRobot.RootView!.DataContext!).ToCollection(forExporting: true);
        if (format == ExportCollectionFormat.Pororoca)
        {
            MemoryStream ms = new(16384);
            PororocaCollectionExporter.ExportAsPororocaCollection(ms, collection);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        else if (format == ExportCollectionFormat.Postman)
        {
            return PostmanCollectionV21Exporter.ExportAsPostmanCollectionV21(collection);
        }
        else
        {
            return string.Empty;
        };
    }

    private void ClearAllCollections()
    {
        MainWindowVm.CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = null;
        MainWindowVm.CollectionsGroupViewDataCtx.Items.Clear();
        CollectionsGroup.AssertTreeItemNotExists("COL1");
    }

    private void RestoreCollection(ExportCollectionFormat format, string json)
    {
        var mwvm = ((MainWindowViewModel)MainWindow.Instance!.DataContext!);
        if (format == ExportCollectionFormat.Pororoca)
        {
            AssertCondition(PororocaCollectionImporter.TryImportPororocaCollection(json, preserveId: true, out var reimportedCollection));
            mwvm.AddCollection(reimportedCollection!, showItemInScreen: true);
        }
        else if (format == ExportCollectionFormat.Postman)
        {
            AssertCondition(PostmanCollectionV21Importer.TryImportPostmanCollection(json, out var reimportedCollection));
            mwvm.AddCollection(reimportedCollection!, showItemInScreen: true);
        }
    }

    private async Task AssertCollectionReimportedSuccessfully(ObservableCollection<VariableViewModel> expectedColVarsVms, ObservableCollection<VariableViewModel>? expectedEnv1VarsVms, ObservableCollection<VariableViewModel>? expectedEnv2VarsVms)
    {
        CollectionsGroup.AssertTreeItemExists("COL1");
        CollectionsGroup.AssertTreeItemExists("COL1/AUTH");
        if (expectedEnv1VarsVms is not null)
        {
            CollectionsGroup.AssertTreeItemExists("COL1/ENVS/ENV1");
        }
        else
        {
            CollectionsGroup.AssertTreeItemNotExists("COL1/ENVS/ENV1");
        }
        if (expectedEnv2VarsVms is not null)
        {
            CollectionsGroup.AssertTreeItemExists("COL1/ENVS/ENV2");
        }
        else
        {
            CollectionsGroup.AssertTreeItemNotExists("COL1/ENVS/ENV2");
        }
        CollectionsGroup.AssertTreeItemExists("COL1/HTTPHEADERS");

        await TreeRobot.Select("COL1");
        ColRobot.RootView.AssertIsVisible();

        await TreeRobot.Select("COL1/AUTH");
        ColAuthRobot.RootView.AssertIsVisible();
        ColAuthRobot.Auth.AuthType.AssertSelection(ColAuthRobot.Auth.AuthTypeOptionBearer);
        ColAuthRobot.Auth.BearerAuthToken.AssertIsVisible();
        ColAuthRobot.Auth.BearerAuthToken.AssertHasText("token");
        ColAuthRobot.Auth.BasicAuthLogin.AssertIsHidden();
        ColAuthRobot.Auth.ClientCertificateType.AssertIsHidden();

        await TreeRobot.Select("COL1/VARS");
        ColVarsRobot.RootView.AssertIsVisible();
        var expectedColVars = expectedColVarsVms.Select(x => x.ToVariable()).ToList();
        var colVars = ColVarsRobot.VariablesVm.GetVariables(true);
        AssertCondition(Enumerable.SequenceEqual(expectedColVars, colVars));

        if (expectedEnv1VarsVms is not null)
        {
            await TreeRobot.Select("COL1/ENVS/ENV1");
            EnvRobot.RootView.AssertIsVisible();
            var expectedEnv1Vars = expectedEnv1VarsVms.Select(x => x.ToVariable()).ToList();
            var env1Vars = EnvRobot.VariablesVm.GetVariables(true);
            AssertCondition(Enumerable.SequenceEqual(expectedEnv1Vars, env1Vars));
        }

        if (expectedEnv2VarsVms is not null)
        {
            await TreeRobot.Select("COL1/ENVS/ENV2");
            EnvRobot.RootView.AssertIsVisible();
            var expectedEnv2Vars = expectedEnv2VarsVms.Select(x => x.ToVariable()).ToList();
            var env2Vars = EnvRobot.VariablesVm.GetVariables(true);
            AssertCondition(Enumerable.SequenceEqual(expectedEnv2Vars, env2Vars));
        }

        await TreeRobot.Select("COL1/HTTPHEADERS");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("GET");
        HttpRobot.Url.AssertHasText("{{BaseUrl}}/test/get/headers");
        HttpRobot.HttpVersion.AssertHasText("HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionNone);
        var actualHeaders = HttpRobot.ReqHeadersVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        AssertCondition(Enumerable.SequenceEqual(headers, actualHeaders));
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionNone);
    }

    #endregion

}