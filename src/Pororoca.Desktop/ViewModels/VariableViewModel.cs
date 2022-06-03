using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public class VariableViewModel : ViewModelBase
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

    private bool isSecretField;
    public bool IsSecret
    {
        get => this.isSecretField;
        set => this.RaiseAndSetIfChanged(ref this.isSecretField, value);
    }

    public VariableViewModel(PororocaVariable v) : this(v.Enabled, v.Key, v.Value ?? string.Empty, v.IsSecret)
    {
    }

    public VariableViewModel(bool enabled, string key, string value, bool isSecret)
    {
        this.enabledField = enabled;
        this.keyField = key;
        this.valueField = value;
        this.isSecretField = isSecret;
    }

    public PororocaVariable ToVariable() =>
        new(this.enabledField, this.keyField, this.valueField, this.isSecretField);
}