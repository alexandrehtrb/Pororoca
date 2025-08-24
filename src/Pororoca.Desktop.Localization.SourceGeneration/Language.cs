namespace Pororoca.Desktop.Localization.SourceGeneration;

public enum Language
{
    Portuguese,
    English,
    Russian,
    Italian,
    SimplifiedChinese,
    German,
    Spanish,
    Polish
}

public static class LanguageExtensions
{
    public static string ToLCID(this Language lang) => lang switch
    {
        Language.Portuguese => "pt-br",
        Language.English => "en-gb",
        Language.Russian => "ru-ru",
        Language.Italian => "it-it",
        Language.German => "de-de",
        Language.SimplifiedChinese => "zh-cn",
        Language.Spanish => "es-mx",
        Language.Polish => "pl-pl",
        _ => "en-gb",
    };

    public static Language GetLanguageByLCID(string lcid) => lcid switch
    {
        "pt-br" => Language.Portuguese,
        "en-gb" => Language.English,
        "de-de" => Language.German,
        "ru-ru" => Language.Russian,
        "it-it" => Language.Italian,
        "zh-cn" => Language.SimplifiedChinese,
        "es-mx" => Language.Spanish,
        "pl-pl" => Language.Polish,
        _ => throw new KeyNotFoundException($"No language found for LCID '{lcid}'.")
    };
}