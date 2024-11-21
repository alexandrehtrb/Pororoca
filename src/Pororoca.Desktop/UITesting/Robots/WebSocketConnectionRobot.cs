using Avalonia.Controls;
using AvaloniaEdit;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;
using Pororoca.Infrastructure.Features.WebSockets;
using static Pororoca.Domain.Features.Common.HttpVersionFormatter;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class WebSocketConnectionRobot : BaseNamedRobot
{
    public WebSocketConnectionRobot(WebSocketConnectionView rootView) : base(rootView) { }

    internal DataGrid ConnectionRequestHeaders => GetChildView<RequestHeadersTableView>("rhtvWsConnectionReqHeaders")!.FindControl<DataGrid>("datagrid")!;
    internal Button AddConnectionRequestHeader => GetChildView<Button>("btAddConnectionRequestHeader")!;
    internal Button AddSubprotocol => GetChildView<Button>("btAddSubprotocol")!;
    internal Button AddClientMessage => GetChildView<Button>("btAddWsCliMsg")!;
    internal Button ConnectDisconnectCancel => GetChildView<Button>("btConnectDisconnectCancel")!;
    internal Button DisableTlsVerification => GetChildView<Button>("btDisableTlsVerification")!;
    internal Button SendMessage => GetChildView<Button>("btSendMessage")!;
    internal ComboBox HttpVersion => GetChildView<ComboBox>("cbHttpVersion")!;
    internal ComboBox ConnectionRequestCompressionEnable => GetChildView<ComboBox>("cbWsConnReqCompressionEnable")!;
    internal ComboBox MessageToSend => GetChildView<ComboBox>("cbWsMsgToSend")!;
    internal ComboBox ConnectionRequestOptions => GetChildView<ComboBox>("cbWsScrReqOptions")!;
    internal ComboBoxItem ConnectionRequestOptionCompression => GetChildView<ComboBoxItem>("cbiWsScrReqOptionCompression")!;
    internal ComboBoxItem ConnectionRequestOptionHeaders => GetChildView<ComboBoxItem>("cbiWsScrReqOptionHeaders")!;
    internal ComboBoxItem ConnectionRequestOptionSubprotocols => GetChildView<ComboBoxItem>("cbiWsScrReqOptionSubprotocols")!;
    internal DataGrid Subprotocols => GetChildView<DataGrid>("dbSubprotocols")!;
    internal ListBox ExchangedMessages => GetChildView<ListBox>("ExchangedMessagesList")!;
    internal TextEditor MessageDetailContent => GetChildView<TextEditor>("SelectedExchangedMessageContentEditor")!;
    internal TabControl TabControlConnectionRequest => GetChildView<TabControl>("tabControlConnectionRequest")!;
    internal TabItem TabConnectionRequestAuth => GetChildView<TabItem>("tabItemConnectionRequestAuth")!;
    internal TabItem TabConnectionResponse => GetChildView<TabItem>("tabItemConnectionResponse")!;
    internal TextBlock ConnectionResponseStatusCode => GetChildView<TextBlock>("tbWebsocketResStatusCodeAndTime")!;
    internal DataGrid ConnectionResponseHeaders => GetChildView<ResponseHeadersTableView>("rhtvWebsocketResHeaders")!.FindControl<DataGrid>("datagrid")!;
    internal TabItem TabConnectionRequestOptions => GetChildView<TabItem>("tabItemConnectionRequestOptions")!;
    internal TextBox ConnectionRequestException => GetChildView<TextBox>("tbConnectionException")!;
    internal TextBox MessageDetailType => GetChildView<TextBox>("tbMessageDetailType")!;
    internal TextBox Url => GetChildView<TextBox>("tbUrl")!;
    internal TextBlock ErrorMsg => GetChildView<TextBlock>("tbErrorMsg")!;
    internal TextBlock IsWsConnected => GetChildView<TextBlock>("tbIsWsConnected")!;
    internal TextBlock MessageToSendError => GetChildView<TextBlock>("tbMessageToSendError")!;

    internal Task SetHttpVersion(decimal version)
    {
        string ver = FormatHttpVersion(version);
        return HttpVersion.Select(ver);
    }

    internal async Task ClickOnConnectAndWaitForConnection()
    {
        var vm = (WebSocketConnectionViewModel)RootView!.DataContext!;
        CancellationTokenSource cts = new(TimeSpan.FromMinutes(3));
        await ConnectDisconnectCancel.RaiseClickEvent();
        do
        {
            // don't make the value too low,
            // because the first request causes a little lag in the screen
            await Task.Delay(1500);
        }
        while (!cts.IsCancellationRequested && vm.ConnectionState == WebSocketConnectorState.Connecting);
    }

    internal async Task ClickOnDisconnectAndWaitForDisconnection()
    {
        var vm = (WebSocketConnectionViewModel)RootView!.DataContext!;
        CancellationTokenSource cts = new(TimeSpan.FromMinutes(3));
        await ConnectDisconnectCancel.RaiseClickEvent();
        do
        {
            // don't make the value too low,
            // because the first request causes a little lag in the screen
            await Task.Delay(750);
        }
        while (!cts.IsCancellationRequested && vm.ConnectionState == WebSocketConnectorState.Disconnecting);
    }
}