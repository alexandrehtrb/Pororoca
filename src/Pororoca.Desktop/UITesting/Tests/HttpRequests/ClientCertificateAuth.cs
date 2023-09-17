namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : UITest
{
    private async Task TestClientCertificatePkcs12Auth()
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BadSslClientCertTestsUrl}}");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.SetPkcs12CertificateAuth("{{ClientCertificatesDir}}/badssl.com-client.p12", "{{BadSslClientCertFilePassword}}");
        await HttpRobot.ClickOnSendAndWaitForResponse();
        await Wait(3);

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        //AssertContainsResponseHeader("Content-Type", "text/plain; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertContainsText(HttpRobot.ResBodyRawContent, "<html>");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }

    private Task TestClientCertificatePemConjoinedUnencryptedAuth() =>
        TestClientCertificatePem("{{ClientCertificatesDir}}/badssl.com-client-certificate-with-unencrypted-private-key.pem",
                                 string.Empty,
                                 string.Empty);

    private Task TestClientCertificatePemConjoinedEncryptedAuth() =>
        TestClientCertificatePem("{{ClientCertificatesDir}}/badssl.com-client-certificate-with-encrypted-private-key.pem",
                                 string.Empty,
                                 "{{BadSslClientCertFilePassword}}");

    private Task TestClientCertificatePemSeparateUnencryptedAuth() =>
        TestClientCertificatePem("{{ClientCertificatesDir}}/badssl.com-client-certificate-without-private-key.pem",
                                 "{{ClientCertificatesDir}}/badssl.com-client-unencrypted-private-key.key",
                                 string.Empty);

    private Task TestClientCertificatePemSeparateEncryptedAuth() =>
        TestClientCertificatePem("{{ClientCertificatesDir}}/badssl.com-client-certificate-without-private-key.pem",
                                 "{{ClientCertificatesDir}}/badssl.com-client-encrypted-private-key.key",
                                 "{{BadSslClientCertFilePassword}}");

    private async Task TestClientCertificatePem(string certFilePath, string privateKeyFilePath, string privateKeyPassword)
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BadSslClientCertTestsUrl}}");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.SetPemCertificateAuth(certFilePath, privateKeyFilePath, privateKeyPassword);
        await HttpRobot.ClickOnSendAndWaitForResponse();
        await Wait(2);

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        //AssertContainsResponseHeader("Content-Type", "text/plain; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertContainsText(HttpRobot.ResBodyRawContent, "<html>");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }
}