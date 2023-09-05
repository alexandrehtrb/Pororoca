using System.Text;
using Pororoca.Desktop.UITesting.Tests;

namespace Pororoca.Desktop.UITesting;

public static class UITestsRunner
{
    public static Task<string> RunAllTestsAsync() => RunTestsAsync(new UITest[]
    {
        new TopMenuUITest(),
        new SwitchLanguagesUITest(),
        new SwitchThemesUITest(),
        new EditableTextBlockUITest(),
        new CollectionAndCollectionFolderUITest(),
        new TreeCopyAndPasteItemsUITest(),
        new TreeCutAndPasteItemsUITest(),
        new HttpRequestValidationsUITest(),
        // TODO: UI tests to be made:
        // new CollectionAndEnvironmentVariablesUITest() // add collection and env variables, check if they are being applied on URL
        // // for the test below:
        // // - if Windows 7, check if HTTP/2 is blocked
        // // - if Windows < 11 or Mac OSX, check if HTTP/3 is blocked
        // new HttpRequestFieldsValidationsUITest() // using collection and env vars in auth
        // new WebSocketConnectionFieldsValidationsUITest() // using collection and env vars in auth
        // new HttpRequestBasicAuthUITest() // using collection and env vars in auth
        // new HttpRequestBearerAuthUITest() // using collection and env vars in auth
        // new HttpRequestClientCertificatePkcs12AuthUITest() // using collection and env vars in auth
        // new HttpRequestClientCertificatePemUnencryptedConjoinedAuthUITest() // using collection and env vars in auth
        // new HttpRequestClientCertificatePemEncryptedConjoinedAuthUITest() // using collection and env vars in auth
        // new HttpRequestClientCertificatePemUnencryptedSeparateAuthUITest() // using collection and env vars in auth
        // new HttpRequestClientCertificatePemEncryptedSeparateAuthUITest() // using collection and env vars in auth
        // foreach (var method in new[] { HttpMethod.Get, HttpMethod.Post })
        // // for the loop below, include:
        // // - HTTP/2 only if Mac OSX, Linux or Windows >= 10
        // // - HTTP/3 only if Linux or Windows >= 11
        // foreach (var version in new[] { 1.0m, 2.0m, 3.0m })
        // // usar Pororoca.TestServer
        // new HttpEmptyRequestUITest(method, version) // using collection variable on URL
        // new HttpRawJsonRequestUITest(method, version) // using collection and env variable on body
        // new HttpRawXmlRequestUITest(method, version) // using collection and env variable on body
        // new HttpFileRequestUITest(method, version) // using collection and env variable on file src path
        // new HttpURLEncodedRequestUITest(method, version) // using collection and env variables on params
        // new HttpFormDataRequestUITest(method, version) // using collection and env variables on params
        // foreach (var version in new[] { 1.0m, 2.0m })
        // new WebSocketConnectionAndClientMessagesUITest(version)
        // new WebSocketConversationUITest(version)
        // Out of scope of automated UI tests:
        // some keybindings
        // import and export collections and environments (involve dialogs)
        // help dialogs
        // context menu actions
        // cut collection, cut and paste to itself
        // delete items actions
        // save responses to file
    });

    private static async Task<string> RunTestsAsync(params UITest[] tests)
    {
        static TimeSpan SumTotalTime(IEnumerable<UITest> ts)
        {
            var totalTime = TimeSpan.Zero;
            foreach (var t in ts)
            {
                totalTime += t.ElapsedTime;
            }
            return totalTime;
        }

        StringBuilder allTestsLogsAppender = new();
        foreach (var test in tests)
        {
            await RunTestAsync(allTestsLogsAppender, test);
        }
        var totalTime = SumTotalTime(tests);
        string fmtTime = @"hh'h'mm'm'ss's'";
        allTestsLogsAppender.AppendLine("TOTAL TIME: " + totalTime.ToString(fmtTime));
        return allTestsLogsAppender.ToString();
    }

    private static async Task RunTestAsync(StringBuilder allTestsLogsAppender, UITest test)
    {
        try
        {
            test.Start();
            await test.RunAsync();
        }
        catch (Exception ex)
        {
            allTestsLogsAppender.AppendLine(ex.ToString());
        }
        finally
        {
            test.Finish();
            allTestsLogsAppender.AppendLine($"{test.TestName}: {(test.Successful == true ? "SUCCESS" : "FAILED")} {test.TotalElapsedSeconds}s");
            if (!string.IsNullOrWhiteSpace(test.Log))
            {
                allTestsLogsAppender.AppendLine(test.Log);
            }
        }
    }
}