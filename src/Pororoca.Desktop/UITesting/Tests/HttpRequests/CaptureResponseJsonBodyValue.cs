namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class ResponseCapturesUITest : PororocaUITest
{
    private async Task TestCaptureResponseJsonBody(bool saveCapturesInCurrentEnvironment, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        await SetupReqAndSendAsync(discard: false);

        if (saveCapturesInCurrentEnvironment)
        {
            await TreeRobot.Select("COL1/ENVS/ENV1");
            var targetVar = EnvRobot.VariablesVm.Items.First(x => x.Key == "MyCapturedJSONValue");
            AssertCondition(targetVar.Value == "1");
            AssertCondition(targetVar.IsSecret == true);
            targetVar.RemoveVariable();
        }
        else
        {
            await TreeRobot.Select("COL1/VARS");
            var targetVar = ColVarsRobot.VariablesVm.Items.First(x => x.Key == "MyCapturedJSONValue");
            AssertCondition(targetVar.Value == "1");
            AssertCondition(targetVar.IsSecret == true);
            targetVar.RemoveVariable();
        }

        await TeardownReq();
    }

    private async Task TestCaptureAndDiscardResponseJsonBody(bool saveCapturesInCurrentEnvironment, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        await SetupReqAndSendAsync(discard: true);

        await TreeRobot.Select("COL1/VARS");
        AssertCondition(ColVarsRobot.VariablesVm.Items.Any(x => x.Key == "MyCapturedJSONValue") == false,
                        "Should not save MyCapturedJSONValue as collection variable.");
        AssertCondition(ColVarsRobot.VariablesVm.Items.Any(x => x.Key == "_MyCapturedJSONValue") == false,
                        "Should not save _MyCapturedJSONValue as collection variable.");

        if (saveCapturesInCurrentEnvironment)
        {
            await TreeRobot.Select("COL1/ENVS/ENV1");
            AssertCondition(EnvRobot.VariablesVm.Items.Any(x => x.Key == "MyCapturedJSONValue") == false,
                            "Should not save MyCapturedJSONValue as environment variable.");
            AssertCondition(EnvRobot.VariablesVm.Items.Any(x => x.Key == "_MyCapturedJSONValue") == false,
                            "Should not save _MyCapturedJSONValue as environment variable.");
        }

        await TeardownReq();
    }

    private async Task SetupReqAndSendAsync(bool discard)
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/json");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResCapture);
        await HttpRobot.ResAddCaptureBody.ClickOn();
        await HttpRobot.EditResponseCaptureAt(0, discard ? "_MyCapturedJSONValue" : "MyCapturedJSONValue", "$.id");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        var capture = HttpRobot.ResCapturesVm.Items[0];
        AssertCondition(capture.CapturedValue == "1");
    }

    private async Task TeardownReq()
    {
        await TreeRobot.Select("COL1/HTTPREQ");
        HttpRobot.ResCapturesVm.Items.Clear();
    }
}