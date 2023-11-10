using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using Pororoca.Desktop.HotKeys;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public abstract class BaseDataGridWithOperationsViewModel<VM, D> : ViewModelBase
    where VM : class
    where D : notnull, new()
{
    public abstract SimpleClipboardArea<D> InnerClipboardArea { get; }

    public ObservableCollection<VM> Items { get; }

    public FlatTreeDataGridSource<VM> Source { get; }

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
        PasteCmd = ReactiveCommand.Create(Paste);
        DuplicateCmd = ReactiveCommand.Create(DuplicateSelected);
        DeleteCmd = ReactiveCommand.Create(DeleteSelected);

        Items = new(); // this is necessary because ToVm() uses Items property, which cannot be null
        if (initialValues is not null)
        {
            foreach (var v in initialValues)
            {
                Items.Add(ToVm(v));
            }
        }
        Source = GenerateDataGridSource();
        Source.RowSelection!.SingleSelect = false;
    }

    protected abstract FlatTreeDataGridSource<VM> GenerateDataGridSource();
    protected abstract VM ToVm(D domainObj);
    protected abstract D ToDomain(VM viewModel);
    protected abstract D MakeCopy(D domainObj);

    private void AddNew() =>
        Items.Add(ToVm(new D()));

    internal void CutOrCopySelected(bool falseIfCutTrueIfCopy)
    {
        if (Source.RowSelection is not null)
        {
            // needs to generate a new array because of concurrency problems
            var vars = Source.RowSelection.SelectedItems.ToArray();
            if (falseIfCutTrueIfCopy == false)
            {
                foreach (var v in vars)
                {
                    Items.Remove(v!);
                }
            }
            var varsDomain = vars.Select(x => ToDomain(x!)).ToArray();
            InnerClipboardArea.PushToArea(varsDomain);
        }
    }

    internal void Paste()
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
        if (Source.RowSelection is not null)
        {
            // needs to generate a new array because of concurrency problems
            var varsToDuplicate = Source.RowSelection.SelectedItems.ToArray();
            foreach (var v in varsToDuplicate)
            {
                var vCopy = ToVm(ToDomain(v!));
                int position = Items.IndexOf(v!);
                Items.Insert(position, vCopy);
            }
        }
    }

    internal void DeleteSelected()
    {
        if (Source.RowSelection is not null)
        {
            // needs to generate a new array because of concurrency problems
            var varsToDelete = Source.RowSelection.SelectedItems.ToArray();
            foreach (var v in varsToDelete)
            {
                Items.Remove(v!);
            }
        }
    }

    internal D[] ConvertItemsToDomain() =>
        Items.Select(ToDomain).ToArray();
}