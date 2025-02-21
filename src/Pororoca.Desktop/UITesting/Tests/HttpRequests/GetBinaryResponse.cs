namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private async Task TestGetBinaryResponse()
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/img");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.ClickOnSendAndWaitForResponse();

        HttpRobot.ResTitle.AssertContainsText("Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "image/gif");
        AssertContainsResponseHeader("Content-Length", "888");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        HttpRobot.ResBodyRawContent.AssertContainsText("(binary content not shown, 888 bytes)");
        HttpRobot.ResBodySaveToFile.AssertIsVisible();
    }
}