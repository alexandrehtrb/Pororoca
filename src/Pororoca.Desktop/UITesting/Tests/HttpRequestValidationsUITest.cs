using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class HttpRequestValidationsUITest : PororocaUITest
{
    private const string TestValidUrl = "https://localhost:5001";

    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private EnvironmentRobot EnvRobot { get; }
    private HttpRequestRobot HttpRobot { get; }

    public HttpRequestValidationsUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
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
        await TestWindowsAuthValidation();
        await TestClientCertificatePkcs12Validation();
        await TestClientCertificatePemValidation();
    }

    private async Task TestUrlValidation()
    {
        async Task TestBadUrl(string url)
        {
            await HttpRobot.Url.ClearAndTypeText(url);
            HttpRobot.ErrorMsg.AssertIsHidden();
            HttpRobot.Url.AssertDoesntHaveStyleClass("HasValidationProblem");
            await HttpRobot.SendOrCancel.RaiseClickEvent();
            await Wait(0.5);
            HttpRobot.ErrorMsg.AssertIsVisible();
            HttpRobot.ErrorMsg.AssertHasText("Invalid URL. Please, check it and try again.");
            HttpRobot.Url.AssertHasStyleClass("HasValidationProblem");
        }

        // bad url, non-parameterized
        await TestBadUrl("dasdasdasd");
        await TestBadUrl("ftp://192.168.0.2");
        // bad url, with variable
        // variable resolving will be checked in other tests
        await TestBadUrl("{{MyDomain}}");

        await HttpRobot.Url.ClearAndTypeText(TestValidUrl);
        HttpRobot.ErrorMsg.AssertIsHidden();
        HttpRobot.Url.AssertDoesntHaveStyleClass("HasValidationProblem");
    }

    private async Task TestHttpVersionValidation()
    {
        if (!AvailablePororocaRequestSelectionOptions.IsHttpVersionAvailableInOS(2.0m, out _))
        {
            await HttpRobot.SetHttpVersion(2.0m);
            HttpRobot.ErrorMsg.AssertIsHidden();
            HttpRobot.HttpVersion.AssertDoesntHaveStyleClass("HasValidationProblem");
            await HttpRobot.Url.ClearAndTypeText(TestValidUrl);
            await HttpRobot.SendOrCancel.RaiseClickEvent();
            await Wait(0.5);
            HttpRobot.ErrorMsg.AssertIsVisible();
            HttpRobot.ErrorMsg.AssertHasText("On Windows, support for HTTP/2 requires Windows 10 or greater.");
            HttpRobot.HttpVersion.AssertHasStyleClass("HasValidationProblem");
        }

        if (!AvailablePororocaRequestSelectionOptions.IsHttpVersionAvailableInOS(3.0m, out _))
        {
            await HttpRobot.SetHttpVersion(3.0m);
            HttpRobot.ErrorMsg.AssertIsHidden();
            HttpRobot.HttpVersion.AssertDoesntHaveStyleClass("HasValidationProblem");
            await HttpRobot.Url.ClearAndTypeText(TestValidUrl);
            await HttpRobot.SendOrCancel.RaiseClickEvent();
            await Wait(0.5);
            HttpRobot.ErrorMsg.AssertIsVisible();
            HttpRobot.ErrorMsg.AssertHasText("HTTP/3 is only available for Linux with msquic or Windows 11 and greater.");
            HttpRobot.HttpVersion.AssertHasStyleClass("HasValidationProblem");
        }

        await HttpRobot.SetHttpVersion(1.1m);
        HttpRobot.ErrorMsg.AssertIsHidden();
        HttpRobot.HttpVersion.AssertDoesntHaveStyleClass("HasValidationProblem");
    }

    private async Task TestRawBodyValidation()
    {
        // blank content-type
        await HttpRobot.SetRawBody(null, string.Empty);
        HttpRobot.ErrorMsg.AssertIsHidden();
        HttpRobot.ReqBodyRawContentType.AssertDoesntHaveStyleClass("HasValidationProblem");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("The request body requires a Content-Type.");
        HttpRobot.ReqBodyRawContentType.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.ReqBodyRawContentType.AssertIsVisible(); // input field should be visible

        // invalid content-type
        await HttpRobot.SetRawBody("gdgagadg", string.Empty);
        HttpRobot.ErrorMsg.AssertIsHidden();
        HttpRobot.ReqBodyRawContentType.AssertDoesntHaveStyleClass("HasValidationProblem");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("Use one of the available Content-Types for the request body.");
        HttpRobot.ReqBodyRawContentType.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.ReqBodyRawContentType.AssertIsVisible(); // input field should be visible

        await HttpRobot.SetRawBody("application/json", string.Empty);
        HttpRobot.ErrorMsg.AssertIsHidden();
        HttpRobot.ReqBodyRawContentType.AssertDoesntHaveStyleClass("HasValidationProblem");
    }

    private async Task TestFileBodyValidation()
    {
        // blank content-type
        await HttpRobot.SetFileBody(null, string.Empty);
        HttpRobot.ErrorMsg.AssertIsHidden();
        HttpRobot.ReqBodyFileContentType.AssertDoesntHaveStyleClass("HasValidationProblem");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("The request body requires a Content-Type.");
        HttpRobot.ReqBodyFileContentType.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.ReqBodyFileContentType.AssertIsVisible(); // input field should be visible

        // invalid content-type
        await HttpRobot.SetFileBody("gdgagadg", string.Empty);
        HttpRobot.ErrorMsg.AssertIsHidden();
        HttpRobot.ReqBodyFileContentType.AssertDoesntHaveStyleClass("HasValidationProblem");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("Use one of the available Content-Types for the request body.");
        HttpRobot.ReqBodyFileContentType.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.ReqBodyFileContentType.AssertIsVisible(); // input field should be visible

        // file not found
        await HttpRobot.SetFileBody("application/json", "K:\\FILES\\file.json");
        HttpRobot.ErrorMsg.AssertIsHidden();
        HttpRobot.ReqBodyFileSrcPath.AssertDoesntHaveStyleClass("HasValidationProblem");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("File for request body not found.");
        HttpRobot.ReqBodyFileSrcPath.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.ReqBodyFileSrcPath.AssertIsVisible(); // input field should be visible

        await HttpRobot.SetFileBody("application/json", string.Empty);
        HttpRobot.ErrorMsg.AssertIsHidden();
        HttpRobot.ReqBodyFileSrcPath.AssertDoesntHaveStyleClass("HasValidationProblem");
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
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("One of the Form Data parameters has an invalid Content-Type.");
        // TODO: HttpRobot.ReqBodyFormDataParams.AssertIsVisible(); // input field should be visible

        paramVm.ContentType = "dagadgadgad";
        await UITestActions.WaitAfterActionAsync();
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("One of the Form Data parameters has an invalid Content-Type.");
        // TODO: HttpRobot.ReqBodyFormDataParams.AssertIsVisible(); // input field should be visible

        await HttpRobot.SetEmptyBody();
    }

    private async Task TestClientCertificatePkcs12Validation()
    {
        // file not found
        await HttpRobot.SetPkcs12CertificateAuth("K:\\FILES\\cert.p12", string.Empty);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("Client certificate file not found.");
        HttpRobot.Auth.ClientCertificatePkcs12FilePath.AssertIsVisible();
        HttpRobot.Auth.ClientCertificatePkcs12FilePath.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.Auth.ClientCertificatePkcs12FilePassword.AssertDoesntHaveStyleClass("HasValidationProblem");
        // password cannot be blank
        string certFilePath = GetTestFilePath("ClientCertificates", "badssl.com-client.p12");
        await HttpRobot.SetPkcs12CertificateAuth(certFilePath, string.Empty);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("PKCS#12 client certificates need a password.");
        HttpRobot.Auth.ClientCertificatePkcs12FilePassword.AssertIsVisible();
        HttpRobot.Auth.ClientCertificatePkcs12FilePassword.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.Auth.ClientCertificatePkcs12FilePath.AssertDoesntHaveStyleClass("HasValidationProblem");

        await HttpRobot.SetNoAuth();
        HttpRobot.ErrorMsg.AssertIsHidden();
    }

    private async Task TestClientCertificatePemValidation()
    {
        // certificate file not found
        await HttpRobot.SetPemCertificateAuth("K:\\FILES\\cert.pem", string.Empty, string.Empty);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("Client certificate file not found.");
        HttpRobot.Auth.ClientCertificatePemCertificateFilePath.AssertIsVisible();
        HttpRobot.Auth.ClientCertificatePemCertificateFilePath.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.Auth.ClientCertificatePemPrivateKeyFilePath.AssertDoesntHaveStyleClass("HasValidationProblem");

        // private key file specified, but not found
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        string certFilePath = GetTestFilePath("ClientCertificates", "badssl.com-client-certificate-without-private-key.pem");
        string prvKeyFilePath = GetTestFilePath("ClientCertificates", "dgjsdjkg.key");
        await HttpRobot.SetPemCertificateAuth(certFilePath, prvKeyFilePath, string.Empty);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("Client certificate private key file not found.");
        HttpRobot.Auth.ClientCertificatePemPrivateKeyFilePath.AssertIsVisible();
        HttpRobot.Auth.ClientCertificatePemPrivateKeyFilePath.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.Auth.ClientCertificatePemCertificateFilePath.AssertDoesntHaveStyleClass("HasValidationProblem");

        await HttpRobot.SetNoAuth();
        HttpRobot.ErrorMsg.AssertIsHidden();
    }

    private async Task TestWindowsAuthValidation()
    {
        // windows login blank
        await HttpRobot.SetWindowsAuthOtherUser(string.Empty, "pwd", "domain");
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("The login for Windows authentication cannot be blank.");
        HttpRobot.Auth.WindowsAuthLogin.AssertIsVisible();
        HttpRobot.Auth.WindowsAuthLogin.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.Auth.WindowsAuthPassword.AssertDoesntHaveStyleClass("HasValidationProblem");
        HttpRobot.Auth.WindowsAuthDomain.AssertDoesntHaveStyleClass("HasValidationProblem");

        // windows password blank
        await HttpRobot.SetWindowsAuthOtherUser("usr", string.Empty, "domain");
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("The password for Windows authentication cannot be blank.");
        HttpRobot.Auth.WindowsAuthPassword.AssertIsVisible();
        HttpRobot.Auth.WindowsAuthLogin.AssertDoesntHaveStyleClass("HasValidationProblem");
        HttpRobot.Auth.WindowsAuthPassword.AssertHasStyleClass("HasValidationProblem");
        HttpRobot.Auth.WindowsAuthDomain.AssertDoesntHaveStyleClass("HasValidationProblem");

        // windows domain blank
        await HttpRobot.SetWindowsAuthOtherUser("usr", "pwd", string.Empty);
        await HttpRobot.SendOrCancel.RaiseClickEvent();
        await Wait(0.5);
        HttpRobot.ErrorMsg.AssertIsVisible();
        HttpRobot.ErrorMsg.AssertHasText("The domain for Windows authentication cannot be blank.");
        HttpRobot.Auth.WindowsAuthDomain.AssertIsVisible();
        HttpRobot.Auth.WindowsAuthLogin.AssertDoesntHaveStyleClass("HasValidationProblem");
        HttpRobot.Auth.WindowsAuthPassword.AssertDoesntHaveStyleClass("HasValidationProblem");
        HttpRobot.Auth.WindowsAuthDomain.AssertHasStyleClass("HasValidationProblem");

        await HttpRobot.SetWindowsAuthCurrentUser();
        HttpRobot.ErrorMsg.AssertIsHidden();

        await HttpRobot.SetNoAuth();
        HttpRobot.ErrorMsg.AssertIsHidden();
    }
}