using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
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

        HttpRobot.ResTitle.AssertContainsText("Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "text/plain; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        HttpRobot.ResBodyRawContent.AssertContainsText("### Received multipart request ###");
        //
        HttpRobot.ResBodyRawContent.AssertContainsText("Content-Type: text/plain; charset=utf-8");
        HttpRobot.ResBodyRawContent.AssertContainsText("Content-Disposition: form-data; name=a");
        HttpRobot.ResBodyRawContent.AssertContainsText("Body: xyzTailândia");
        //
        HttpRobot.ResBodyRawContent.AssertContainsText("Content-Type: application/json; charset=utf-8");
        HttpRobot.ResBodyRawContent.AssertContainsText("Content-Disposition: form-data; name=b");
        HttpRobot.ResBodyRawContent.AssertContainsText("Body: []");
        //
        HttpRobot.ResBodyRawContent.AssertContainsText("Content-Type: text/plain");
        HttpRobot.ResBodyRawContent.AssertContainsText("Content-Disposition: form-data; name=arq; filename=arq.txt; filename*=utf-8''arq.txt");
        HttpRobot.ResBodyRawContent.AssertContainsText("Body: oi");
        HttpRobot.ResBodySaveToFile.AssertIsVisible();
    }
}