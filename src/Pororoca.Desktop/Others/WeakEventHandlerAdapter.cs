using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Pororoca.Desktop.Others;

/// <summary>
/// Esta classe é necessária para fazer WeakReferences. NÃO REMOVER.
/// https://pragmateek.com/the-net-weak-event-pattern-in-c/
/// </summary>
/// <typeparam name="TObject"></typeparam>
/// <typeparam name="TArgs"></typeparam>
internal class WeakEventHandlerAdapter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicEvents)] TObject, TArgs> : IDisposable where TArgs : EventArgs
{
    // Fields.
    private readonly EventInfo eventInfo;
    private readonly EventHandler<TArgs> handlerStub;
    private readonly WeakReference<EventHandler<TArgs>> handlerRef;
    private int isDisposed;
    private readonly SynchronizationContext? syncContext;
    private readonly TObject target;

    // Constructor.
    internal WeakEventHandlerAdapter(TObject target, string eventName, EventHandler<TArgs> handler)
    {
        this.eventInfo = typeof(TObject).GetEvent(eventName) ?? throw new ArgumentException($"Cannot find event '{eventName}' in {typeof(TObject).Name}.");
        // ReSharper disable VirtualMemberCallInConstructor
        this.handlerStub = CreateEventHandlerStub();
        // ReSharper restore VirtualMemberCallInConstructor
        this.handlerRef = new(handler);
        this.syncContext = SynchronizationContext.Current;
        this.target = target;
        this.eventInfo.AddEventHandler(target, this.handlerStub);
    }

    // Called to create stub of event handler.
    protected EventHandler<TArgs> CreateEventHandlerStub() =>
        OnEventReceived;

    private void OnEventReceived(object? sender, TArgs e) =>
        InvokeEventHandler(sender, e);

    // Dispose.
    public void Dispose()
    {
        if (Interlocked.Exchange(ref this.isDisposed, 1) != 0)
            return;
        if (this.syncContext != null && this.syncContext != SynchronizationContext.Current)
        {
            try
            {
                this.syncContext.Post(_ => this.eventInfo.RemoveEventHandler(this.target, this.handlerStub), null);
                return;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            { }
            // ReSharper restore EmptyGeneralCatchClause
        }
        this.eventInfo.RemoveEventHandler(this.target, this.handlerStub);
    }

    // Invoke event handler.
    private void InvokeEventHandler(params object?[] args)
    {
        if (this.handlerRef.TryGetTarget(out var handler))
            handler.DynamicInvoke(args);
        else
            Dispose();
    }
}