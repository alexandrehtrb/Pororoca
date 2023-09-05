using System.Globalization;
using Avalonia.Controls;
using AvaloniaEdit;
using Pororoca.Desktop.Views;

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
    internal TextBox ResTitle => GetChildView<TextBox>("tbResTitle")!;
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

    internal async Task SelectUrlEncodedBody()
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionUrlEncoded);
    }

    internal async Task SelectFormDataBody()
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionFormData);
    }

    internal async Task SelectGraphQlBody()
    {
        await TabControlReq.Select(TabReqBody);
        await ReqBodyMode.Select(ReqBodyModeOptionGraphQl);
    }
}