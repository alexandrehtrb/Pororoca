using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Avalonia.Xaml.Interactivity;

namespace Pororoca.Desktop.Behaviors;

/// <summary>
///
/// </summary>
public sealed class ContextDragWithDirectionBehavior : Behavior<Control>
{
    private Point _dragStartPoint;
    private PointerEventArgs? _triggerEvent;
    private bool _lock;
    private bool _captured;

    /// <summary>
    ///
    /// </summary>
    public static readonly StyledProperty<object?> ContextProperty =
        AvaloniaProperty.Register<ContextDragBehavior, object?>(nameof(Context));

    /// <summary>
    ///
    /// </summary>
    public static readonly StyledProperty<IDragHandler?> HandlerProperty =
        AvaloniaProperty.Register<ContextDragBehavior, IDragHandler?>(nameof(Handler));

    /// <summary>
    ///
    /// </summary>
    public static readonly StyledProperty<double> HorizontalDragThresholdProperty =
        AvaloniaProperty.Register<ContextDragBehavior, double>(nameof(HorizontalDragThreshold), 3);

    /// <summary>
    ///
    /// </summary>
    public static readonly StyledProperty<double> VerticalDragThresholdProperty =
        AvaloniaProperty.Register<ContextDragBehavior, double>(nameof(VerticalDragThreshold), 3);

    /// <summary>
    ///
    /// </summary>
    public object? Context
    {
        get => GetValue(ContextProperty);
        set => SetValue(ContextProperty, value);
    }

    /// <summary>
    ///
    /// </summary>
    public IDragHandler? Handler
    {
        get => GetValue(HandlerProperty);
        set => SetValue(HandlerProperty, value);
    }

    /// <summary>
    ///
    /// </summary>
    public double HorizontalDragThreshold
    {
        get => GetValue(HorizontalDragThresholdProperty);
        set => SetValue(HorizontalDragThresholdProperty, value);
    }

    /// <summary>
    ///
    /// </summary>
    public double VerticalDragThreshold
    {
        get => GetValue(VerticalDragThresholdProperty);
        set => SetValue(VerticalDragThresholdProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        AssociatedObject?.AddHandler(InputElement.PointerPressedEvent, AssociatedObject_PointerPressed, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AssociatedObject?.AddHandler(InputElement.PointerReleasedEvent, AssociatedObject_PointerReleased, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AssociatedObject?.AddHandler(InputElement.PointerMovedEvent, AssociatedObject_PointerMoved, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AssociatedObject?.AddHandler(InputElement.PointerCaptureLostEvent, AssociatedObject_CaptureLost, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        AssociatedObject?.RemoveHandler(InputElement.PointerPressedEvent, AssociatedObject_PointerPressed);
        AssociatedObject?.RemoveHandler(InputElement.PointerReleasedEvent, AssociatedObject_PointerReleased);
        AssociatedObject?.RemoveHandler(InputElement.PointerMovedEvent, AssociatedObject_PointerMoved);
        AssociatedObject?.RemoveHandler(InputElement.PointerCaptureLostEvent, AssociatedObject_CaptureLost);
    }

    private async Task DoDragDrop(PointerEventArgs triggerEvent, object? value, string upOrDown)
    {
        var data = new DataObject();
        data.Set(ContextDropBehavior.DataFormat, value!);
        data.Set("direction", upOrDown);

        var effect = DragDropEffects.None;

        if (triggerEvent.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            effect |= DragDropEffects.Link;
        }
        else if (triggerEvent.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            effect |= DragDropEffects.Move;
        }
        else if (triggerEvent.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            effect |= DragDropEffects.Copy;
        }
        else
        {
            effect |= DragDropEffects.Move;
        }

        await DragDrop.DoDragDrop(triggerEvent, data, effect);
    }

    private void Released()
    {
        this._triggerEvent = null;
        this._lock = false;
    }

    private void AssociatedObject_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (properties.IsLeftButtonPressed)
        {
            if (e.Source is Control control
                && AssociatedObject?.DataContext == control.DataContext)
            {
                this._dragStartPoint = e.GetPosition(null);
                this._triggerEvent = e;
                this._lock = true;
                this._captured = true;
            }
        }
    }

    private void AssociatedObject_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (this._captured)
        {
            if (e.InitialPressMouseButton == MouseButton.Left && this._triggerEvent is { })
            {
                Released();
            }

            this._captured = false;
        }
    }

    private async void AssociatedObject_PointerMoved(object? sender, PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (this._captured
            && properties.IsLeftButtonPressed &&
            this._triggerEvent is { })
        {
            var point = e.GetPosition(null);
            var diff = this._dragStartPoint - point;
            double horizontalDragThreshold = HorizontalDragThreshold;
            double verticalDragThreshold = VerticalDragThreshold;

            if (Math.Abs(diff.X) > horizontalDragThreshold || Math.Abs(diff.Y) > verticalDragThreshold)
            {
                if (this._lock)
                {
                    this._lock = false;
                }
                else
                {
                    return;
                }

                object? context = Context ?? AssociatedObject?.DataContext;

                Handler?.BeforeDragDrop(sender, this._triggerEvent, context);

                await DoDragDrop(this._triggerEvent, context, diff.Y > 0 ? "up" : "down");

                Handler?.AfterDragDrop(sender, this._triggerEvent, context);

                this._triggerEvent = null;
            }
        }
    }

    private void AssociatedObject_CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        Released();
        this._captured = false;
    }
}