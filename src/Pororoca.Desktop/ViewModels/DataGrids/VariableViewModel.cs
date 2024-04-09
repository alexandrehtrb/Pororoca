using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class VariableViewModel : ViewModelBase
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

    public ReactiveCommand<Unit, Unit> MoveVariableUpCmd { get; }

    public ReactiveCommand<Unit, Unit> MoveVariableDownCmd { get; }

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
        MoveVariableUpCmd = ReactiveCommand.Create(MoveVariableUp);
        MoveVariableDownCmd = ReactiveCommand.Create(MoveVariableDown);
    }

    public PororocaVariable ToVariable() =>
        new(Enabled, Key, Value, IsSecret);

    public void RemoveVariable() =>
        this.parentCollection.Remove(this);

    private void MoveVariableUp()
    {
        int oldIndex = this.parentCollection.IndexOf(this);
        if (oldIndex == 0) return;
        int newIndex = oldIndex - 1;
        MoveVariable(oldIndex, newIndex);
    }

    private void MoveVariableDown()
    {
        int oldIndex = this.parentCollection.IndexOf(this);
        if (oldIndex == this.parentCollection.Count - 1) return;
        int newIndex = oldIndex + 1;
        MoveVariable(oldIndex, newIndex);
    }

    private void MoveVariable(int oldIndex, int newIndex)
    {
        // For some reason, ObservableCollection.Move() does not work here with DataGrid
        this.parentCollection.RemoveAt(oldIndex);
        this.parentCollection.Insert(newIndex, this);
    }
}