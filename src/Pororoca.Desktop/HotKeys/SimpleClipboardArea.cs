using Pororoca.Desktop.ViewModels;
using ReactiveUI.SourceGenerators;

namespace Pororoca.Desktop.HotKeys;

public abstract partial class SimpleClipboardArea<T> : ViewModelBase
{
    protected readonly List<T> copied = new();

    [Reactive]
    public virtual partial bool CanPaste { get; private set; }

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