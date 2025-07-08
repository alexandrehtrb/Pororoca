using Pororoca.Desktop.Localization;
using static Pororoca.Desktop.Localization.TimeTextFormatter;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class TimeTextFormattingUITest : PororocaUITest
{
    public override async Task RunAsync()
    {
        TimeSpan ts;

        ts = TimeSpan.Parse("00:00:00.1234000");
        AssertEqual("123ms", FormatTimeText(ts));
        AssertEqual("< 1min", FormatRemainingTimeText(ts));

        MainWindowVm.SelectLanguage(Language.Portuguese);
        await Wait(0.5);

        ts = TimeSpan.Parse("00:00:01.0000000");
        AssertEqual("1,00s", FormatTimeText(ts));
        AssertEqual("< 1min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("00:00:01.2345000");
        AssertEqual("1,23s", FormatTimeText(ts));
        AssertEqual("< 1min", FormatRemainingTimeText(ts));

        MainWindowVm.SelectLanguage(Language.English);
        await Wait(0.5);

        ts = TimeSpan.Parse("00:00:01.0000000");
        AssertEqual("1.00s", FormatTimeText(ts));
        AssertEqual("< 1min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("00:00:01.2345000");
        AssertEqual("1.23s", FormatTimeText(ts));
        AssertEqual("< 1min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("00:01:00.0000000");
        AssertEqual("1min 0s", FormatTimeText(ts));
        AssertEqual("1min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("00:01:00.2345000");
        AssertEqual("1min 0s", FormatTimeText(ts));
        AssertEqual("1min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("00:01:02.0000000");
        AssertEqual("1min 2s", FormatTimeText(ts));
        AssertEqual("1min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("00:01:02.3456000");
        AssertEqual("1min 2s", FormatTimeText(ts));
        AssertEqual("1min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("01:00:00.0000000");
        AssertEqual("1h 0min", FormatTimeText(ts));
        AssertEqual("1h 0min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("01:00:00.2345000");
        AssertEqual("1h 0min", FormatTimeText(ts));
        AssertEqual("1h 0min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("01:00:02.0000000");
        AssertEqual("1h 0min", FormatTimeText(ts));
        AssertEqual("1h 0min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("01:00:02.3456000");
        AssertEqual("1h 0min", FormatTimeText(ts));
        AssertEqual("1h 0min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("01:02:00.0000000");
        AssertEqual("1h 2min", FormatTimeText(ts));
        AssertEqual("1h 2min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("01:02:00.3456000");
        AssertEqual("1h 2min", FormatTimeText(ts));
        AssertEqual("1h 2min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("01:02:03.0000000");
        AssertEqual("1h 2min", FormatTimeText(ts));
        AssertEqual("1h 2min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("01:02:03.4567000");
        AssertEqual("1h 2min", FormatTimeText(ts));
        AssertEqual("1h 2min", FormatRemainingTimeText(ts));

        ts = TimeSpan.Parse("02:54:04.5678000");
        AssertEqual("2h 54min", FormatTimeText(ts));
        AssertEqual("2h 54min", FormatRemainingTimeText(ts));
    }

    private static void AssertEqual(string expected, string actual) =>
        AssertCondition(expected == actual, $"Expected text: '{expected}'; actual text: '{actual}'.");
}