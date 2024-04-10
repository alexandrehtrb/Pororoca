using Avalonia.Controls;
using AvaloniaEdit;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using static Pororoca.Domain.Features.Common.HttpVersionFormatter;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class HttpRequestRobot : BaseNamedRobot
{
    internal RequestAuthRobot Auth { get; }

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
    internal DataGrid ReqHeaders => GetChildView<RequestHeadersTableView>("rhtvReqHeaders")!.FindControl<DataGrid>("datagrid")!;
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
    internal DataGrid ResHeaders => GetChildView<ResponseHeadersTableView>("rhtvResHeaders")!.FindControl<DataGrid>("datagrid")!;
    internal TabItem TabResBody => GetChildView<TabItem>("tabItemResBody")!;
    internal DataGrid ResCaptures => GetChildView<DataGrid>("dgResCaptures")!;
    internal TabItem TabResCapture => GetChildView<TabItem>("tabItemResCapture")!;
    internal TextEditor ResBodyRawContent => GetChildView<TextEditor>("ResponseBodyRawContentEditor")!;
    internal Button ResAddCaptureHeader => GetChildView<Button>("btResCaptureAddHeaderCapture")!;
    internal Button ResAddCaptureBody => GetChildView<Button>("btResCaptureAddBodyCapture")!;
    internal Button ResBodySaveToFile => GetChildView<Button>("btResBodySaveToFile")!;
    internal Button ResDisableTlsVerification => GetChildView<Button>("btResDisableTlsVerification")!;
    internal ProgressBar ResProgressBar => GetChildView<ProgressBar>("pbResProgressBar")!;

    internal RequestHeadersDataGridViewModel ReqHeadersVm => ((HttpRequestViewModel)RootView!.DataContext!).RequestHeadersTableVm;
    internal KeyValueParamsDataGridViewModel UrlEncodedParamsVm => ((HttpRequestViewModel)RootView!.DataContext!).UrlEncodedParamsTableVm;
    internal FormDataParamsDataGridViewModel FormDataParamsVm => ((HttpRequestViewModel)RootView!.DataContext!).FormDataParamsTableVm;
    internal KeyValueParamsDataGridViewModel ResHeadersVm => ((HttpRequestViewModel)RootView!.DataContext!).ResponseDataCtx.ResponseHeadersAndTrailersTableVm;
    internal HttpResponseCapturesDataGridViewModel ResCapturesVm => ((HttpRequestViewModel)RootView!.DataContext!).ResCapturesTableVm;

    internal Task SetHttpVersion(decimal version)
    {
        string ver = FormatHttpVersion(version);
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

    internal async Task EditRequestHeaderAt(int index, bool enabled, string key, string value)
    {
        var vms = ReqHeadersVm.Items;
        var h = vms.ElementAt(index);
        h.Enabled = enabled;
        h.Key = key;
        h.Value = value;
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task SelectRequestHeaders(params RequestHeaderViewModel[] headersVms)
    {
        ReqHeaders.SelectedItems.Clear();
        foreach (var h in headersVms)
            ReqHeaders.SelectedItems.Add(h);
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task CutSelectedRequestHeaders()
    {
        ReqHeadersVm.CutOrCopySelected(false);
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task CopySelectedRequestHeaders()
    {
        ReqHeadersVm.CutOrCopySelected(true);
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task PasteRequestHeaders()
    {
        ReqHeadersVm.Paste();
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task DeleteSelectedRequestHeaders()
    {
        ReqHeadersVm.DeleteSelected();
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task SetEmptyBody()
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionNone);
    }

    internal async Task SetRawBody(string? contentType, string content)
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionRaw);
        await ReqBodyRawContentType.Select(contentType);
        await ReqBodyRawContent.ClearAndTypeText(content);
    }

    internal async Task SetFileBody(string? contentType, string fileSrcPath)
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionFile);
        await ReqBodyFileContentType.Select(contentType);
        await ReqBodyFileSrcPath.ClearAndTypeText(fileSrcPath);
    }

    internal async Task SetUrlEncodedBody(IEnumerable<PororocaKeyValueParam> urlEncodedParams)
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

    internal async Task EditUrlEncodedParamAt(int index, bool enabled, string key, string value)
    {
        var vms = UrlEncodedParamsVm.Items;
        var v = vms.ElementAt(index);
        v.Enabled = enabled;
        v.Key = key;
        v.Value = value;
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task SelectUrlEncodedParams(params KeyValueParamViewModel[] vms)
    {
        ReqBodyUrlEncodedParams.SelectedItems.Clear();
        foreach (var h in vms)
            ReqBodyUrlEncodedParams.SelectedItems.Add(h);
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task CutSelectedUrlEncodedParams()
    {
        UrlEncodedParamsVm.CutOrCopySelected(false);
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task CopySelectedUrlEncodedParams()
    {
        UrlEncodedParamsVm.CutOrCopySelected(true);
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task PasteUrlEncodedParams()
    {
        UrlEncodedParamsVm.Paste();
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task DeleteSelectedUrlEncodedParams()
    {
        UrlEncodedParamsVm.DeleteSelected();
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task SetFormDataBody(IEnumerable<PororocaHttpRequestFormDataParam> formDataParams)
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

    internal async Task SelectFormDataParams(params FormDataParamViewModel[] vms)
    {
        ReqBodyFormDataParams.SelectedItems.Clear();
        foreach (var h in vms)
            ReqBodyFormDataParams.SelectedItems.Add(h);
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task CutSelectedFormDataParams()
    {
        FormDataParamsVm.CutOrCopySelected(false);
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task CopySelectedFormDataParams()
    {
        FormDataParamsVm.CutOrCopySelected(true);
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task PasteFormDataParams()
    {
        FormDataParamsVm.Paste();
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task DeleteSelectedFormDataParams()
    {
        FormDataParamsVm.DeleteSelected();
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task SetGraphQlBody(string query, string variables)
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionGraphQl);
        await ReqBodyGraphQlQuery.ClearAndTypeText(query);
        await ReqBodyGraphQlVariables.ClearAndTypeText(variables);
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

    internal async Task EditResponseCaptureAt(int index, string targetVar, string headerNameOrBodyPath)
    {
        var vms = ResCapturesVm.Items;
        var v = vms.ElementAt(index);
        v.TargetVariable = targetVar;
        v.HeaderNameOrBodyPath = headerNameOrBodyPath;
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task SetNoAuth()
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.AuthType.Select(Auth.AuthTypeOptionNone);
    }

    internal async Task SetInheritFromCollectionAuth()
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.AuthType.Select(Auth.AuthTypeOptionInheritFromCollection);
    }

    internal async Task SetBasicAuth(string login, string password)
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.SetBasicAuth(login, password);
    }

    internal async Task SetBearerAuth(string token)
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.SetBearerAuth(token);
    }

    internal async Task SetWindowsAuthCurrentUser()
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.SetWindowsAuthCurrentUser();
    }

    internal async Task SetWindowsAuthOtherUser(string login, string password, string domain)
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.SetWindowsAuthOtherUser(login, password, domain);
    }

    internal async Task SetPkcs12CertificateAuth(string certFilePath, string certPassword)
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.SetPkcs12CertificateAuth(certFilePath, certPassword);
    }

    internal async Task SetPemCertificateAuth(string certFilePath, string prvKeyFilePath, string prvKeyPassword)
    {
        await TabControlReq.Select(TabReqAuth);
        await Auth.SetPemCertificateAuth(certFilePath, prvKeyFilePath, prvKeyPassword);
    }

    internal async Task SelectResponseHeaders(params KeyValueParamViewModel[] headersVms)
    {
        ResHeaders.SelectedItems.Clear();
        foreach (var h in headersVms)
            ResHeaders.SelectedItems.Add(h);
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task CopySelectedResponseHeaders()
    {
        ResHeadersVm.CutOrCopySelected(true);
        await UITestActions.WaitAfterActionAsync();
    }
}