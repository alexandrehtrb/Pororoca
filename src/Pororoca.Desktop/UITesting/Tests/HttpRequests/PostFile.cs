namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private async Task TestPostFileBody()
    {
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/file");
        await HttpRobot.SetFileBody("image/jpeg", "{{TestFilesDir}}/homem_aranha.jpg");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        HttpRobot.ResTitle.AssertContainsText("Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "text/plain; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        HttpRobot.ResBodyRawContent.AssertContainsText("--- Received file ---");
        HttpRobot.ResBodyRawContent.AssertContainsText("Content-Type: image/jpeg");
        HttpRobot.ResBodyRawContent.AssertContainsText("Body length: 9784 bytes");
        HttpRobot.ResBodySaveToFile.AssertIsVisible();
    }
}