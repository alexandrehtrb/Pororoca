namespace Pororoca.Desktop.Localization;

public enum Language
{
    PtBr = 1,
    EnGb = 2
}

public static class LanguageExtensions
{
    internal static string GetLanguageLCID(this Language lang) =>
        lang switch
        {
            Language.PtBr => "pt-BR",
            Language.EnGb => "en-GB",
            _ => "en-GB"
        };

    internal static Language GetLanguageFromLCID(string? lcid) =>
        lcid switch
        {
            "pt-BR" => Language.PtBr,
            "en-GB" => Language.EnGb,
            _ => Language.EnGb
        };
}