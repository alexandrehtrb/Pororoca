using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Pororoca.Desktop.Localization;

public class LocalizeExtension : MarkupExtension
{
    public string Key { get; set; }

    public string? Context { get; set; }

    public LocalizeExtension(string key) =>
        Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        string keyToUse = Key;
        if (!string.IsNullOrWhiteSpace(Context))
            keyToUse = $"{Context}/{Key}";

        var binding = new ReflectionBindingExtension($"[{keyToUse}]")
        {
            Mode = BindingMode.OneWay,
            Source = Localizer.Instance,
        };

        return binding.ProvideValue(serviceProvider);
    }
}