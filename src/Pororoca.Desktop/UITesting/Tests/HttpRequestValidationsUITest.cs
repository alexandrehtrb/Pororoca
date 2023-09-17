using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class HttpRequestValidationsUITest : UITest
{
    private static readonly (string Key, string BaseUrl) TestServer =
        ("BaseUrl", "https://localhost:5001");
    
    private const string TestUrl = "{{BaseUrl}}/test/get/json";

    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private EnvironmentRobot EnvRobot { get; }
    private HttpRequestRobot HttpRobot { get; }

    public HttpRequestValidationsUITest()
    {
        RootView = (Control) MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
    }

    public override async Task RunAsync()
    {
        // create a collection with many different items
        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL1");

        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTP1");

        await TestUrlValidation();
        await TestHttpVersionValidation();
        await TestRawBodyValidation();
        await TestFileBodyValidation();
        await TestFormDataValidation();
        /*
        IMPORTANT: For client certificate tests,
        create TestFiles/ClientCertificates folder inside PororocaUserData folder 
        and paste test cert files.
        */
        await TestClientCertificatePkcs12Validation();
        await TestClientCertificatePemValidation();
    }

    private async Task TestUrlValidation()
    {
        async Task TestBadUrl(string url)
        {
            await HttpRobot.Url.ClearAndTypeText(url);
            AssertIsHidden(HttpRobot.ErrorMsg);
            AssertDoesntHaveStyleClass(HttpRobot.Url, "HasValidationProblem");
            await HttpRobot.Send.ClickOn();
            AssertIsVisible(HttpRobot.ErrorMsg);
            AssertHasText(HttpRobot.ErrorMsg, "Invalid URL. Please, check it and try again.");
            AssertHasStyleClass(HttpRobot.Url, "HasValidationProblem");
        }

        // bad url, non-parameterized
        await TestBadUrl("dasdasdasd");
        await TestBadUrl("ftp://192.168.0.2");
        // bad url, with variable
        // variable resolving will be checked in other tests
        await TestBadUrl("{{MyDomain}}");
        
        await HttpRobot.Url.ClearAndTypeText(TestServer.BaseUrl);
        AssertIsHidden(HttpRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(HttpRobot.Url, "HasValidationProblem");
    }

    private async Task TestHttpVersionValidation()
    {
        if (!AvailablePororocaRequestSelectionOptions.IsHttpVersionAvailableInOS(2.0m, out _))
        {
            await HttpRobot.SetHttpVersion(2.0m);
            AssertIsHidden(HttpRobot.ErrorMsg);
            AssertDoesntHaveStyleClass(HttpRobot.HttpVersion, "HasValidationProblem");
            await HttpRobot.Url.ClearAndTypeText(TestUrl);
            await HttpRobot.Send.ClickOn();
            AssertIsVisible(HttpRobot.ErrorMsg);
            AssertHasText(HttpRobot.ErrorMsg, "On Windows, support for HTTP/2 requires Windows 10 or greater.");
            AssertHasStyleClass(HttpRobot.HttpVersion, "HasValidationProblem");
        }

        if (!AvailablePororocaRequestSelectionOptions.IsHttpVersionAvailableInOS(3.0m, out _))
        {
            await HttpRobot.SetHttpVersion(3.0m);
            AssertIsHidden(HttpRobot.ErrorMsg);
            AssertDoesntHaveStyleClass(HttpRobot.HttpVersion, "HasValidationProblem");
            await HttpRobot.Url.ClearAndTypeText(TestUrl);
            await HttpRobot.Send.ClickOn();
            AssertIsVisible(HttpRobot.ErrorMsg);
            AssertHasText(HttpRobot.ErrorMsg, "HTTP/3 is only available for Linux or Windows 11 and greater.");
            AssertHasStyleClass(HttpRobot.HttpVersion, "HasValidationProblem");
        }

        await HttpRobot.SetHttpVersion(1.1m);
        AssertIsHidden(HttpRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(HttpRobot.HttpVersion, "HasValidationProblem");
    }

    private async Task TestRawBodyValidation()
    {
        // blank content-type
        await HttpRobot.SetRawBody(null, string.Empty);
        AssertIsHidden(HttpRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(HttpRobot.ReqBodyRawContentType, "HasValidationProblem");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "The request body requires a Content-Type.");
        AssertHasStyleClass(HttpRobot.ReqBodyRawContentType, "HasValidationProblem");
        AssertIsVisible(HttpRobot.ReqBodyRawContentType); // input field should be visible

        // invalid content-type
        await HttpRobot.SetRawBody("gdgagadg", string.Empty);
        AssertIsHidden(HttpRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(HttpRobot.ReqBodyRawContentType, "HasValidationProblem");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "Use one of the available Content-Types for the request body.");
        AssertHasStyleClass(HttpRobot.ReqBodyRawContentType, "HasValidationProblem");
        AssertIsVisible(HttpRobot.ReqBodyRawContentType); // input field should be visible

        await HttpRobot.SetRawBody("application/json", string.Empty);
        AssertIsHidden(HttpRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(HttpRobot.ReqBodyRawContentType, "HasValidationProblem");
    }

    private async Task TestFileBodyValidation()
    {
        // blank content-type
        await HttpRobot.SetFileBody(null, string.Empty);
        AssertIsHidden(HttpRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(HttpRobot.ReqBodyFileContentType, "HasValidationProblem");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "The request body requires a Content-Type.");
        AssertHasStyleClass(HttpRobot.ReqBodyFileContentType, "HasValidationProblem");
        AssertIsVisible(HttpRobot.ReqBodyFileContentType); // input field should be visible

        // invalid content-type
        await HttpRobot.SetFileBody("gdgagadg", string.Empty);
        AssertIsHidden(HttpRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(HttpRobot.ReqBodyFileContentType, "HasValidationProblem");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "Use one of the available Content-Types for the request body.");
        AssertHasStyleClass(HttpRobot.ReqBodyFileContentType, "HasValidationProblem");
        AssertIsVisible(HttpRobot.ReqBodyFileContentType); // input field should be visible

        // file not found
        await HttpRobot.SetFileBody("application/json", "K:\\FILES\\file.json");
        AssertIsHidden(HttpRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(HttpRobot.ReqBodyFileSrcPath, "HasValidationProblem");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "File for request body not found.");
        AssertHasStyleClass(HttpRobot.ReqBodyFileSrcPath, "HasValidationProblem");
        AssertIsVisible(HttpRobot.ReqBodyFileSrcPath); // input field should be visible

        await HttpRobot.SetFileBody("application/json", string.Empty);
        AssertIsHidden(HttpRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(HttpRobot.ReqBodyFileSrcPath, "HasValidationProblem");
    }

    private async Task TestFormDataValidation()
    {
        // invalid or blank content-type
        await HttpRobot.SetFormDataBody(Array.Empty<PororocaHttpRequestFormDataParam>());
        await HttpRobot.ReqBodyFormDataAddTextParam.ClickOn();
        var paramVm = HttpRobot.ReqBodyFormDataParams.ItemsSource.Cast<FormDataParamViewModel>().First();
        
        paramVm.ContentType = string.Empty;
        await UITestActions.WaitAfterActionAsync();
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "One of the Form Data parameters has an invalid Content-Type.");
        // TODO: AssertIsVisible(HttpRobot.ReqBodyFormDataParams); // input field should be visible

        paramVm.ContentType = "dagadgadgad";
        await UITestActions.WaitAfterActionAsync();
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "One of the Form Data parameters has an invalid Content-Type.");
        // TODO: AssertIsVisible(HttpRobot.ReqBodyFormDataParams); // input field should be visible

        await HttpRobot.SetEmptyBody();
    }

    private async Task TestClientCertificatePkcs12Validation()
    {
        // file not found
        await HttpRobot.SetPkcs12CertificateAuth("K:\\FILES\\cert.p12", string.Empty);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "Client certificate file not found.");
        // TODO: AssertIsVisible(HttpRobot.Auth.RootView); // input field should be visible
        // password cannot be blank
        string certFilePath = GetTestFilePath("ClientCertificates", "badssl.com-client.p12");
        await HttpRobot.SetPkcs12CertificateAuth(certFilePath, string.Empty);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "PKCS#12 client certificates need a password.");
        // TODO: AssertIsVisible(HttpRobot.Auth.RootView); // input field should be visible
    }

    private async Task TestClientCertificatePemValidation()
    {
        // certificate file not found
        await HttpRobot.SetPemCertificateAuth("K:\\FILES\\cert.pem", string.Empty, string.Empty);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "Client certificate file not found.");
        // TODO: AssertIsVisible(HttpRobot.Auth.RootView); // input field should be visible

        // private key file specified, but not found
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        string certFilePath = GetTestFilePath("ClientCertificates", "badssl.com-client-certificate-without-private-key.pem");
        string prvKeyFilePath = GetTestFilePath("ClientCertificates", "dgjsdjkg.key");
        await HttpRobot.SetPemCertificateAuth(certFilePath, prvKeyFilePath, string.Empty);
        await HttpRobot.Send.ClickOn();
        AssertIsVisible(HttpRobot.ErrorMsg);
        AssertHasText(HttpRobot.ErrorMsg, "Client certificate private key file not found.");
        // TODO: AssertIsVisible(HttpRobot.Auth.RootView); // input field should be visible
    }

    private static string GetTestFilePath(string subFolder, string fileName)
    {
        var userDataDir = UserDataManager.GetUserDataFolder();
        return Path.Combine(userDataDir.FullName, "PororocaUserData", "TestFiles", subFolder, fileName);
    }
}