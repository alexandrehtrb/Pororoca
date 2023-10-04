using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestCaptureResponseJsonBody()
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/json");        
        await HttpRobot.SetEmptyBody();
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResCapture);
        await HttpRobot.ResAddCaptureBody.ClickOn();
        await HttpRobot.EditResponseCaptureAt(0, "MyCapturedJSONValue", "$.id");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        var capture = HttpRobot.ResCapturesVm.Items[0];
        Assert(capture.CapturedValue == "1");

        await TreeRobot.Select("COL1/ENVS/ENV1");
        var envVar = EnvRobot.VariablesVm.Items.First(x => x.Key == "MyCapturedJSONValue");
        Assert(envVar.Value == "1");
        Assert(envVar.IsSecret == true);
        envVar.RemoveVariable();
        
        await TreeRobot.Select("COL1/HTTPREQ");
        HttpRobot.ResCapturesVm.Items.Clear();
    }
}