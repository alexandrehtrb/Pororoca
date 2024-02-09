namespace Pororoca.Desktop.Localization;

internal static class TimeTextFormatter
{
    private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan oneMinute = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan oneHour = TimeSpan.FromHours(1);

    internal static string FormatTimeText(TimeSpan time) =>
        time < oneSecond ?
        string.Format(Localizer.Instance.TimeText.MillisecondsFormat, (int)time.TotalMilliseconds) :
        time < oneMinute ? // More or equal than one second, but less than one minute
        string.Format(Localizer.Instance.TimeText.SecondsFormat, time.TotalSeconds) : // TODO: Format digit separator according to language
        time < oneHour ? // More or equal than one minute, but less than one hour
        string.Format(Localizer.Instance.TimeText.MinutesAndSecondsFormat, time.Minutes, time.Seconds) :
        // more than one hour
        string.Format(Localizer.Instance.TimeText.HoursAndMinutesFormat, time.Hours, time.Minutes);

    internal static string FormatRemainingTimeText(TimeSpan time) =>
        time < oneMinute ?
        "< " + string.Format(Localizer.Instance.TimeText.MinutesFormat, 1) :
        time < oneHour ?
        string.Format(Localizer.Instance.TimeText.MinutesFormat, time.Minutes) :
        string.Format(Localizer.Instance.TimeText.HoursAndMinutesFormat, time.Hours, time.Minutes);


}