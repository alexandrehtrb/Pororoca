namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestGetBinaryResponse()
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/img");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.ClickOnSendAndWaitForResponse();

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "image/gif");
        AssertContainsResponseHeader("Content-Length", "888");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertContainsText(HttpRobot.ResBodyRawContent, "(binary content not shown, 888 bytes)");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }
}