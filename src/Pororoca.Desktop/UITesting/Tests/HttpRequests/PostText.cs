using System.Collections.ObjectModel;
using System.Drawing.Text;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestPostRawTextBody()
    {
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/txt");
        await HttpRobot.SetRawBody("text/xml", "<XML><MyValue>{{SpecialValue1}}</MyValue></XML>");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "text/xml; charset=utf-8");
        AssertContainsResponseHeader("Content-Length", "40");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertHasText(HttpRobot.ResBodyRawContent, "<XML>" + Environment.NewLine + "  <MyValue>Tailândia</MyValue>" + Environment.NewLine + "</XML>");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }
}