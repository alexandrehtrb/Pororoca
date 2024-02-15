using Pororoca.Desktop.UITesting.Robots;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class ResponseCapturesUITest : UITest
{
    private async Task TestCaptureResponseXmlBody(bool saveCapturesInCurrentEnvironment)
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

        if (saveCapturesInCurrentEnvironment)
        {
            await TreeRobot.Select("COL1/ENVS/ENV1");
            var targetVar = EnvRobot.VariablesVm.Items.First(x => x.Key == "MyCapturedXMLValue");
            Assert(targetVar.Value == "123987456");
            Assert(targetVar.IsSecret == true);
            targetVar.RemoveVariable();
        }
        else
        {
            await TreeRobot.Select("COL1/VARS");
            var targetVar = ColVarsRobot.VariablesVm.Items.First(x => x.Key == "MyCapturedXMLValue");
            Assert(targetVar.Value == "123987456");
            Assert(targetVar.IsSecret == true);
            targetVar.RemoveVariable();
        }

        await TreeRobot.Select("COL1/HTTPREQ");
        HttpRobot.ResCapturesVm.Items.Clear();
    }
}