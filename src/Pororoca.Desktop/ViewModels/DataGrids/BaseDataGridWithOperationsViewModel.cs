using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.HotKeys;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public abstract class BaseDataGridWithOperationsViewModel<VM, D> : ViewModelBase
    where VM : notnull
    where D : notnull, new()
{
    protected abstract SimpleClipboardArea<D> InnerClipboardArea { get; }

    public ObservableCollection<VM> Items { get; }

    public ConcurrentDictionary<VM, bool> SelectedItems { get; }

    public ReactiveCommand<Unit, Unit> AddNewCmd { get; }
    public ReactiveCommand<Unit, Unit> CutSelectedCmd { get; }
    public ReactiveCommand<Unit, Unit> CopySelectedCmd { get; }
    public ReactiveCommand<Unit, Unit> PasteCmd { get; }
    public ReactiveCommand<Unit, Unit> DuplicateSelectedCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteSelectedCmd { get; }

    public BaseDataGridWithOperationsViewModel(List<D> initialValues)
    {
        AddNewCmd = ReactiveCommand.Create(AddNew);
        CutSelectedCmd = ReactiveCommand.Create(() => CutOrCopySelected(false));
        CopySelectedCmd = ReactiveCommand.Create(() => CutOrCopySelected(true));
        PasteCmd = ReactiveCommand.Create(Paste);
        DuplicateSelectedCmd = ReactiveCommand.Create(DuplicateSelected);
        DeleteSelectedCmd = ReactiveCommand.Create(DeleteSelected);

        Items = new();
        SelectedItems = new();
        foreach (var v in initialValues)
        {
            Items.Add(ToVm(v));
        }
    }

    protected abstract VM ToVm(D domainObj);
    protected abstract D ToDomain(VM viewModel);
    protected abstract D MakeCopy(D domainObj);

    private void AddNew() =>
        Items.Add(ToVm(new D()));

    private void CutOrCopySelected(bool falseIfCutTrueIfCopy)
    {
        if (SelectedItems is not null && SelectedItems.Count > 0)
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

    private void Paste()
    {
        if (InnerClipboardArea.CanPaste)
        {
            var vars = InnerClipboardArea.FetchCopies();
            foreach (var v in vars)
            {
                Items.Add(ToVm(v));
            }
        }
    }
    
    private void DuplicateSelected()
    {
        if (SelectedItems is not null && SelectedItems.Count > 0)
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

    private void DeleteSelected()
    {
        if (SelectedItems is not null && SelectedItems.Count > 0)
        {
            // needs to generate a new array because of concurrency problems
            var varsToDelete = SelectedItems.Select(x => x.Key).ToArray();
            foreach (var v in varsToDelete)
            {
                Items.Remove(v);
            }
        }
    }
}