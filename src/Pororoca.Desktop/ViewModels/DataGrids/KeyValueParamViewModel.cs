using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class KeyValueParamViewModel : ViewModelBase
{
    private readonly ObservableCollection<KeyValueParamViewModel> parentCollection;

    [Reactive]
    public bool Enabled { get; set; }

    [Reactive]
    public string Key { get; set; }

    [Reactive]
    public string Value { get; set; }

    public ReactiveCommand<Unit, Unit> RemoveParamCmd { get; }

    public KeyValueParamViewModel(ObservableCollection<KeyValueParamViewModel> parentCollection, PororocaKeyValueParam p)
        : this(parentCollection, p.Enabled, p.Key, p.Value ?? string.Empty)
    {
    }

    public KeyValueParamViewModel(ObservableCollection<KeyValueParamViewModel> parentCollection,
        bool enabled, string key, string value)
    {
        this.parentCollection = parentCollection;
        Enabled = enabled;
        Key = key;
        Value = value;
        RemoveParamCmd = ReactiveCommand.Create(RemoveParam);
    }

    public PororocaKeyValueParam ToKeyValueParam() =>
        new(Enabled, Key, Value);

    private void RemoveParam() =>
        this.parentCollection.Remove(this);
}