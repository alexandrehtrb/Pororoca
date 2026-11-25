namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private async Task TestQueryByHeaders(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        await HttpRobot.HttpMethod.Select("QUERY");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/query/fruits");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.SetRequestHeaders([
            new(true, "Nome", "cup")
        ]);
        await HttpRobot.ClickOnSendAndWaitForResponse();

        HttpRobot.ResTitle.AssertContainsText("Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "application/json; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        string nl = Environment.NewLine;
        HttpRobot.ResBodyRawContent.AssertHasText(
            "[" + nl
          + "  {" + nl
          + "    \"id\": 10," + nl
          + "    \"nome\": \"Cupuaçu\"," + nl
          + "    \"familia\": \"Malvaceae\"," + nl
          + "    \"calorias\": 53" + nl
          + "  }" + nl
          + "]");
        HttpRobot.ResBodySaveToFile.AssertIsVisible();
    }
}