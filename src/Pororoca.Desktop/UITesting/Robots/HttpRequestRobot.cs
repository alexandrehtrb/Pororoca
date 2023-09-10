using System.Globalization;
using Avalonia.Controls;
using AvaloniaEdit;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class HttpRequestRobot : BaseNamedRobot
{
    private RequestAuthRobot Auth { get; }

    public HttpRequestRobot(HttpRequestView rootView) : base(rootView) =>
        Auth = new(GetChildView<RequestAuthView>("reqAuthView")!);

    internal TabControl TabControlReq => GetChildView<TabControl>("tabControlReq")!;
    internal TabControl TabControlRes => GetChildView<TabControl>("tabControlRes")!;
    internal ComboBox HttpMethod => GetChildView<ComboBox>("cbHttpMethod")!;
    internal TextBox Url => GetChildView<TextBox>("tbUrl")!;
    internal TextBlock ErrorMsg => GetChildView<TextBlock>("tbErrorMsg")!;
    internal ComboBox HttpVersion => GetChildView<ComboBox>("cbHttpVersion")!;
    internal Button Send => GetChildView<Button>("btSendRequest")!;
    internal Button Cancel => GetChildView<Button>("btCancelRequest")!;
    internal TabItem TabReqHeaders => GetChildView<TabItem>("tabItemReqHeaders")!;
    internal Button AddReqHeader => GetChildView<Button>("btReqHeaderAdd")!;
    internal DataGrid ReqHeaders => GetChildView<DataGrid>("dgReqHeaders")!;
    internal TabItem TabReqBody => GetChildView<TabItem>("tabItemReqBody")!;
    internal ComboBox ReqBodyMode => GetChildView<ComboBox>("cbReqBodyMode")!;
    internal ComboBoxItem ReqBodyModeOptionNone => GetChildView<ComboBoxItem>("cbiReqBodyModeNone")!;
    internal ComboBoxItem ReqBodyModeOptionRaw => GetChildView<ComboBoxItem>("cbiReqBodyModeRaw")!;
    internal ComboBoxItem ReqBodyModeOptionFile => GetChildView<ComboBoxItem>("cbiReqBodyModeFile")!;
    internal ComboBoxItem ReqBodyModeOptionUrlEncoded => GetChildView<ComboBoxItem>("cbiReqBodyModeUrlEncoded")!;
    internal ComboBoxItem ReqBodyModeOptionFormData => GetChildView<ComboBoxItem>("cbiReqBodyModeFormData")!;
    internal ComboBoxItem ReqBodyModeOptionGraphQl => GetChildView<ComboBoxItem>("cbiReqBodyModeGraphQl")!;
    internal AutoCompleteBox ReqBodyRawContentType => GetChildView<AutoCompleteBox>("acbReqBodyRawContentType")!;
    internal TextEditor ReqBodyRawContent => GetChildView<TextEditor>("teReqBodyRawContent")!;
    internal AutoCompleteBox ReqBodyFileContentType => GetChildView<AutoCompleteBox>("acbReqBodyFileContentType")!;
    internal TextBox ReqBodyFileSrcPath => GetChildView<TextBox>("tbReqBodyFileSrcPath")!;
    internal Button ReqBodyFileSearch => GetChildView<Button>("btReqBodyFileSearch")!;
    internal Button ReqBodyUrlEncodedAddParam => GetChildView<Button>("btReqBodyUrlEncodedAddParam")!;
    internal DataGrid ReqBodyUrlEncodedParams => GetChildView<DataGrid>("dgReqBodyUrlEncodedParams")!;
    internal Button ReqBodyFormDataAddTextParam => GetChildView<Button>("btReqBodyFormDataAddTextParam")!;
    internal Button ReqBodyFormDataAddFileParam => GetChildView<Button>("btReqBodyFormDataAddFileParam")!;
    internal DataGrid ReqBodyFormDataParams => GetChildView<DataGrid>("dgReqBodyFormDataParams")!;
    internal TextBox ReqBodyGraphQlQuery => GetChildView<TextBox>("tbReqBodyGraphQlQuery")!;
    internal TextBox ReqBodyGraphQlVariables => GetChildView<TextBox>("tbReqBodyGraphQlVariables")!;
    internal TabItem TabReqAuth => GetChildView<TabItem>("tabItemReqAuth")!;
    internal TextBlock ResTitle => GetChildView<TextBlock>("tbResTitle")!;
    internal TabItem TabResHeaders => GetChildView<TabItem>("tabItemResHeaders")!;
    internal DataGrid ResHeaders => GetChildView<DataGrid>("dgResHeaders")!;
    internal TabItem TabResBody => GetChildView<TabItem>("tabItemResBody")!;
    internal TextEditor ResBodyRawContent => GetChildView<TextEditor>("ResponseBodyRawContentEditor")!;
    internal Button ResBodySaveToFile => GetChildView<Button>("btResBodySaveToFile")!;
    internal Button ResDisableTlsVerification => GetChildView<Button>("btResDisableTlsVerification")!;
    internal ProgressBar ResProgressBar => GetChildView<ProgressBar>("pbResProgressBar")!;

    internal Task SelectHttpVersion(decimal version)
    {
        string ver = version switch
        {
            1.0m => "HTTP/1.0",
            1.1m => "HTTP/1.1",
            2.0m => "HTTP/2",
            3.0m => "HTTP/3",
            _ => string.Format(CultureInfo.InvariantCulture, "HTTP/{0:0.0}", version)
        };
        return HttpVersion.Select(ver);
    }

    internal async Task SetRequestHeaders(IEnumerable<PororocaKeyValueParam> headers)
    {
        var vm = ((HttpRequestViewModel)RootView!.DataContext!).RequestHeadersTableVm;
        vm.Items.Clear();
        foreach (var header in headers)
        {
            vm.Items.Add(new(vm.Items, header));
        }
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task SelectEmptyBody()
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionNone);
    }

    internal async Task SelectRawBody(string? contentType, string content)
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionRaw);
        await ReqBodyRawContentType.Select(contentType);
        await ReqBodyRawContent.ClearAndTypeText(content);
    }

    internal async Task SelectFileBody(string? contentType, string fileSrcPath)
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionFile);
        await ReqBodyFileContentType.Select(contentType);
        await ReqBodyFileSrcPath.ClearAndTypeText(fileSrcPath);
    }

    internal async Task SelectUrlEncodedBody(IEnumerable<PororocaKeyValueParam> urlEncodedParams)
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionUrlEncoded);
        var vm = ((HttpRequestViewModel)RootView!.DataContext!).UrlEncodedParamsTableVm;
        vm.Items.Clear();
        foreach (var param in urlEncodedParams)
        {
            vm.Items.Add(new(vm.Items, param));
        }
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task SelectFormDataBody(IEnumerable<PororocaHttpRequestFormDataParam> formDataParams)
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionFormData);
        var vm = ((HttpRequestViewModel)RootView!.DataContext!).FormDataParamsTableVm;
        vm.Items.Clear();
        foreach (var param in formDataParams)
        {
            vm.Items.Add(new(vm.Items, param));
        }
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task SelectGraphQlBody()
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionGraphQl);
    }

    internal async Task ClickOnSendAndWaitForResponse()
    {
        var vm = (HttpRequestViewModel)RootView!.DataContext!;
        CancellationTokenSource cts = new(TimeSpan.FromMinutes(3));
        await Send.ClickOn();
        do
        {
            // don't make the value too low,
            // because the first request causes a little lag in the screen
            await Task.Delay(750);
        }
        while (!cts.IsCancellationRequested && vm.IsRequesting);
    }

    internal async Task SelectNoAuth()
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.AuthType.Select(Auth.AuthTypeOptionNone);
    }

    internal async Task SelectBasicAuth(string login, string password)
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.AuthType.Select(Auth.AuthTypeOptionBasic);
        await Auth.BasicAuthLogin.ClearAndTypeText(login);
        await Auth.BasicAuthPassword.ClearAndTypeText(password);
    }

    internal async Task SelectBearerAuth(string token)
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.AuthType.Select(Auth.AuthTypeOptionBearer);
        await Auth.BearerAuthToken.ClearAndTypeText(token);
    }

    internal async Task SelectPkcs12CertificateAuth(string certFilePath, string certPassword)
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.AuthType.Select(Auth.AuthTypeOptionClientCertificate);
        await Auth.ClientCertificateType.Select(Auth.ClientCertificateTypeOptionPkcs12);
        await Auth.ClientCertificatePkcs12FilePath.ClearAndTypeText(certFilePath);
        await Auth.ClientCertificatePkcs12FilePassword.ClearAndTypeText(certPassword);
    }

    internal async Task SelectPemCertificateAuth(string certFilePath, string prvKeyFilePath, string prvKeyPassword)
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.AuthType.Select(Auth.AuthTypeOptionClientCertificate);
        await Auth.ClientCertificateType.Select(Auth.ClientCertificateTypeOptionPem);
        await Auth.ClientCertificatePemCertificateFilePath.ClearAndTypeText(certFilePath);
        await Auth.ClientCertificatePemPrivateKeyFilePath.ClearAndTypeText(prvKeyFilePath);
        await Auth.ClientCertificatePemPrivateKeyPassword.ClearAndTypeText(prvKeyPassword);
    }
}