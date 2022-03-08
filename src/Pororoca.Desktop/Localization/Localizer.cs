using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Pororoca.Desktop.Localization;

public class Localizer : INotifyPropertyChanged
{
    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";
    public static Localizer Instance { get; set; } = new Localizer();
    public event PropertyChangedEventHandler? PropertyChanged;
    public Language Language { get; private set; }
    private readonly List<Action> _languageChangedSubscriptions = new();

    public string this[string key]
    {
        get
        {
            if (_mappings != null && _mappings.TryGetValue(key, out string? res))
                return res.Replace("\\n", "\n");
            else
                return $"{Language}:{key}";
        }
    }
    private IDictionary<string, string>? _mappings = null;

    public bool LoadLanguage(Language language)
    {
        Language = language;
        IAssetLoader? assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

        Uri uri = new Uri($"avares://Pororoca.Desktop/Assets/i18n/{language.GetLanguageLCID()}.json");
        if (assets != null && assets.Exists(uri))
        {
            using Stream stringsFileUtf8Stream = assets.Open(uri);
            _mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(stringsFileUtf8Stream);
            Invalidate();

            return true;
        }
        return false;
    } // LoadLanguage

    public void Invalidate()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
        _languageChangedSubscriptions.ForEach(sub => sub.Invoke());
    }

    public void SubscribeToLanguageChange(Action onLanguageChanged) =>
        _languageChangedSubscriptions.Add(onLanguageChanged);
}