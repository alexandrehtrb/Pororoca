namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class ResponseCapturesUITest : PororocaUITest
{
    private async Task TestCaptureResponseJsonBody(bool hasActiveEnvironment, bool discard, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/json");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResCapture);
        await HttpRobot.ResAddCaptureBody.ClickOn();
        await HttpRobot.EditResponseCaptureAt(0, discard ? "_MyCapturedJSONValue" : "MyCapturedJSONValue", "$.id");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        var capture = HttpRobot.ResCapturesVm.Items[0];
        AssertCondition(capture.CapturedValue == "1");

        if (discard)
        {
            await TreeRobot.Select("COL1/VARS");
            AssertCondition(ColVarsRobot.VariablesVm.Items.Any(x => x.Key == "MyCapturedJSONValue") == false,
                            "Should not save MyCapturedJSONValue as collection variable.");
            AssertCondition(ColVarsRobot.VariablesVm.Items.Any(x => x.Key == "_MyCapturedJSONValue") == false,
                            "Should not save _MyCapturedJSONValue as collection variable.");

            if (hasActiveEnvironment)
            {
                await TreeRobot.Select("COL1/ENVS/ENV1");
                AssertCondition(EnvRobot.VariablesVm.Items.Any(x => x.Key == "MyCapturedJSONValue") == false,
                                "Should not save MyCapturedJSONValue as environment variable.");
                AssertCondition(EnvRobot.VariablesVm.Items.Any(x => x.Key == "_MyCapturedJSONValue") == false,
                                "Should not save _MyCapturedJSONValue as environment variable.");
            }
        }
        else
        {
            if (hasActiveEnvironment)
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
        }

        await TreeRobot.Select("COL1/HTTPREQ");
        HttpRobot.ResCapturesVm.Items.Clear();
    }
}