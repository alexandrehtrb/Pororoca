using Avalonia.Platform;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace Pororoca.Desktop.TextEditorConfig;

internal class CustomTextMateRegistryOptions : IRegistryOptions
{
    private const string pororocaJsonGrammarName = "source.json.pororoca";

    private static readonly Lazy<IRawGrammar> cachedPororocaJsonGrammar = new(LoadPororocaJsonGrammar);

    private readonly RegistryOptions defaultOptions;

    private readonly ThemeName defaultTheme;

    public CustomTextMateRegistryOptions(ThemeName defaultTheme) =>
        this.defaultOptions = new(this.defaultTheme = defaultTheme);

    public IRawTheme GetDefaultTheme() => this.defaultTheme switch
    {
        ThemeName.QuietLight => LoadCustomQuietLightTheme(),
        ThemeName.Red => LoadCustomRedTheme(),
        _ => this.defaultOptions.GetDefaultTheme()
    };        

    public IRawGrammar GetGrammar(string scopeName) =>
        scopeName == pororocaJsonGrammarName ?
        cachedPororocaJsonGrammar.Value :
        this.defaultOptions.GetGrammar(scopeName);

    public ICollection<string> GetInjections(string scopeName) => this.defaultOptions.GetInjections(scopeName);

    public IRawTheme GetTheme(string scopeName) => this.defaultOptions.GetTheme(scopeName);

    public string GetScopeByLanguageId(string langId) =>
        langId == "json" || langId == "jsonc" ?
        pororocaJsonGrammarName :
        this.defaultOptions.GetScopeByLanguageId(langId);

    public IRawTheme LoadTheme(ThemeName themeName) => themeName switch
    {
        ThemeName.QuietLight => LoadCustomQuietLightTheme(),
        ThemeName.Red => LoadCustomRedTheme(),
        _ => this.defaultOptions.LoadTheme(themeName)
    };

    public Language GetLanguageByExtension(string extension) =>
        this.defaultOptions.GetLanguageByExtension(extension);

    public List<Language> GetAvailableLanguages() =>
        this.defaultOptions.GetAvailableLanguages();

    public void PreLoadPororocaJsonGrammar() =>
        _ = cachedPororocaJsonGrammar.Value;

    private static IRawGrammar LoadPororocaJsonGrammar()
    {
        Uri tmLangUri = new($"avares://Pororoca.Desktop/Assets/TextMateGrammars/JSONC_pororoca.tmLanguage.json");
        using var stream = AssetLoader.Open(tmLangUri);
        using StreamReader sr = new(stream);
        return GrammarReader.ReadGrammarSync(sr);
    }

    private static IRawTheme LoadCustomQuietLightTheme()
    {
        Uri tmThemeUri = new($"avares://Pororoca.Desktop/Assets/TextMateThemes/Custom-Quiet-Light-Theme.json");
        using var stream = AssetLoader.Open(tmThemeUri);
        using StreamReader sr = new(stream);
        return ThemeReader.ReadThemeSync(sr);
    }

    private static IRawTheme LoadCustomRedTheme()
    {
        Uri tmThemeUri = new($"avares://Pororoca.Desktop/Assets/TextMateThemes/Custom-Red-Theme.json");
        using var stream = AssetLoader.Open(tmThemeUri);
        using StreamReader sr = new(stream);
        return ThemeReader.ReadThemeSync(sr);
    }
}