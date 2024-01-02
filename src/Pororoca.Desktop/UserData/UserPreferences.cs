using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        Lang = lang.ToLCID();
        UpdateReminderLastShownAt = updateReminderLastShownDate.ToString("yyyy-MM-dd");
        Theme = theme;
    }

    public Language GetLanguage() =>
        LanguageExtensions.GetLanguageByLCID(Lang);

    public void SetLanguage(Language lang) =>
        Lang = lang.ToLCID();

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

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(UserPreferences))]
internal partial class UserPreferencesJsonSrcGenContext : JsonSerializerContext
{
}