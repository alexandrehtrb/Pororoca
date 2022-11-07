using System.Text.RegularExpressions;
using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace Pororoca.Desktop.TextEditorConfig;

internal class CustomTextMateRegistryOptions : IRegistryOptions
{
    private readonly RegistryOptions defaultOptions;

    private readonly IAssetLoader assets;

    public CustomTextMateRegistryOptions(IAssetLoader assets, ThemeName defaultTheme)
    {
        this.assets = assets;
        this.defaultOptions = new(defaultTheme);
    }

    public IRawTheme GetDefaultTheme() => this.defaultOptions.GetDefaultTheme();

    public IRawGrammar GetGrammar(string scopeName) =>
        scopeName == "source.json.pororoca" ?
        GrammarReader.ReadGrammarSync(new StreamReader(this.assets!.Open(new Uri($"avares://Pororoca.Desktop/Assets/TextMateGrammars/JSONC_pororoca.tmLanguage.json")))) :
        this.defaultOptions.GetGrammar(scopeName);

    public ICollection<string> GetInjections(string scopeName) => this.defaultOptions.GetInjections(scopeName);

    public IRawTheme GetTheme(string scopeName) => this.defaultOptions.GetTheme(scopeName);

    public string GetScopeByLanguageId(string langId) =>
        langId == "json" || langId == "jsonc" ?
        "source.json.pororoca" :
        this.defaultOptions.GetScopeByLanguageId(langId);

    public IRawTheme LoadTheme(ThemeName themeName) =>
        this.defaultOptions.LoadTheme(themeName);

    public Language GetLanguageByExtension(string extension) =>
        this.defaultOptions.GetLanguageByExtension(extension);

    public List<Language> GetAvailableLanguages() =>
        this.defaultOptions.GetAvailableLanguages();
}