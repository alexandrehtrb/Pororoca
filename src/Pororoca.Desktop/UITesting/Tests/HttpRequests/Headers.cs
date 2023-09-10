using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestHeaders()
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/headers");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.SetRequestHeaders(new PororocaKeyValueParam[]
        {
            new(false, "Header1", "ValueHeader1"),
            new(true, "Header1", "Header1Value"),
            new(true, "oi_{{SpecialHeaderKey}}", "oi-{{SpecialHeaderValue}}"),
        });
        await HttpRobot.ClickOnSendAndWaitForResponse();

        AssertContainsText(HttpRobot.ResTitle, "Response: 204 NoContent");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("MIRRORED-Host", "localhost:5001");
        AssertContainsResponseHeader("MIRRORED-Accept-Encoding", "gzip, deflate, br");
        AssertContainsResponseHeader("MIRRORED-Header1", "Header1Value");
        AssertContainsResponseHeader("MIRRORED-oi_Header2", "oi-ciao");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertHasText(HttpRobot.ResBodyRawContent, string.Empty);
        AssertIsHidden(HttpRobot.ResBodySaveToFile);
    }    
}