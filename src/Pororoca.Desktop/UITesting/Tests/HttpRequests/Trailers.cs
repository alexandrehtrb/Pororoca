namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestTrailers()
    {
        if (HttpRobot.HttpVersion.SelectedItem is string httpVersion && httpVersion.StartsWith("HTTP/1"))
            return; // only HTTP/2 and HTTP/3 have trailers

        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/trailers");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.ClickOnSendAndWaitForResponse();

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "application/json; charset=utf-8");
        AssertContainsResponseHeader("mytrailer", "MyTrailerValue");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertHasText(HttpRobot.ResBodyRawContent, "{" + Environment.NewLine + "  \"id\": 1" + Environment.NewLine + "}");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }
}