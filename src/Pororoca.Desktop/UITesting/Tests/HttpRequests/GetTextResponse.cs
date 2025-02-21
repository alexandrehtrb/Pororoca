namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private async Task TestGetTextResponse()
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/txt");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.ClickOnSendAndWaitForResponse();

        HttpRobot.ResTitle.AssertContainsText("Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Length");
        AssertContainsResponseHeader("Content-Disposition", "attachment; filename=ascii.txt; filename*=UTF-8''ascii.txt");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        HttpRobot.ResBodyRawContent.AssertContainsText("Cross-Stitch Pattern");
        HttpRobot.ResBodySaveToFile.AssertIsVisible();
    }
}