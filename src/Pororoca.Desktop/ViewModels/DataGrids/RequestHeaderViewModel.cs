using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class RequestHeaderViewModel : ViewModelBase
{
    private readonly ObservableCollection<RequestHeaderViewModel> parentCollection;

    [Reactive]
    public bool Enabled { get; set; }

    private string keyField;
    public string Key
    {
        get => this.keyField;
        set
        {
            string? commonHeader = MostCommonHeaders.FirstOrDefault(x => x.Equals(value, StringComparison.InvariantCultureIgnoreCase));
            if (commonHeader is not null && string.IsNullOrEmpty(Value))
            {
                Value = ProvideSampleValueForHeader(commonHeader);
            }
            this.RaiseAndSetIfChanged(ref this.keyField, commonHeader ?? value);
        }
    }

    [Reactive]
    public string Value { get; set; }

    public ReactiveCommand<Unit, Unit> RemoveParamCmd { get; }

    public RequestHeaderViewModel(ObservableCollection<RequestHeaderViewModel> parentCollection, PororocaKeyValueParam p)
        : this(parentCollection, p.Enabled, p.Key, p.Value ?? string.Empty)
    {
    }

    public RequestHeaderViewModel(ObservableCollection<RequestHeaderViewModel> parentCollection,
        bool enabled, string key, string value)
    {
        this.parentCollection = parentCollection;
        Enabled = enabled;
        this.keyField = key;
        Value = value;
        RemoveParamCmd = ReactiveCommand.Create(RemoveParam);
    }

    public PororocaKeyValueParam ToKeyValueParam() =>
        new(Enabled, Key, Value);

    private void RemoveParam() =>
        this.parentCollection.Remove(this);
}