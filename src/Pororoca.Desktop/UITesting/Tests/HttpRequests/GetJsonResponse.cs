namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private async Task TestGetJsonResponse()
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/json");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.ClickOnSendAndWaitForResponse();

        HttpRobot.ResTitle.AssertContainsText("Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "application/json; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        HttpRobot.ResBodyRawContent.AssertHasText("{" + Environment.NewLine + "  \"id\": 1" + Environment.NewLine + "}");
        HttpRobot.ResBodySaveToFile.AssertIsVisible();
    }
}