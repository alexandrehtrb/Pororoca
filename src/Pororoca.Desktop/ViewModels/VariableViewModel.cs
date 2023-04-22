using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public class VariableViewModel : ViewModelBase
{
    private readonly ObservableCollection<VariableViewModel> parentCollection;

    [Reactive]
    public bool Enabled { get; set; }

    [Reactive]
    public string Key { get; set; }

    [Reactive]
    public string Value { get; set; }

    [Reactive]
    public bool IsSecret { get; set; }

    public ReactiveCommand<Unit, Unit> RemoveVariableCmd { get; }

    public VariableViewModel(ObservableCollection<VariableViewModel> parentCollection, PororocaVariable v)
        : this(parentCollection, v.Enabled, v.Key, v.Value ?? string.Empty, v.IsSecret)
    {
    }

    public VariableViewModel(ObservableCollection<VariableViewModel> parentCollection,
                             bool enabled, string key, string value, bool isSecret)
    {
        this.parentCollection = parentCollection;
        Enabled = enabled;
        Key = key;
        Value = value;
        IsSecret = isSecret;
        RemoveVariableCmd = ReactiveCommand.Create(RemoveVariable);
    }

    public PororocaVariable ToVariable() =>
        new(Enabled, Key, Value, IsSecret);

    public void RemoveVariable() =>
        this.parentCollection.Remove(this);
}