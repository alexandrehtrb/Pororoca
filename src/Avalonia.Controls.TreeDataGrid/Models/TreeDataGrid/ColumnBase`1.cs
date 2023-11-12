using System;
using System.ComponentModel;
using Avalonia.Utilities;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns which select cell values from a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class ColumnBase<TModel> : NotifyingBase, IColumn<TModel>, IUpdateColumnLayout
    {
        private double _actualWidth = double.NaN;
        private GridLength _width;
        private double _autoWidth = double.NaN;
        private double _starWidth = double.NaN;
        private bool _starWidthWasConstrained;
        private object? _header;
        private ListSortDirection? _sortDirection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase{TModel, TValue}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="width">
        /// The column width. If null defaults to <see cref="GridLength.Auto"/>.
        /// </param>
        /// <param name="options">Additional column options.</param>
        public ColumnBase(
            object? header,
            GridLength? width,
            ColumnOptions<TModel> options)
        {            
            _header = header;
            Options = options;
            SetWidth(width ?? GridLength.Auto);
        }

        /// <summary>
        /// Gets the actual width of the column after measurement.
        /// </summary>
        public double ActualWidth
        {
            get => _actualWidth;
            private set => RaiseAndSetIfChanged(ref _actualWidth, value);
        }

        /// <summary>
        /// Gets the width of the column.
        /// </summary>
        /// <remarks>
        /// To set the column width use <see cref="IColumns.SetColumnWidth(int, GridLength)"/>.
        /// </remarks>
        public GridLength Width 
        {
            get => _width;
            private set => RaiseAndSetIfChanged(ref _width, value);
        }

        /// <summary>
        /// Gets or sets the column header.
        /// </summary>
        public object? Header
        {
            get => _header;
            set => RaiseAndSetIfChanged(ref _header, value);
        }

        /// <summary>
        /// Gets the column options.
        /// </summary>
        public ColumnOptions<TModel> Options { get; }

        /// <summary>
        /// Gets or sets the sort direction indicator that will be displayed on the column.
        /// </summary>
        /// <remarks>
        /// Note that changing this property does not change the sorting of the data, it is only 
        /// used to display a sort direction indicator. To sort data according to a column use
        /// <see cref="ITreeDataGridSource.SortBy(IColumn, ListSortDirection)"/>.
        /// </remarks>
        public ListSortDirection? SortDirection
        {
            get => _sortDirection;
            set => RaiseAndSetIfChanged(ref _sortDirection, value);
        }

        /// <summary>
        /// Gets or sets a user-defined object attached to the column.
        /// </summary>
        public object? Tag { get; set; }

        bool? IColumn.CanUserResize => Options.CanUserResizeColumn;
        double IUpdateColumnLayout.MinActualWidth => CoerceActualWidth(0);
        double IUpdateColumnLayout.MaxActualWidth => CoerceActualWidth(double.PositiveInfinity);
        bool IUpdateColumnLayout.StarWidthWasConstrained => _starWidthWasConstrained;

        /// <summary>
        /// Creates a cell for this column on the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>The cell.</returns>
        public abstract ICell CreateCell(IRow<TModel> row);

        public abstract Comparison<TModel?>? GetComparison(ListSortDirection direction);

        double IUpdateColumnLayout.CellMeasured(double width, int rowIndex)
        {
            _autoWidth = Math.Max(NonNaN(_autoWidth), CoerceActualWidth(width));
            return Width.GridUnitType == GridUnitType.Auto || double.IsNaN(ActualWidth) ?
                _autoWidth : ActualWidth;
        }

        void IUpdateColumnLayout.CalculateStarWidth(double availableWidth, double totalStars)
        {
            if (!Width.IsStar)
                throw new InvalidOperationException("Attempt to calculate star width on a non-star column.");

            var width = (availableWidth / totalStars) * Width.Value;
            _starWidth = CoerceActualWidth(width);
            _starWidthWasConstrained = !MathUtilities.AreClose(_starWidth, width);
        }

        bool IUpdateColumnLayout.CommitActualWidth()
        {
            var width = Width.GridUnitType switch
            {
                GridUnitType.Auto => _autoWidth,
                GridUnitType.Pixel => CoerceActualWidth(Width.Value),
                GridUnitType.Star => _starWidth,
                _ => throw new NotSupportedException(),
            };

            var oldWidth = ActualWidth;
            ActualWidth = width;
            _starWidthWasConstrained = false;
            return !MathUtilities.AreClose(oldWidth, ActualWidth);
        }

        void IUpdateColumnLayout.SetWidth(GridLength width) => SetWidth(width);

        private double CoerceActualWidth(double width)
        {
            width = Options.MinWidth.GridUnitType switch
            {
                GridUnitType.Auto => Math.Max(width, _autoWidth),
                GridUnitType.Pixel => Math.Max(width, Options.MinWidth.Value),
                GridUnitType.Star => throw new NotImplementedException(),
                _ => width
            };

            return Options.MaxWidth?.GridUnitType switch
            {
                GridUnitType.Auto => Math.Min(width, _autoWidth),
                GridUnitType.Pixel => Math.Min(width, Options.MaxWidth.Value.Value),
                GridUnitType.Star => throw new NotImplementedException(),
                _ => width
            };
        }

        private void SetWidth(GridLength width)
        {
            _width = width;

            if (width.IsAbsolute)
                ActualWidth = width.Value;
        }

        private static double NonNaN(double v) => double.IsNaN(v) ? 0 : v;
    }
}
