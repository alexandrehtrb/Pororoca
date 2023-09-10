using Avalonia.Controls;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class WebSocketConnectionRobot : BaseNamedRobot
{
    public WebSocketConnectionRobot(WebSocketConnectionView rootView) : base(rootView){}

    internal Button AddWsClientMsg => GetChildView<Button>("btAddWsCliMsg")!;
    internal DataGrid ConnectionReqHeaders => GetChildView<DataGrid>("dgWsConnectionReqHeaders")!;
}