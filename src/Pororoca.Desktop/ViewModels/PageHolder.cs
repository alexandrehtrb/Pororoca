using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public abstract class PageHolder : ReactiveObject
{
    public abstract Type PageType { get; }

    private bool visibleField;
    public bool Visible
    {
        get => this.visibleField;
        set => this.RaiseAndSetIfChanged(ref this.visibleField, value);
    }

    public abstract void SetVM(ViewModelBase? vm);
}

public sealed class PageHolder<X> : PageHolder where X : ViewModelBase
{
    public override Type PageType => typeof(X);

    private X? vmField;
    public X? VM
    {
        get => this.vmField;
        private set => this.RaiseAndSetIfChanged(ref this.vmField, value);
    }

    public override void SetVM(ViewModelBase? vm) =>
        VM = (X?)vm;
}