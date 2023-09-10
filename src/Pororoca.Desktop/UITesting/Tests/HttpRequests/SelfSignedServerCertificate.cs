using Pororoca.Desktop.UITesting.Robots;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestSelfSigned()
    {
        await TopMenuRobot.SwitchTlsVerification(true);
        await AssertTopMenuTlsVerification(true);

        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BadSslSelfSignedTestsUrl}}");
        await HttpRobot.SelectEmptyBody();
        await HttpRobot.ClickOnSendAndWaitForResponse();
        await Wait(5);

        AssertIsVisible(HttpRobot.ResDisableTlsVerification);
        AssertContainsText(HttpRobot.ResBodyRawContent, "The remote certificate is invalid because of errors in the certificate chain: UntrustedRoot");
        AssertContainsText(HttpRobot.ResTitle, "Response: Failed");
        AssertIsHidden(HttpRobot.ResBodySaveToFile);

        await HttpRobot.ResDisableTlsVerification.ClickOn();
        AssertIsHidden(HttpRobot.ResDisableTlsVerification);
        await AssertTopMenuTlsVerification(false);
        
        await HttpRobot.ClickOnSendAndWaitForResponse();
        AssertIsHidden(HttpRobot.ResDisableTlsVerification);
        AssertContainsText(HttpRobot.ResBodyRawContent, "<html>");
        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
        
        await TopMenuRobot.SwitchTlsVerification(true);
    }

    private async Task AssertTopMenuTlsVerification(bool shouldBeEnabled)
    {
        TopMenuRobot.Options.Open();
        await UITestActions.WaitAfterActionAsync();
        if (shouldBeEnabled)
        {
            AssertHasIconVisible(TopMenuRobot.Options_EnableTlsVerification);
        }
        else
        {
            AssertHasIconHidden(TopMenuRobot.Options_EnableTlsVerification);
        }
        TopMenuRobot.Options.Close();
        await UITestActions.WaitAfterActionAsync();
    }
}