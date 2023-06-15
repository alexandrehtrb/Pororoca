using System.Globalization;
using Pororoca.Desktop.Localization;

namespace Pororoca.Desktop.UserData;

public sealed class UserPreferences
{
    private static readonly TimeSpan updateReminderPeriod = TimeSpan.FromDays(60);

#nullable disable warnings
    public string Lang { get; set; }
    public string? UpdateReminderLastShownAt { get; set; }
    public PororocaTheme? Theme { get; set; }

    public UserPreferences()
    {
        // default constructor for JSON deserialization
    }
#nullable enable warnings

    public UserPreferences(Language lang, DateTime updateReminderLastShownDate, PororocaTheme theme)
    {
        Lang = lang.GetLanguageLCID();
        UpdateReminderLastShownAt = updateReminderLastShownDate.ToString("yyyy-MM-dd");
        Theme = theme;
    }

    public Language GetLanguage() =>
        LanguageExtensions.GetLanguageFromLCID(Lang);

    public void SetLanguage(Language lang) =>
        Lang = lang.GetLanguageLCID();

    private DateTime? UpdateReminderLastShownDate
    {
        get
        {
            if (UpdateReminderLastShownAt is null)
            {
                return null;
            }
            else if (DateTime.TryParseExact(UpdateReminderLastShownAt, "yyyy-MM-dd", null, DateTimeStyles.AssumeLocal, out var parsedDate))
            {
                return parsedDate;
            }
            else
            {
                return null;
            }
        }
    }

    public bool NeedsToShowUpdateReminder() =>
        UpdateReminderLastShownDate is not null &&
        (DateTime.Now - UpdateReminderLastShownDate) > updateReminderPeriod;

    public bool HasUpdateReminderLastShownDate() =>
        UpdateReminderLastShownDate is not null;

    public void SetUpdateReminderLastShownDateAsToday() =>
        UpdateReminderLastShownAt = DateTime.Now.ToString("yyyy-MM-dd");
}