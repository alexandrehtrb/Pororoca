using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public class VariableViewModel : ViewModelBase
{
    [Reactive]
    public bool Enabled { get; set; }

    [Reactive]
    public string Key { get; set; }

    [Reactive]
    public string Value { get; set; }

    [Reactive]
    public bool IsSecret { get; set; }

    public VariableViewModel(PororocaVariable v) : this(v.Enabled, v.Key, v.Value ?? string.Empty, v.IsSecret)
    {
    }

    public VariableViewModel(bool enabled, string key, string value, bool isSecret)
    {
        Enabled = enabled;
        Key = key;
        Value = value;
        IsSecret = isSecret;
    }

    public PororocaVariable ToVariable() =>
        new(Enabled, Key, Value, IsSecret);
}