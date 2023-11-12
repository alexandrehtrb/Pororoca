using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Rendering;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using CollectionExtensions = Avalonia.Controls.Models.TreeDataGrid.CollectionExtensions;

namespace Avalonia.Controls.Primitives
{
    public abstract class TreeDataGridPresenterBase<TItem> : Border
    {
#pragma warning disable AVP1002
        public static readonly DirectProperty<TreeDataGridPresenterBase<TItem>, TreeDataGridElementFactory?>
            ElementFactoryProperty =
                AvaloniaProperty.RegisterDirect<TreeDataGridPresenterBase<TItem>, TreeDataGridElementFactory?>(
                    nameof(ElementFactory),
                    o => o.ElementFactory,
                    (o, v) => o.ElementFactory = v);

        public static readonly DirectProperty<TreeDataGridPresenterBase<TItem>, IReadOnlyList<TItem>?> ItemsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridPresenterBase<TItem>, IReadOnlyList<TItem>?>(
                nameof(Items),
                o => o.Items,
                (o, v) => o.Items = v);
#pragma warning restore AVP1002
        private static readonly Rect s_invalidViewport = new(double.PositiveInfinity, double.PositiveInfinity, 0, 0);
        private readonly Action<Control, int> _recycleElement;
        private readonly Action<Control> _recycleElementOnItemRemoved;
        private readonly Action<Control, int, int> _updateElementIndex;
        private int _scrollToIndex = -1;
        private Control? _scrollToElement;
        private TreeDataGridElementFactory? _elementFactory;
        private bool _isInLayout;
        private bool _isWaitingForViewportUpdate;
        private IReadOnlyList<TItem>? _items;
        private RealizedStackElements? _measureElements;
        private RealizedStackElements? _realizedElements;
        private ScrollViewer? _scrollViewer;
        private double _lastEstimatedElementSizeU = 25;
        private Control? _focusedElement;
        private int _focusedIndex = -1;

        public TreeDataGridPresenterBase()
        {
            _recycleElement = RecycleElement;
            _recycleElementOnItemRemoved = RecycleElementOnItemRemoved;
            _updateElementIndex = UpdateElementIndex;
            EffectiveViewportChanged += OnEffectiveViewportChanged;
        }

        public TreeDataGridElementFactory? ElementFactory
        {
            get => _elementFactory;
            set => SetAndRaise(ElementFactoryProperty, ref _elementFactory, value);
        }

        public IReadOnlyList<TItem>? Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    if (_items is INotifyCollectionChanged oldIncc)
                        oldIncc.CollectionChanged -= OnItemsCollectionChanged;

                    var oldValue = _items;
                    _items = value;

                    if (_items is INotifyCollectionChanged newIncc)
                        newIncc.CollectionChanged += OnItemsCollectionChanged;

