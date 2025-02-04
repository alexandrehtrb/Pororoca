using Pororoca.Desktop.UITesting.Robots;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private async Task TestSelfSigned()
    {
        await TopMenuRobot.SwitchTlsVerification(true);
        await AssertTopMenuTlsVerification(true);

        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BadSslSelfSignedTestsUrl}}");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.ClickOnSendAndWaitForResponse();
        await Wait(5);

        HttpRobot.ResDisableTlsVerification.AssertIsVisible();
        HttpRobot.ResBodyRawContent.AssertContainsText("The remote certificate is invalid");
        HttpRobot.ResTitle.AssertContainsText("Response: Failed");
        HttpRobot.ResBodySaveToFile.AssertIsHidden();

        await HttpRobot.ResDisableTlsVerification.ClickOn();
        HttpRobot.ResDisableTlsVerification.AssertIsHidden();
        await AssertTopMenuTlsVerification(false);

        await HttpRobot.ClickOnSendAndWaitForResponse();
        await Wait(5);
        HttpRobot.ResDisableTlsVerification.AssertIsHidden();
        HttpRobot.ResBodyRawContent.AssertContainsText("<html>");
        HttpRobot.ResTitle.AssertContainsText("Response: 200 OK");
        HttpRobot.ResBodySaveToFile.AssertIsVisible();

        await TopMenuRobot.SwitchTlsVerification(true);
    }

    private async Task AssertTopMenuTlsVerification(bool shouldBeEnabled)
    {
        TopMenuRobot.Options.Open();
        await UITestActions.WaitAfterActionAsync();
        if (shouldBeEnabled)
        {
            TopMenuRobot.Options_EnableTlsVerification.AssertHasIconVisible();
        }
        else
        {
            TopMenuRobot.Options_EnableTlsVerification.AssertHasIconHidden();
        }
        TopMenuRobot.Options.Close();
        await UITestActions.WaitAfterActionAsync();
    }
}