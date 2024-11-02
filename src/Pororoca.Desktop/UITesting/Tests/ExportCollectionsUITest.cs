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

        AssertHasText(ExportColRobot.CollectionName, "COL1");

        await ExportColRobot.SetIncludeColSecretVars(exportWithColVarsSecrets);
        await ExportColRobot.SelectExportFormat(format);
        await ExportColRobot.SetEnvironmentToExport(includeEnv1InExport, "ENV1", includeEnv1Secrets);
        await ExportColRobot.SetEnvironmentToExport(includeEnv2InExport, "ENV2", includeEnv2Secrets);
    }

    private async Task<string> ExportCollection(ExportCollectionFormat format)
    {
        await TreeRobot.Select("COL1");
        var collection = ((CollectionViewModel)ColRobot.RootView!.DataContext!).ToCollection(forExporting: true);
        return format switch
        {
            ExportCollectionFormat.Pororoca => Encoding.UTF8.GetString(PororocaCollectionExporter.ExportAsPororocaCollection(collection)),
            ExportCollectionFormat.Postman => PostmanCollectionV21Exporter.ExportAsPostmanCollectionV21(collection),
            _ => string.Empty
        };
    }

    private void ClearAllCollections()
    {
        MainWindowVm.CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = null;
        MainWindowVm.CollectionsGroupViewDataCtx.Items.Clear();
        AssertTreeItemNotExists(CollectionsGroup, "COL1");
    }

    private void RestoreCollection(ExportCollectionFormat format, string json)
    {
        var mwvm = ((MainWindowViewModel)MainWindow.Instance!.DataContext!);
        if (format == ExportCollectionFormat.Pororoca)
        {
            Assert(PororocaCollectionImporter.TryImportPororocaCollection(json, preserveId: true, out var reimportedCollection));
            mwvm.AddCollection(reimportedCollection!, showItemInScreen: true);
        }
        else if (format == ExportCollectionFormat.Postman)
        {
            Assert(PostmanCollectionV21Importer.TryImportPostmanCollection(json, out var reimportedCollection));
            mwvm.AddCollection(reimportedCollection!, showItemInScreen: true);
        }
    }

    private async Task AssertCollectionReimportedSuccessfully(ObservableCollection<VariableViewModel> expectedColVarsVms, ObservableCollection<VariableViewModel>? expectedEnv1VarsVms, ObservableCollection<VariableViewModel>? expectedEnv2VarsVms)
    {
        AssertTreeItemExists(CollectionsGroup, "COL1");
        AssertTreeItemExists(CollectionsGroup, "COL1/AUTH");
        if (expectedEnv1VarsVms is not null)
        {
            AssertTreeItemExists(CollectionsGroup, "COL1/ENVS/ENV1");
        }
        else
        {
            AssertTreeItemNotExists(CollectionsGroup, "COL1/ENVS/ENV1");
        }
        if (expectedEnv2VarsVms is not null)
        {
            AssertTreeItemExists(CollectionsGroup, "COL1/ENVS/ENV2");
        }
        else
        {
            AssertTreeItemNotExists(CollectionsGroup, "COL1/ENVS/ENV2");
        }
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTPHEADERS");

        await TreeRobot.Select("COL1");
        AssertIsVisible(ColRobot.RootView);

        await TreeRobot.Select("COL1/AUTH");
        AssertIsVisible(ColAuthRobot.RootView);
        AssertSelection(ColAuthRobot.Auth.AuthType, ColAuthRobot.Auth.AuthTypeOptionBearer);
        AssertIsVisible(ColAuthRobot.Auth.BearerAuthToken);
        AssertHasText(ColAuthRobot.Auth.BearerAuthToken, "token");
        AssertIsHidden(ColAuthRobot.Auth.BasicAuthLogin);
        AssertIsHidden(ColAuthRobot.Auth.ClientCertificateType);

        await TreeRobot.Select("COL1/VARS");
        AssertIsVisible(ColVarsRobot.RootView);
        var expectedColVars = expectedColVarsVms.Select(x => x.ToVariable()).ToList();
        var colVars = ColVarsRobot.VariablesVm.GetVariables(true);
        Assert(Enumerable.SequenceEqual(expectedColVars, colVars));

        if (expectedEnv1VarsVms is not null)
        {
            await TreeRobot.Select("COL1/ENVS/ENV1");
            AssertIsVisible(EnvRobot.RootView);
            var expectedEnv1Vars = expectedEnv1VarsVms.Select(x => x.ToVariable()).ToList();
            var env1Vars = EnvRobot.VariablesVm.GetVariables(true);
            Assert(Enumerable.SequenceEqual(expectedEnv1Vars, env1Vars));
        }

        if (expectedEnv2VarsVms is not null)
        {
            await TreeRobot.Select("COL1/ENVS/ENV2");
            AssertIsVisible(EnvRobot.RootView);
            var expectedEnv2Vars = expectedEnv2VarsVms.Select(x => x.ToVariable()).ToList();
            var env2Vars = EnvRobot.VariablesVm.GetVariables(true);
            Assert(Enumerable.SequenceEqual(expectedEnv2Vars, env2Vars));
        }

        await TreeRobot.Select("COL1/HTTPHEADERS");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "GET");
        AssertHasText(HttpRobot.Url, "{{BaseUrl}}/test/get/headers");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionNone);
        var actualHeaders = HttpRobot.ReqHeadersVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(Enumerable.SequenceEqual(headers, actualHeaders));
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionNone);
    }

    #endregion

}