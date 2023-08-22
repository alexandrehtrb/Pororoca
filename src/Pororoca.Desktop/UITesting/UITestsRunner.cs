using System.Diagnostics;
using System.Text;
using Pororoca.Desktop.UITesting.Tests;

namespace Pororoca.Desktop.UITesting;

public static class UITestsRunner
{
    public static Task<string> RunAllTestsAsync() => RunTestsAsync(new UITest[]
    {
        new TopMenuUITest(),
        new SwitchLanguagesUITest(),
        new SwitchThemesUITest()
    });

    private static async Task<string> RunTestsAsync(params UITest[] tests)
    {
        StringBuilder allTestsLogsAppender = new();
        foreach (var test in tests)
        {
            await RunTestAsync(allTestsLogsAppender, test);
        }
        return allTestsLogsAppender.ToString();
    }

    private static async Task RunTestAsync(StringBuilder allTestsLogsAppender, UITest test)
    {
        allTestsLogsAppender.AppendLine($"Running test {test.TestName}...");
        Stopwatch sw = new();
        try
        {
            sw.Start();
            await test.RunAsync();
        }
        catch (Exception ex)
        {
            allTestsLogsAppender.AppendLine(ex.ToString());
        }
        finally
        {
            sw.Stop();
            if (!string.IsNullOrWhiteSpace(test.Log))
            {
                allTestsLogsAppender.AppendLine(test.Log);
            }
            test.Finish();
            allTestsLogsAppender.AppendLine($"Test {test.TestName} finished: {(test.Successful == true ? "SUCCESS" : "FAILED")} {sw.Elapsed.ToString()}s");
        }
    }
}