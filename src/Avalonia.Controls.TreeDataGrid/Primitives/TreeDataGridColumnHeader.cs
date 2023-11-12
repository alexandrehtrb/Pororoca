using System;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;
using Avalonia.Utilities;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridColumnHeader : Button
    {
        public static readonly DirectProperty<TreeDataGridColumnHeader, bool> CanUserResizeProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, bool>(
                nameof(CanUserResize),
                x => x.CanUserResize);

        public static readonly DirectProperty<TreeDataGridColumnHeader, object?> HeaderProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, object?>(
                nameof(Header),
                o => o.Header);

        public static readonly DirectProperty<TreeDataGridColumnHeader, ListSortDirection?> SortDirectionProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, ListSortDirection?>(
                nameof(SortDirection),
                o => o.SortDirection);

        private bool _canUserResize;
        private IColumns? _columns;
        private object? _header;
        private IColumn? _model;
        private ListSortDirection? _sortDirection;
        private TreeDataGrid? _owner;
        private Thumb? _resizer;

        public bool CanUserResize
        {
            get => _canUserResize;
            private set => SetAndRaise(CanUserResizeProperty, ref _canUserResize, value);
        }

        public int ColumnIndex { get; private set; }

        public object? Header
        {
            get => _header;
            private set => SetAndRaise(HeaderProperty, ref _header, value);
        }

        public ListSortDirection? SortDirection
        {
            get => _sortDirection;
            private set => SetAndRaise(SortDirectionProperty, ref _sortDirection, value);
        }

        public void Realize(IColumns columns, int columnIndex)
        {
            if (_model is object)
                throw new InvalidOperationException("Column header is already realized.");

            _columns = columns;
            _model = columns[columnIndex];
            ColumnIndex = columnIndex;
            UpdatePropertiesFromModel();

            if (_model is INotifyPropertyChanged newInpc)
                newInpc.PropertyChanged += OnModelPropertyChanged;
        }

        public void Unrealize()
        {
            if (_model is INotifyPropertyChanged oldInpc)
                oldInpc.PropertyChanged -= OnModelPropertyChanged;

            _columns = null;
            _model = null;
            ColumnIndex = -1;
            UpdatePropertiesFromModel();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _resizer = e.NameScope.Find<Thumb>("PART_Resizer");

            if (_resizer is not null)
            {
                _resizer.DragDelta += ResizerDragDelta;
                _resizer.DoubleTapped += ResizerDoubleTapped;
            }
        }

        private void ResizerDoubleTapped(object? sender, Interactivity.RoutedEventArgs e)
        {
            _columns?.SetColumnWidth(ColumnIndex, GridLength.Auto);
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

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == CanUserResizeProperty)
            {
                PseudoClasses.Set(":resizable", change.GetNewValue<bool>());
            }
            else if (change.Property == DataContextProperty)
            {
                var oldModel = change.GetOldValue<object?>() as IColumn;
                var newModel = change.GetNewValue<object?>() as IColumn;

                if (oldModel is INotifyPropertyChanged oldInpc)
                    oldInpc.PropertyChanged -= OnModelPropertyChanged;
                if (newModel is INotifyPropertyChanged newInpc)
                    newInpc.PropertyChanged += OnModelPropertyChanged;

                UpdatePropertiesFromModel();
            }
            else if (change.Property == ParentProperty)
            {
                if (_owner is not null)
                    _owner.PropertyChanged -= OnOwnerPropertyChanged;
                _owner = change.GetNewValue<StyledElement>()?.TemplatedParent as TreeDataGrid;
                if (_owner is not null)
                    _owner.PropertyChanged += OnOwnerPropertyChanged;
                UpdatePropertiesFromModel();
            }

            base.OnPropertyChanged(change);
        }

        private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IColumn.CanUserResize) ||
                e.PropertyName == nameof(IColumn.Header) ||
                e.PropertyName == nameof(IColumn.SortDirection))
                UpdatePropertiesFromModel();
        }

        private void OnOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_owner is null)
                return;
            if (e.Property == TreeDataGrid.CanUserResizeColumnsProperty)
                CanUserResize = _model?.CanUserResize ?? _owner.CanUserResizeColumns;
        }

        private void ResizerDragDelta(object? sender, VectorEventArgs e)
        {
            if (_columns is null || _model is null || MathUtilities.IsZero(e.Vector.X))
                return;

            var pixelWidth = _model.Width.IsAbsolute ? _model.Width.Value : Bounds.Width;

            if (double.IsNaN(pixelWidth) || double.IsInfinity(pixelWidth) || pixelWidth + e.Vector.X < 0)
                return;

            var width = new GridLength(pixelWidth + e.Vector.X, GridUnitType.Pixel);
            _columns.SetColumnWidth(ColumnIndex, width);
        }

        private void UpdatePropertiesFromModel()
        {
            CanUserResize = _model?.CanUserResize ?? _owner?.CanUserResizeColumns ?? false;
            Header = _model?.Header;
            SortDirection = _model?.SortDirection;
        }
    }
}
