using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public class KeyValueParamViewModel : ViewModelBase
{
    [Reactive]
    public bool Enabled { get; set; }

    [Reactive]
    public string Key { get; set; }

    [Reactive]
    public string Value { get; set; }

    public KeyValueParamViewModel(PororocaKeyValueParam p) : this(p.Enabled, p.Key, p.Value ?? string.Empty)
    {
    }

    public KeyValueParamViewModel(bool enabled, string key, string value)
    {
        Enabled = enabled;
        Key = key;
        Value = value;
    }

    public PororocaKeyValueParam ToKeyValueParam() =>
        new(Enabled, Key, Value);
}