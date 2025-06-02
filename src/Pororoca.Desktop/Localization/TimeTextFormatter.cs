namespace Pororoca.Desktop.Localization;

internal static class TimeTextFormatter
{
    private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan oneMinute = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan oneHour = TimeSpan.FromHours(1);

    internal static string FormatTimeText(TimeSpan time) =>
        time < oneSecond ?
        string.Format("{0}{1}", (int)time.TotalMilliseconds, Localizer.Instance.TimeText.Milliseconds) :
        time < oneMinute ? // More or equal than one second, but less than one minute
        string.Format("{0:0.00}{1}", time.TotalSeconds, Localizer.Instance.TimeText.Seconds) : // TODO: Format digit separator according to language
        time < oneHour ? // More or equal than one minute, but less than one hour
        string.Format("{0}{1} {2}{3}", time.Minutes, Localizer.Instance.TimeText.Minutes, time.Seconds, Localizer.Instance.TimeText.Seconds) :
        // more than one hour
        string.Format("{0}{1} {2}{3}", time.Hours, Localizer.Instance.TimeText.Hours, time.Minutes, Localizer.Instance.TimeText.Minutes);

    internal static string FormatRemainingTimeText(TimeSpan time) =>
        time < oneMinute ?
        string.Format("< 1{0}", Localizer.Instance.TimeText.Minutes) :
        time < oneHour ?
        string.Format("{0}{1}", time.Minutes, Localizer.Instance.TimeText.Minutes) :
        string.Format("{0}{1} {2}{3}", time.Hours, Localizer.Instance.TimeText.Hours, time.Minutes, Localizer.Instance.TimeText.Minutes);
}