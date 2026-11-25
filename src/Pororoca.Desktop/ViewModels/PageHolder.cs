using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public abstract class PageHolder : ReactiveObject
{
    public abstract Type PageType { get; }
    public bool Visible
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public abstract void SetVM(ViewModelBase? vm);
}

public sealed class PageHolder<X> : PageHolder where X : ViewModelBase
{
    public override Type PageType => typeof(X);

    public X? VM
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public override void SetVM(ViewModelBase? vm) =>
        VM = (X?)vm;
}