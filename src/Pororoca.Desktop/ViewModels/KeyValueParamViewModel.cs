using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public class KeyValueParamViewModel : ViewModelBase
{
    private bool enabledField;
    public bool Enabled
    {
        get => this.enabledField;
        set => this.RaiseAndSetIfChanged(ref this.enabledField, value);
    }

    private string keyField;
    public string Key
    {
        get => this.keyField;
        set => this.RaiseAndSetIfChanged(ref this.keyField, value);
    }

    private string valueField;
    public string Value
    {
        get => this.valueField;
        set => this.RaiseAndSetIfChanged(ref this.valueField, value);
    }

    public KeyValueParamViewModel(PororocaKeyValueParam p) : this(p.Enabled, p.Key, p.Value ?? string.Empty)
    {
    }

    public KeyValueParamViewModel(bool enabled, string key, string value)
    {
        this.enabledField = enabled;
        this.keyField = key;
        this.valueField = value;
    }

    public PororocaKeyValueParam ToKeyValueParam() =>
        new(this.enabledField, this.keyField, this.valueField);
}