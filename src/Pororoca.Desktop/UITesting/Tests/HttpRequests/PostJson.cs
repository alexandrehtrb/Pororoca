namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestPostRawJsonBody()
    {
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/json");
        await HttpRobot.SetRawBody("application/json", "{\"myValue\":\"{{SpecialValue1}}\"}");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "application/json; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertHasText(HttpRobot.ResBodyRawContent, "{" + Environment.NewLine + "  \"myValue\": \"Tailândia\"" + Environment.NewLine + "}");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }
}