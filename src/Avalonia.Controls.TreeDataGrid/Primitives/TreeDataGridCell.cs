using System;
using System.ComponentModel;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Primitives
{
    [PseudoClasses(":editing")]
    public abstract class TreeDataGridCell : TemplatedControl, ITreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridCell, bool> IsSelectedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCell, bool>(
                nameof(IsSelected),
                o => o.IsSelected);

        private static readonly Point s_invalidPoint = new Point(double.NaN, double.NaN);
        private bool _isSelected;
        private TreeDataGrid? _treeDataGrid;
        private Point _pressedPoint = s_invalidPoint;

        static TreeDataGridCell()
        {
            FocusableProperty.OverrideDefaultValue<TreeDataGridCell>(true);
            DoubleTappedEvent.AddClassHandler<TreeDataGridCell>((x, e) => x.OnDoubleTapped(e));
        }

        public int ColumnIndex { get; private set; } = -1;
        public int RowIndex { get; private set; } = -1;
        public bool IsEditing { get; private set; }
        public ICell? Model { get; private set; }

        public bool IsSelected
        {
            get => _isSelected;
            private set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
        }

        public bool IsEffectivelySelected
        {
            get => IsSelected || this.FindAncestorOfType<TreeDataGridRow>()?.IsSelected == true;
        }

        public virtual void Realize(
            TreeDataGridElementFactory factory,
            ITreeDataGridSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            if (ColumnIndex >= 0 || RowIndex >= 0)
                throw new InvalidOperationException("Cell is already realized.");
            if (columnIndex < 0)
                throw new IndexOutOfRangeException("Invalid column index.");
            if (rowIndex < 0)
                throw new IndexOutOfRangeException("Invalid row index.");

            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
            Model = model;
            IsSelected = selection?.IsCellSelected(columnIndex, rowIndex) ?? false;

            _treeDataGrid?.RaiseCellPrepared(this, columnIndex, RowIndex);
        }

        public virtual void Unrealize()
        {
            _treeDataGrid?.RaiseCellClearing(this, ColumnIndex, RowIndex);
            ColumnIndex = RowIndex = -1;
            Model = null;
        }

        protected void BeginEdit()
        {
            if (!IsEditing)
            {
                IsEditing = true;
                (Model as IEditableObject)?.BeginEdit();
                PseudoClasses.Add(":editing");
            }
        }

        protected void CancelEdit()
        {
            if (EndEditCore() && Model is IEditableObject editable)
                editable.CancelEdit();
        }

        protected void EndEdit()
        {
            if (EndEditCore() && Model is IEditableObject editable)
                editable.EndEdit();
        }

        protected void SubscribeToModelChanges()
        {
            if (Model is INotifyPropertyChanged inpc)
                inpc.PropertyChanged += OnModelPropertyChanged;
        }

        protected void UnsubscribeFromModelChanges()
        {
            if (Model is INotifyPropertyChanged inpc)
                inpc.PropertyChanged -= OnModelPropertyChanged;
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _treeDataGrid = this.FindLogicalAncestorOfType<TreeDataGrid>();
            base.OnAttachedToLogicalTree(e);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _treeDataGrid = null;
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            // The cell may be realized before being parented. In this case raise the CellPrepared event here.
            if (_treeDataGrid is not null && ColumnIndex >= 0 && RowIndex >= 0)
                _treeDataGrid.RaiseCellPrepared(this, ColumnIndex, RowIndex);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (!IsKeyboardFocusWithin && IsEditing)
                EndEdit();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var result = base.MeasureOverride(availableSize);

            // HACKFIX for #83. Seems that cells are getting truncated at times due to DPI scaling.
            // New text stack in Avalonia 11.0 should fix this but until then a hack to add a pixel
            // to cell size seems to fix it.
            result = result.Inflate(new Thickness(1, 0));

            return result;
        }

        protected virtual void OnDoubleTapped(TappedEventArgs e)
        {
            if (Model is not null &&
                !e.Handled &&
                !IsEditing &&
                Model.CanEdit &&
                IsEnabledEditGesture(BeginEditGestures.DoubleTap, Model.EditGestures))
            {
                BeginEdit();
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Model is null || e.Handled)
                return;

            if (e.Key == Key.F2 && 
                !IsEditing && 
                Model.CanEdit &&
                IsEnabledEditGesture(BeginEditGestures.F2, Model.EditGestures))
            {
                BeginEdit();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && IsEditing)
            {
                EndEdit();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && IsEditing)
            {
                CancelEdit();
                e.Handled = true;
            }
        }

        protected virtual void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (Model is not null &&
                !e.Handled &&
                !IsEditing &&
                Model.CanEdit &&
                IsEnabledEditGesture(BeginEditGestures.Tap, Model.EditGestures))
            {
                _pressedPoint = e.GetCurrentPoint(null).Position;
                e.Handled = true;
            }
            else
            {
                _pressedPoint = s_invalidPoint;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (Model is not null &&
                !e.Handled &&
                !IsEditing &&
                !double.IsNaN(_pressedPoint.X) &&
                Model.CanEdit &&
                IsEnabledEditGesture(BeginEditGestures.Tap, Model.EditGestures))
            {
                var point = e.GetCurrentPoint(this);
                var settings = TopLevel.GetTopLevel(this)?.PlatformSettings;
                var tapSize = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);
                var tapRect = new Rect(_pressedPoint, new Size())
                       .Inflate(new Thickness(tapSize.Width, tapSize.Height));

                if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                    tapRect.ContainsExclusive(e.GetCurrentPoint(null).Position))
                {
                    BeginEdit();
                    e.Handled = true;
                }
            }

            _pressedPoint = s_invalidPoint;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsSelectedProperty)
            {
                PseudoClasses.Set(":selected", IsSelected);
            }

            base.OnPropertyChanged(change);
        }

        internal void UpdateRowIndex(int index)
        {
            if (RowIndex == -1)
                throw new InvalidOperationException("Cell is not realized.");
            RowIndex = index;
        }

        internal void UpdateSelection(ITreeDataGridSelectionInteraction? selection)
        {
            IsSelected = selection?.IsCellSelected(ColumnIndex, RowIndex) ?? false;
        }

        private bool EndEditCore()
        {
            if (IsEditing)
            {
                var restoreFocus = IsKeyboardFocusWithin;
                IsEditing = false;
                PseudoClasses.Remove(":editing");
                if (restoreFocus)
                    Focus();
                return true;
            }

            return false;
        }

        private bool IsEnabledEditGesture(BeginEditGestures gesture, BeginEditGestures enabledGestures)
        {
            if (!enabledGestures.HasFlag(gesture))
                return false;

            return enabledGestures.HasFlag(BeginEditGestures.WhenSelected) ?
                IsEffectivelySelected : true;
        }
    }
}
