using Pororoca.Desktop.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.HotKeys;

public abstract class SimpleClipboardArea<T> : ViewModelBase
{
    protected readonly List<T> copied = new();

    [Reactive]
    public bool CanPaste { get; private set; }

    public void Clear()
    {
        this.copied.Clear();
        CanPaste = false;
    }

    public void PushToArea(params T[] itemsToCopy)
    {
        this.copied.Clear();
        this.copied.AddRange(itemsToCopy);
        CanPaste = this.copied.Count > 0;
    }

    public abstract List<T> FetchCopies();
}