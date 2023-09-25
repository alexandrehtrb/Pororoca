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
    private async Task TestPostEmptyBody()
    {
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/none");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.ClickOnSendAndWaitForResponse();

        AssertContainsText(HttpRobot.ResTitle, "Response: 204 NoContent");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertHasText(HttpRobot.ResBodyRawContent, string.Empty);
        AssertIsHidden(HttpRobot.ResBodySaveToFile);
    }
}