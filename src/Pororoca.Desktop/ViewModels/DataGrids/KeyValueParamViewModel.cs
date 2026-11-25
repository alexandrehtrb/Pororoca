using System.Collections.ObjectModel;
using ReactiveUI.Primitives;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed partial class KeyValueParamViewModel : ViewModelBase
{
    private readonly ObservableCollection<KeyValueParamViewModel> parentCollection;

    [Reactive]
    public partial bool Enabled { get; set; }

    [Reactive]
    public partial string Key { get; set; }

    [Reactive]
    public partial string Value { get; set; }

    public ReactiveCommand<RxVoid, RxVoid> RemoveParamCmd { get; }

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