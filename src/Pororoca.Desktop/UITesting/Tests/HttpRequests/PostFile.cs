namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestPostFileBody()
    {
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/file");
        await HttpRobot.SetFileBody("image/jpeg", "{{TestFilesDir}}/homem_aranha.jpg");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "text/plain; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertContainsText(HttpRobot.ResBodyRawContent, "--- Received file ---");
        AssertContainsText(HttpRobot.ResBodyRawContent, "Content-Type: image/jpeg");
        AssertContainsText(HttpRobot.ResBodyRawContent, "Body length: 9784 bytes");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }
}