namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class ResponseCapturesUITest : PororocaUITest
{
    private async Task TestCaptureResponseHeader(bool saveCapturesInCurrentEnvironment)
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/headers");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqHeaders);
        await HttpRobot.SetRequestHeaders(
        [
            new(true, "Header1", "oi"),
        ]);
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResCapture);
        await HttpRobot.ResAddCaptureHeader.ClickOn();
        await HttpRobot.EditResponseCaptureAt(0, "MyCapturedHeaderValue", "MIRRORED-Header1");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        var capture = HttpRobot.ResCapturesVm.Items[0];
        AssertCondition(capture.CapturedValue == "oi");

        if (saveCapturesInCurrentEnvironment)
        {
            await TreeRobot.Select("COL1/ENVS/ENV1");
            var targetVar = EnvRobot.VariablesVm.Items.First(x => x.Key == "MyCapturedHeaderValue");
            AssertCondition(targetVar.Value == "oi");
            AssertCondition(targetVar.IsSecret == true);
            targetVar.RemoveVariable();
        }
        else
        {
            await TreeRobot.Select("COL1/VARS");
            var targetVar = ColVarsRobot.VariablesVm.Items.First(x => x.Key == "MyCapturedHeaderValue");
            AssertCondition(targetVar.Value == "oi");
            AssertCondition(targetVar.IsSecret == true);
            targetVar.RemoveVariable();
        }

        await TreeRobot.Select("COL1/HTTPREQ");
        HttpRobot.ResCapturesVm.Items.Clear();
    }
}