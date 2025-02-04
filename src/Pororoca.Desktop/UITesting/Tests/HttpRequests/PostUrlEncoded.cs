namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private async Task TestPostUrlEncodedBody()
    {
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/urlencoded");
        await HttpRobot.SetUrlEncodedBody(
        [
            new(true, "a", "xyz"),
            new(true, "b", "123"),
            new(false, "c", "false"),
            new(true, "c", "true"),
            new(true, "myIdSecret", "{{SpecialValue1}}")
        ]);
        await HttpRobot.ClickOnSendAndWaitForResponse();

        HttpRobot.ResTitle.AssertContainsText("Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "text/plain; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        HttpRobot.ResBodyRawContent.AssertContainsText("--- Received URL encoded params ---");
        HttpRobot.ResBodyRawContent.AssertContainsText("Content-Type: application/x-www-form-urlencoded");
        HttpRobot.ResBodyRawContent.AssertContainsText("Body: a=xyz&b=123&c=true&myIdSecret=Tail%C3%A2ndia");
        HttpRobot.ResBodySaveToFile.AssertIsVisible();
    }
}