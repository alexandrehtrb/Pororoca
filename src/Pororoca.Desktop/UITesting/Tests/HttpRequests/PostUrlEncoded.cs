using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestPostUrlEncodedBody()
    {
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/urlencoded");
        await HttpRobot.SelectUrlEncodedBody(new PororocaKeyValueParam[]
        {
            new(true, "a", "xyz"),
            new(true, "b", "123"),
            new(false, "c", "false"),
            new(true, "c", "true"),
            new(true, "myIdSecret", "{{SpecialValue1}}")
        });
        await HttpRobot.ClickOnSendAndWaitForResponse();

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "text/plain; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertContainsText(HttpRobot.ResBodyRawContent, "--- Received URL encoded params ---");
        AssertContainsText(HttpRobot.ResBodyRawContent, "Content-Type: application/x-www-form-urlencoded");
        AssertContainsText(HttpRobot.ResBodyRawContent, "Body: a=xyz&b=123&c=true&myIdSecret=Tail%C3%A2ndia");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }
}