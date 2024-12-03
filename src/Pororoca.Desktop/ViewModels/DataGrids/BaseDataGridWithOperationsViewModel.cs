using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using Pororoca.Desktop.HotKeys;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public abstract class BaseDataGridWithOperationsViewModel<VM, D> : ViewModelBase
    where VM : notnull
    where D : notnull, new()
{
    public abstract SimpleClipboardArea<D> InnerClipboardArea { get; }

    public ObservableCollection<VM> Items { get; }

    public ConcurrentDictionary<VM, bool> SelectedItems { get; }

    public ReactiveCommand<Unit, Unit> AddNewCmd { get; }
    public ReactiveCommand<Unit, Unit> CutCmd { get; }
    public ReactiveCommand<Unit, Unit> CopyCmd { get; }
    public ReactiveCommand<Unit, Unit> PasteCmd { get; }
    public ReactiveCommand<Unit, Unit> DuplicateCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteCmd { get; }

    protected BaseDataGridWithOperationsViewModel(List<D>? initialValues = null)
    {
        AddNewCmd = ReactiveCommand.Create(AddNew);
        CutCmd = ReactiveCommand.Create(() => CutOrCopySelected(false));
        CopyCmd = ReactiveCommand.Create(() => CutOrCopySelected(true));
        PasteCmd = ReactiveCommand.CreateFromTask(PasteAsync);
        DuplicateCmd = ReactiveCommand.Create(DuplicateSelected);
        DeleteCmd = ReactiveCommand.Create(DeleteSelected);

        Items = new();
        SelectedItems = new();
        foreach (var v in (initialValues ?? new()))
        {
            Items.Add(ToVm(v));
        }
    }

    protected abstract VM ToVm(D domainObj);
    protected abstract D ToDomain(VM viewModel);
    protected abstract D MakeCopy(D domainObj);

    private void AddNew() =>
        Items.Add(ToVm(new D()));

    internal void CutOrCopySelected(bool falseIfCutTrueIfCopy)
    {
        if (SelectedItems is not null && !SelectedItems.IsEmpty)
        {
            // needs to generate a new array because of concurrency problems
            var vars = SelectedItems.Select(x => x.Key).ToArray();
            if (falseIfCutTrueIfCopy == false)
            {
                foreach (var v in vars)
                {
                    Items.Remove(v);
                }
            }
            var varsDomain = vars.Select(ToDomain).ToArray();
            InnerClipboardArea.PushToArea(varsDomain);
        }
    }

    internal virtual Task PasteAsync()
    {
        if (InnerClipboardArea.CanPaste)
        {
            var vars = InnerClipboardArea.FetchCopies();
            foreach (var v in vars)
            {
                Items.Add(ToVm(v));
            }
        }
        return Task.CompletedTask;
    }

    private void DuplicateSelected()
    {
        if (SelectedItems is not null && !SelectedItems.IsEmpty)
        {
            // needs to generate a new array because of concurrency problems
            var varsToDuplicate = SelectedItems.Select(x => x.Key).ToArray();
            foreach (var v in varsToDuplicate)
            {
                var vCopy = ToVm(ToDomain(v));
                int position = Items.IndexOf(v);
                Items.Insert(position, vCopy);
            }
        }
    }

    internal void DeleteSelected()
    {
        if (SelectedItems is not null && !SelectedItems.IsEmpty)
        {
            // needs to generate a new array because of concurrency problems
            var varsToDelete = SelectedItems.Select(x => x.Key).ToArray();
            foreach (var v in varsToDelete)
            {
                Items.Remove(v);
            }
        }
    }

    internal List<D> ConvertItemsToDomain() =>
        Items.Select(ToDomain).ToList();

    internal void UpdateSelectedItems(SelectionChangedEventArgs e)
    {
        foreach (object av in e.AddedItems)
        {
            SelectedItems.TryAdd((VM)av, true);
        }
        foreach (object rv in e.RemovedItems)
        {
            SelectedItems.TryRemove((VM)rv, out _);
        }
    }
}