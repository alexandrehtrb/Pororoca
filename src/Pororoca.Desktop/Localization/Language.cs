namespace Pororoca.Desktop.Localization;

public enum Language
{
    PtBr = 1,
    EnGb = 2,
    RuRu = 3
}

public static class LanguageExtensions
{
    internal static string GetLanguageLCID(this Language lang) =>
        lang switch
        {
            Language.PtBr => "pt-BR",
            Language.EnGb => "en-GB",
            Language.RuRu => "ru-RU",
            _ => "en-GB"
        };

    internal static Language GetLanguageFromLCID(string? lcid) =>
        lcid switch
        {
            "pt-BR" => Language.PtBr,
            "en-GB" => Language.EnGb,
            "ru-RU" => Language.RuRu,
            _ => Language.EnGb
        };


}