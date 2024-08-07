using System.Text;
using Pororoca.Desktop.UITesting.Tests;

namespace Pororoca.Desktop.UITesting;

public static class UITestsRunner
{
    public static Task<string> RunAllTestsAsync() => RunTestsAsync(
    [
        new TopMenuUITest(),
        new SwitchLanguagesUITest(),
        new SwitchThemesUITest(),
        new EditableTextBlockUITest(),
        new CollectionAndCollectionFolderUITest(),
        new TreeCopyAndPasteItemsUITest(),
        new TreeCutAndPasteItemsUITest(),
        new TreeDeleteItemsUITest(),
        new HttpRequestValidationsUITest(),
        new HttpRequestsUITest(),
        new VariablesCutCopyPasteDeleteUITest(),
        new HeadersCutCopyPasteDeleteUITest(),
        new UrlEncodedParamsCutCopyPasteDeleteUITest(),
        new FormDataParamsCutCopyPasteDeleteUITest(),
        new WebSocketsValidationsUITest(),
        new WebSocketsUITest(),
        new SaveAndRestoreCollectionUITest(),
        new ExportCollectionsUITest(),
        new ExportEnvironmentsUITest(),
        new CollectionScopedAuthUITest(),
        new HttpRepeaterUITest(),
        new HttpRepeaterValidationsUITest(),
        new ResponseCapturesUITest(),
        // Out of scope of automated UI tests:
        // some keybindings
        // all dialogs
        // context menu actions
        // form data add text and file params buttons
        // cut collection, cut and paste to itself
        // save responses to file
        // welcome screen
        // http headers names and values autocomplete
    ]);

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