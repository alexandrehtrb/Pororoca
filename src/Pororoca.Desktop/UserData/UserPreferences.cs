using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pororoca.Desktop.Localization;

namespace Pororoca.Desktop.UserData;

public sealed class UserPreferences
{
    private static readonly TimeSpan checkForUpdatesPeriod = TimeSpan.FromDays(8);
    private static readonly TimeSpan askForDonationsPeriod = TimeSpan.FromDays(150); // 5 months

#nullable disable warnings
    public string Lang { get; set; }
    public PororocaTheme? Theme { get; set; }
    public bool? AutoCheckForUpdates { get; set; }
    public string? LastCheckedForUpdatesAt { get; set; }
    private DateTime? LastCheckedForUpdatesDate => ParseDateTimeFromString(LastCheckedForUpdatesAt);
    public string? LastAskedForDonationsAt { get; set; }
    private DateTime? LastAskedForDonationsDate => ParseDateTimeFromString(LastAskedForDonationsAt);

    public UserPreferences()
    {
        // default constructor for JSON deserialization
    }
#nullable enable warnings

    public UserPreferences(Language lang, PororocaTheme theme, bool autoCheckForUpdates, DateTime lastCheckedForUpdatesDate)
    {
        Lang = lang.ToLCID();
        Theme = theme;
        AutoCheckForUpdates = autoCheckForUpdates;
        LastCheckedForUpdatesAt = lastCheckedForUpdatesDate.ToString("yyyy-MM-dd");
    }

    public Language GetLanguage() =>
        LanguageExtensions.GetLanguageByLCID(Lang);

    public void SetLanguage(Language lang) =>
        Lang = lang.ToLCID();

    #region UPDATES

    public bool NeedsToCheckForUpdates() =>
        AutoCheckForUpdates == true &&
        LastCheckedForUpdatesDate is not null &&
        (DateTime.Now - LastCheckedForUpdatesDate) > checkForUpdatesPeriod;

    public bool HasLastUpdateCheckDate() =>
        LastCheckedForUpdatesDate is not null;

    public void SetLastUpdateCheckDateAsToday() =>
        LastCheckedForUpdatesAt = DateTime.Now.ToString("yyyy-MM-dd");

    #endregion

    #region DONATIONS

    public bool NeedsToAskForDonations() =>
        LastAskedForDonationsDate is not null &&
        (DateTime.Now - LastAskedForDonationsDate) > askForDonationsPeriod;

    public bool HasLastAskedForDonationsDate() =>
        LastAskedForDonationsDate is not null;

    public void SetLastAskedForDonationsDateAsToday() =>
        LastAskedForDonationsAt = DateTime.Now.ToString("yyyy-MM-dd");

    #endregion

    private static DateTime? ParseDateTimeFromString(string? field)
    {
        if (field is null)
        {
            return null;
        }
        else if (DateTime.TryParseExact(field, "yyyy-MM-dd", null, DateTimeStyles.AssumeLocal, out var parsedDate))
        {
            return parsedDate;
        }
        else
        {
            return null;
        }
    }
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