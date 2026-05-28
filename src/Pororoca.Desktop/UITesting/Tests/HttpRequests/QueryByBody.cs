namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private async Task TestQueryByBody(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        await HttpRobot.HttpMethod.Select("QUERY");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/query/fruits");
        await HttpRobot.SetRawBody("application/json", "{\"nome\":\"tâm\"}");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.SetRequestHeaders([]);
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
          + "    \"id\": 2," + nl
          + "    \"nome\": \"Tâmara\"," + nl
          + "    \"familia\": \"Arecaceae\"," + nl
          + "    \"calorias\": 282" + nl
          + "  }" + nl
          + "]");
        HttpRobot.ResBodySaveToFile.AssertIsVisible();
    }
}