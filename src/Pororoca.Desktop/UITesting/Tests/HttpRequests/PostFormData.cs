using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestPostFormDataBody()
    {
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/multipartformdata");
        await HttpRobot.SetFormDataBody(
        [
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "a", "xyz{{SpecialValue1}}", "text/plain"),
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "b", "[]", "application/json"),
            PororocaHttpRequestFormDataParam.MakeFileParam(true, "arq", "{{TestFilesDir}}/arq.txt", "text/plain")
        ]);
        await HttpRobot.ClickOnSendAndWaitForResponse();

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "text/plain; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertContainsText(HttpRobot.ResBodyRawContent, "### Received multipart request ###");
        //
        AssertContainsText(HttpRobot.ResBodyRawContent, "Content-Type: text/plain; charset=utf-8");
        AssertContainsText(HttpRobot.ResBodyRawContent, "Content-Disposition: form-data; name=a");
        AssertContainsText(HttpRobot.ResBodyRawContent, "Body: xyzTailândia");
        //
        AssertContainsText(HttpRobot.ResBodyRawContent, "Content-Type: application/json; charset=utf-8");
        AssertContainsText(HttpRobot.ResBodyRawContent, "Content-Disposition: form-data; name=b");
        AssertContainsText(HttpRobot.ResBodyRawContent, "Body: []");
        //
        AssertContainsText(HttpRobot.ResBodyRawContent, "Content-Type: text/plain");
        AssertContainsText(HttpRobot.ResBodyRawContent, "Content-Disposition: form-data; name=arq; filename=arq.txt; filename*=utf-8''arq.txt");
        AssertContainsText(HttpRobot.ResBodyRawContent, "Body: oi");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }
}