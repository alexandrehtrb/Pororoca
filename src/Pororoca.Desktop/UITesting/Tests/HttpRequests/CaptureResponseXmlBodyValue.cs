namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestCaptureResponseXmlBody()
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/xml");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResCapture);
        await HttpRobot.ResAddCaptureBody.ClickOn();
        await HttpRobot.EditResponseCaptureAt(0, "MyCapturedXMLValue", "/env:Envelope/env:Body/xsi:response/xsi:Value/wsa:MyVal2");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        var capture = HttpRobot.ResCapturesVm.Items[0];
        Assert(capture.CapturedValue == "123987456");

        await TreeRobot.Select("COL1/ENVS/ENV1");
        var envVar = EnvRobot.VariablesVm.Items.First(x => x.Key == "MyCapturedXMLValue");
        Assert(envVar.Value == "123987456");
        Assert(envVar.IsSecret == true);
        envVar.RemoveVariable();

        await TreeRobot.Select("COL1/HTTPREQ");
        HttpRobot.ResCapturesVm.Items.Clear();
    }
}