namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private async Task TestPostRawTextBody()
    {
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/txt");
        await HttpRobot.SetRawBody("text/xml", "<XML><MyValue>{{SpecialValue1}}</MyValue></XML>");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        HttpRobot.ResTitle.AssertContainsText("Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "text/xml; charset=utf-8");
        AssertContainsResponseHeader("Content-Length", "40");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        HttpRobot.ResBodyRawContent.AssertHasText("<XML>" + Environment.NewLine + "  <MyValue>Tailândia</MyValue>" + Environment.NewLine + "</XML>");
        HttpRobot.ResBodySaveToFile.AssertIsVisible();
    }
}