                    RaisePropertyChanged(
                        ItemsProperty,
                        oldValue,
                        _items);
                    OnItemsCollectionChanged(null, CollectionExtensions.ResetEvent);
                }
            }
        }

        internal IReadOnlyList<Control?> RealizedElements => _realizedElements?.Elements ?? Array.Empty<Control>();

        protected abstract Orientation Orientation { get; }
        protected Rect Viewport { get; private set; } = s_invalidViewport;

        public Control? BringIntoView(int index, Rect? rect = null)
        {
            var items = Items;

            if (_isInLayout || items is null || index < 0 || index >= items.Count || _realizedElements is null)
                return null;

            if (GetRealizedElement(index) is Control element)
            {
                if (rect.HasValue)
                    element.BringIntoView(rect.Value);
                else
                    element.BringIntoView();
                return element;
            }
            else if (this.IsAttachedToVisualTree())
            {
                // Create and measure the element to be brought into view. Store it in a field so that
                // it can be re-used in the layout pass.
                var scrollToElement = GetOrCreateElement(items, index);
                scrollToElement.Measure(Size.Infinity);

                // Get the expected position of the element and put it in place.
                var anchorU = _realizedElements.GetOrEstimateElementU(index, ref _lastEstimatedElementSizeU);
                var elementRect = Orientation == Orientation.Horizontal ?
                    new Rect(anchorU, 0, scrollToElement.DesiredSize.Width, scrollToElement.DesiredSize.Height) :
                    new Rect(0, anchorU, scrollToElement.DesiredSize.Width, scrollToElement.DesiredSize.Height);
                scrollToElement.Arrange(elementRect);

                // Store the element and index so that they can be used in the layout pass.
                _scrollToElement = scrollToElement;
                _scrollToIndex = index;

                // If the item being brought into view was added since the last layout pass then
                // our bounds won't be updated, so any containing scroll viewers will not have an
                // updated extent. Do a layout pass to ensure that the containing scroll viewers
                // will be able to scroll the new item into view.
                if (!Bounds.Contains(elementRect) && !Viewport.Contains(elementRect))
                {
                    _isWaitingForViewportUpdate = true;
                    UpdateLayout();
                    _isWaitingForViewportUpdate = false;
                }

                // Try to bring the item into view and do a layout pass.
                if (rect.HasValue)
                    scrollToElement.BringIntoView(rect.Value);
                else
                    scrollToElement.BringIntoView();

                // If the viewport does not contain the item to scroll to, set _isWaitingForViewportUpdate:
                // this should cause the following chain of events:
                // - Measure is first done with the old viewport (which will be a no-op, see MeasureOverride)
                // - The viewport is then updated by the layout system which invalidates our measure
                // - Measure is then done with the new viewport.
                _isWaitingForViewportUpdate = !Viewport.Contains(elementRect);
                UpdateLayout();

                // If for some reason the layout system didn't give us a new viewport during the layout, we
                // need to do another layout pass as the one that took place was a no-op.
                if (_isWaitingForViewportUpdate)
                {
                    _isWaitingForViewportUpdate = false;
                    InvalidateMeasure();
                    UpdateLayout();
                }

                _scrollToElement = null;
                _scrollToIndex = -1;
                return scrollToElement;
            }

            return null;
        }

        public IEnumerable<Control> GetRealizedElements()
        {
            if (_realizedElements is not null)
                return _realizedElements.Elements.Where(x => x is not null)!;
            else
                return Array.Empty<Control>();
        }

        public Control? TryGetElement(int index) => GetRealizedElement(index);

        internal void RecycleAllElements() => _realizedElements?.RecycleAllElements(_recycleElement);

        internal void RecycleAllElementsOnItemRemoved()
        {
            if (_realizedElements?.Count > 0)
            {
                _realizedElements?.ItemsRemoved(
                    _realizedElements.FirstIndex,
                    _realizedElements.Count,
                    _updateElementIndex,
                    _recycleElementOnItemRemoved);
            }
        }

        protected virtual Rect ArrangeElement(int index, Control element, Rect rect)
        {
            element.Arrange(rect);
            return rect;
        }

        protected virtual Size MeasureElement(int index, Control element, Size availableSize)
        {
            element.Measure(availableSize);
            return element.DesiredSize;
        }

        /// <summary>
        /// Gets the initial constraint for the first pass of the two-pass measure.
        /// </summary>
        /// <param name="element">The element being measured.</param>
        /// <param name="index">The index of the element.</param>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The measure constraint for the element.</returns>
        /// <remarks>
        /// The measure pass is split into two parts:
        /// 
        /// - The initial pass is used to determine the "natural" size of the elements. In this
        ///   pass, infinity can be used as the measure constraint if the element has no other
        ///   constraints on its size.
        /// - The final pass is made once the "natural" sizes of the elements are known and any
        ///   layout logic has been run. This pass is needed because controls should not be 
        ///   arranged with a size less than that passed as the constraint during the measure
        ///   pass. This pass is only run if <see cref="InitialMeasurePassComplete"/> returns
        ///   true.
        /// </remarks>
        protected virtual Size GetInitialConstraint(
            Control element,
            int index,
            Size availableSize)
        {
            return availableSize;
        }

        /// <summary>
        /// Called when the initial pass of the two-pass measure has been completed, in order to determine
        /// whether a final measure pass is necessary.
        /// </summary>
        /// <param name="firstIndex">The index of the first element in <paramref name="elements"/>.</param>
        /// <param name="elements">The elements being measured.</param>
        /// <returns>
        /// true if a final pass should be run; otherwise false.
        /// </returns>
        /// <see cref="GetInitialConstraint(Control, int, Size)"/>
        protected virtual bool NeedsFinalMeasurePass(
            int firstIndex,
            IReadOnlyList<Control?> elements) => false;

        /// <summary>
        /// Gets the final constraint for the second pass of the two-pass measure.
        /// </summary>
        /// <param name="element">The element being measured.</param>
        /// <param name="index">The index of the element.</param>
        /// <param name="availableSize">The available size.</param>
        /// <returns>
        /// The measure constraint for the element.
        /// </returns>
        /// <see cref="GetInitialConstraint(Control, int, Size)"/>
        protected virtual Size GetFinalConstraint(
            Control element,
            int index,
            Size availableSize)
        {
            return element.DesiredSize;
        }

        protected virtual Control GetElementFromFactory(TItem item, int index)
        {
            return GetElementFromFactory(item!, index, this);
        }

        protected Control GetElementFromFactory(object data, int index, Control parent)
        {
            return _elementFactory!.GetOrCreateElement(data, parent);
        }

        protected virtual (int index, double position) GetElementAt(double position) => (-1, -1);
        protected virtual double GetElementPosition(int index) => -1;
        protected abstract void RealizeElement(Control element, TItem item, int index);
        protected abstract void UpdateElementIndex(Control element, int oldIndex, int newIndex);
        protected abstract void UnrealizeElement(Control element);

        protected virtual double CalculateSizeU(Size availableSize)
        {
            if (Items is null)
                return 0;

            // Return the estimated size of all items based on the elements currently realized.
            return EstimateElementSizeU() * Items.Count;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var items = Items;

            if (items is null || items.Count == 0)
            {
                TrimUnrealizedChildren();
                return default;
            }

            // If we're bringing an item into view, ignore any layout passes until we receive a new
            // effective viewport.
            if (_isWaitingForViewportUpdate)
                return DesiredSize;

            _isInLayout = true;

            try
            {
                var orientation = Orientation;

                _realizedElements ??= new();
                _measureElements ??= new();

                // We handle horizontal and vertical layouts here so X and Y are abstracted to:
                // - Horizontal layouts: U = horizontal, V = vertical
                // - Vertical layouts: U = vertical, V = horizontal
                var viewport = CalculateMeasureViewport(items, availableSize);

                // If the viewport is disjunct then we can recycle everything.
                if (viewport.viewportIsDisjunct)
                    _realizedElements.RecycleAllElements(_recycleElement);

                // Do the measure, creating/recycling elements as necessary to fill the viewport. Don't
                // write to _realizedElements yet, only _measureElements.
                RealizeElements(items, availableSize, ref viewport);

                // Run the final measure pass if necessary.
                if (NeedsFinalMeasurePass(_measureElements.FirstIndex, _measureElements.Elements))
                {
                    var count = _measureElements.Count;

                    for (var i = 0; i < count; ++i)
                    {
                        var e = _measureElements.Elements[i]!;
                        var previous = LayoutInformation.GetPreviousMeasureConstraint(e)!.Value;

                        if (HasInfinity(previous))
                        {
                            var index = _measureElements.FirstIndex + i;
                            var constraint = GetFinalConstraint(e, index, availableSize);
                            e.Measure(constraint);
                            viewport.measuredV = Math.Max(
                                viewport.measuredV,
                                Orientation == Orientation.Horizontal ?
                                    e.DesiredSize.Height : e.DesiredSize.Width);
                        }
                    }
                }

                // Now swap the measureElements and realizedElements collection.
                (_measureElements, _realizedElements) = (_realizedElements, _measureElements);
                _measureElements.ResetForReuse();

                TrimUnrealizedChildren();

                return CalculateDesiredSize(orientation, items.Count, viewport);
            }
            finally
            {
                _isInLayout = false;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_realizedElements is null)
                return finalSize;

            _isInLayout = true;

            try
            {
                var orientation = Orientation;
                var u = _realizedElements!.StartU;

                for (var i = 0; i < _realizedElements.Count; ++i)
                {
                    var e = _realizedElements.Elements[i];

                    if (e is not null)
                    {
                        var sizeU = _realizedElements.SizeU[i];
                        var rect = orientation == Orientation.Horizontal ?
                            new Rect(u, 0, sizeU, finalSize.Height) :
                            new Rect(0, u, finalSize.Width, sizeU);
                        rect = ArrangeElement(i + _realizedElements.FirstIndex, e, rect);
                        _scrollViewer?.RegisterAnchorCandidate(e);
                        u += orientation == Orientation.Horizontal ? rect.Width : rect.Height;
                    }
                }

                return finalSize;
            }
            finally
            {
                _isInLayout = false;
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _scrollViewer = this.FindAncestorOfType<ScrollViewer>();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _scrollViewer = null;
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);
            RecycleAllElements();
        }

        protected virtual void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
        {
            var vertical = Orientation == Orientation.Vertical;
            var oldViewportStart = vertical ? Viewport.Top : Viewport.Left;
            var oldViewportEnd = vertical ? Viewport.Bottom : Viewport.Right;

            // We sometimes get sent a viewport of 0,0 because the EffectiveViewportChanged event
            // is being raised when the parent control hasn't yet been arranged. This is a bug in
            // Avalonia, but we can work around it by forcing MeasureOverride to estimate the
            // viewport.
            Viewport = e.EffectiveViewport.Size == default ? 
                s_invalidViewport :
                e.EffectiveViewport.Intersect(new(Bounds.Size));

            _isWaitingForViewportUpdate = false;

            var newViewportStart = vertical ? Viewport.Top : Viewport.Left;
            var newViewportEnd = vertical ? Viewport.Bottom : Viewport.Right;

            if (!MathUtilities.AreClose(oldViewportStart, newViewportStart) ||
                !MathUtilities.AreClose(oldViewportEnd, newViewportEnd))
            {
                InvalidateMeasure();
            }
        }

        protected virtual void UnrealizeElementOnItemRemoved(Control element)
        {
            UnrealizeElement(element);
        }

        private void RealizeElements(
            IReadOnlyList<TItem> items,
            Size availableSize,
            ref MeasureViewport viewport)
        {
            Debug.Assert(_measureElements is not null);
            Debug.Assert(_realizedElements is not null);
            Debug.Assert(items.Count > 0);

            var index = viewport.anchorIndex;
            var horizontal = Orientation == Orientation.Horizontal;
            var u = viewport.anchorU;

            // If the anchor element is at the beginning of, or before, the start of the viewport
            // then we can recycle all elements before it.
            if (u <= viewport.anchorU)
                _realizedElements.RecycleElementsBefore(viewport.anchorIndex, _recycleElement);

            // Start at the anchor element and move forwards, realizing elements.
            do
            {
                var e = GetOrCreateElement(items, index);
                var constraint = GetInitialConstraint(e, index, availableSize);
                var slot = MeasureElement(index, e, constraint);

                var sizeU = horizontal ? slot.Width : slot.Height;
                var sizeV = horizontal ? slot.Height : slot.Width;

                _measureElements!.Add(index, e, u, sizeU);
                viewport.measuredV = Math.Max(viewport.measuredV, sizeV);

                u += sizeU;
                ++index;
            } while (u < viewport.viewportUEnd && index < items.Count);

            // Store the last index and end U position for the desired size calculation.
            viewport.lastIndex = index - 1;
            viewport.realizedEndU = u;

            // We can now recycle elements after the last element.
            _realizedElements.RecycleElementsAfter(viewport.lastIndex, _recycleElement);

            // Next move backwards from the anchor element, realizing elements.
            index = viewport.anchorIndex - 1;
            u = viewport.anchorU;

            while (u > viewport.viewportUStart && index >= 0)
            {
                var e = GetOrCreateElement(items, index);
                var constraint = GetInitialConstraint(e, index, availableSize);
                var slot = MeasureElement(index, e, constraint);

                var sizeU = horizontal ? slot.Width : slot.Height;
                var sizeV = horizontal ? slot.Height : slot.Width;
                u -= sizeU;

                _measureElements.Add(index, e, u, sizeU);
                viewport.measuredV = Math.Max(viewport.measuredV, sizeV);
                --index;
            }

            // We can now recycle elements before the first element.
            _realizedElements.RecycleElementsBefore(index + 1, _recycleElement);
        }

        private Size CalculateDesiredSize(Orientation orientation, int itemCount, in MeasureViewport viewport)
        {
            var sizeU = 0.0;
            var sizeV = viewport.measuredV;

            if (viewport.lastIndex >= 0)
            {
                var remaining = itemCount - viewport.lastIndex - 1;
                sizeU = viewport.realizedEndU + (remaining * EstimateElementSizeU());
            }

            return orientation == Orientation.Horizontal ? new(sizeU, sizeV) : new(sizeV, sizeU);
        }

        private MeasureViewport CalculateMeasureViewport(IReadOnlyList<TItem> items, Size availableSize)
        {
            Debug.Assert(_realizedElements is not null);

            // If the control has not yet been laid out then the effective viewport won't have been set.
            // Try to work it out from an ancestor control.
            var viewport = Viewport != s_invalidViewport ? Viewport : EstimateViewport(availableSize);

            // Get the viewport in the orientation direction.
            var viewportStart = Orientation == Orientation.Horizontal ? viewport.X : viewport.Y;
            var viewportEnd = Orientation == Orientation.Horizontal ? viewport.Right : viewport.Bottom;

            // Get or estimate the anchor element from which to start realization.
            var itemCount = items.Count;
            var (anchorIndex, anchorU) = _realizedElements.GetOrEstimateAnchorElementForViewport(
                viewportStart,
                viewportEnd,
                itemCount,
                ref _lastEstimatedElementSizeU);

            // Check if the anchor element is not within the currently realized elements.
            var disjunct = anchorIndex < _realizedElements.FirstIndex ||
                anchorIndex > _realizedElements.LastIndex;

            return new MeasureViewport
            {
                anchorIndex = anchorIndex,
                anchorU = anchorU,
                viewportUStart = viewportStart,
                viewportUEnd = viewportEnd,
                viewportIsDisjunct = disjunct,
            };
        }

        private Control GetOrCreateElement(IReadOnlyList<TItem> items, int index)
        {
            var e = GetRealizedElement(index) ??
                GetRealizedElement(index, ref _focusedIndex, ref _focusedElement) ??
                GetRealizedElement(index, ref _scrollToIndex, ref _scrollToElement) ??
                GetRecycledOrCreateElement(items, index);
            return e;
        }

        private Control? GetRealizedElement(int index)
        {
            return _realizedElements?.GetElement(index);
        }

        private static Control? GetRealizedElement(
            int index,
            ref int specialIndex,
            ref Control? specialElement)
        {
            if (specialIndex == index)
            {
                Debug.Assert(specialElement is not null);

                var result = specialElement;
                specialIndex = -1;
                specialElement = null;
                return result;
            }

            return null;
        }

        private Control GetRecycledOrCreateElement(IReadOnlyList<TItem> items, int index)
        {
            var item = items[index];
            var e = GetElementFromFactory(item, index);
            e.IsVisible = true;
            RealizeElement(e, item, index);
            if (e.GetVisualParent() is null)
            {
                ((ISetLogicalParent)e).SetParent(this);
                VisualChildren.Add(e);
            }
            return e;
        }

        private double EstimateElementSizeU()
        {
            if (_realizedElements is null)
                return _lastEstimatedElementSizeU;

            var result = _realizedElements.EstimateElementSizeU();
            if (result >= 0)
                _lastEstimatedElementSizeU = result;
            return _lastEstimatedElementSizeU;
        }

        private Rect EstimateViewport(Size availableSize)
        {
            var c = this.GetVisualParent();

            if (c is null)
            {
                return default;
            }

            while (c is not null)
            {
                if (!c.Bounds.Equals(default) && c.TransformToVisual(this) is Matrix transform)
                {
                    return new Rect(0, 0, c.Bounds.Width, c.Bounds.Height)
                        .TransformToAABB(transform)
                        .Intersect(new(0, 0, double.PositiveInfinity, double.PositiveInfinity));
                }

                c = c?.GetVisualParent();
            }

            return new Rect(
                0,
                0,
                double.IsFinite(availableSize.Width) ? availableSize.Width : 0,
                double.IsFinite(availableSize.Height) ? availableSize.Height : 0); 
        }

        private void RecycleElement(Control element, int index)
        {
            if (element.IsKeyboardFocusWithin)
            {
                _focusedElement = element;
                _focusedIndex = index;
                _focusedElement.LostFocus += OnUnrealizedFocusedElementLostFocus;
            }
            else
            {
                UnrealizeElement(element);
                element.IsVisible = false;
                ElementFactory!.RecycleElement(element);
                _scrollViewer?.UnregisterAnchorCandidate(element);
            }
        }

        private void RecycleElementOnItemRemoved(Control element)
        {
            UnrealizeElementOnItemRemoved(element);
            element.IsVisible = false;
            ElementFactory!.RecycleElement(element);
            _scrollViewer?.UnregisterAnchorCandidate(element);
        }

        private void TrimUnrealizedChildren()
        {
            var count = Items?.Count ?? 0;
            var children = VisualChildren;

            if (children.Count > count)
            {
                for (var i = children.Count - 1; i >= 0; --i)
                {
                    var child = children[i];

                    if (!child.IsVisible)
                    {
                        ((ISetLogicalParent)child).SetParent(null);
                        children.RemoveAt(i);
                    }
                }
            }
        }

        private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateMeasure();

            if (_realizedElements is null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _realizedElements.ItemsInserted(e.NewStartingIndex, e.NewItems!.Count, _updateElementIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _realizedElements.ItemsRemoved(e.OldStartingIndex, e.OldItems!.Count, _updateElementIndex, _recycleElementOnItemRemoved);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    _realizedElements.ItemsRemoved(e.OldStartingIndex, e.OldItems!.Count, _updateElementIndex, _recycleElementOnItemRemoved);
                    _realizedElements.ItemsInserted(e.NewStartingIndex, e.NewItems!.Count, _updateElementIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _realizedElements.ItemsReset(_recycleElementOnItemRemoved);
                    break;
            }
        }

        private void OnUnrealizedFocusedElementLostFocus(object? sender, RoutedEventArgs e)
        {
            if (_focusedElement is null || sender != _focusedElement)
                return;

            _focusedElement.LostFocus -= OnUnrealizedFocusedElementLostFocus;
            RecycleElement(_focusedElement, _focusedIndex);
            _focusedElement = null;
            _focusedIndex = -1;
        }

        private static bool HasInfinity(Size s) => double.IsInfinity(s.Width) || double.IsInfinity(s.Height);

        private struct MeasureViewport
        {
            public int anchorIndex;
            public double anchorU;
            public double viewportUStart;
            public double viewportUEnd;
            public double measuredV;
            public double realizedEndU;
            public int lastIndex;
            public bool viewportIsDisjunct;
        }
    }
}
