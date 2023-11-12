using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Avalonia.Experimental.Data;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A column in an <see cref="HierarchicalTreeDataGridSource{TModel}"/> whose cells show an
    /// expander to reveal nested data.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class HierarchicalExpanderColumn<TModel> : NotifyingBase,
        IColumn<TModel>,
        IExpanderColumn<TModel>,
        IUpdateColumnLayout
            where TModel : class
    {
        private readonly IColumn<TModel> _inner;
        private readonly Func<TModel, IEnumerable<TModel>?> _childSelector;
        private readonly TypedBinding<TModel, bool>? _hasChildrenSelector;
        private readonly TypedBinding<TModel, bool>? _isExpandedBinding;
        private double _actualWidth = double.NaN;

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchicalExpanderColumn{TModel}"/> class.
        /// </summary>
        /// <param name="inner">The inner column which defines how the column will be displayed.</param>
        /// <param name="childSelector">The model children selector.</param>
        /// <param name="hasChildrenSelector">
        /// A selector which is used to determine whether the model has children without invoking
        /// <paramref name="childSelector"/>. This is only needed if the initialization of a node's
        /// children is an expensive operation; where that is not true, pass null.
        /// </param>
        /// <param name="isExpandedSelector">
        /// Selects a read/write boolean property which stores the expanded state for the row.
        /// </param>
        /// <param name="width">The column width.</param>
        public HierarchicalExpanderColumn(
            IColumn<TModel> inner,
            Func<TModel, IEnumerable<TModel>?> childSelector,
            Expression<Func<TModel, bool>>? hasChildrenSelector = null,
            Expression<Func<TModel, bool>>? isExpandedSelector = null)
        {
            _inner = inner;
            _inner.PropertyChanged += OnInnerPropertyChanged;
            _childSelector = childSelector;
            _hasChildrenSelector = hasChildrenSelector is not null ?
                TypedBinding<TModel>.OneWay(hasChildrenSelector) :
                null;
            _isExpandedBinding = isExpandedSelector is not null ?
                TypedBinding<TModel>.TwoWay(isExpandedSelector) :
                null;
            _actualWidth = inner.ActualWidth;
        }

        /// <summary>
        /// Gets or sets the actual width of the column after measurement.
        /// </summary>
        public double ActualWidth
        {
            get => _actualWidth;
            private set => RaiseAndSetIfChanged(ref _actualWidth, value);
        }

        public bool? CanUserResize => _inner.CanUserResize;
        public object? Header => _inner.Header;

        public ListSortDirection? SortDirection
        {
            get => _inner.SortDirection;
            set => _inner.SortDirection = value;
        }

        /// <summary>
        /// Gets or sets a user-defined object attached to the column.
        /// </summary>
        public object? Tag
        {
            get => _inner.Tag;
            set => _inner.Tag = value;
        }

        public GridLength Width => _inner.Width;
        public IColumn<TModel> Inner => _inner;
        double IUpdateColumnLayout.MinActualWidth => ((IUpdateColumnLayout)_inner).MinActualWidth;
        double IUpdateColumnLayout.MaxActualWidth => ((IUpdateColumnLayout)_inner).MaxActualWidth;
        bool IUpdateColumnLayout.StarWidthWasConstrained => ((IUpdateColumnLayout)_inner).StarWidthWasConstrained;

        public ICell CreateCell(IRow<TModel> row)
        {
            if (row is HierarchicalRow<TModel> r)
            {
                var showExpander = new ShowExpanderObservable<TModel>(
                    _childSelector,
                    _hasChildrenSelector,
                    r.Model);
                var isExpanded = _isExpandedBinding?.Instance(r.Model);
                return new ExpanderCell<TModel>(_inner.CreateCell(r), r, showExpander, isExpanded);
            }

            throw new NotSupportedException();
        }

        public bool HasChildren(TModel model)
        {
            return _hasChildrenSelector?.Read!(model) ?? _childSelector(model)?.Any() ?? false;
        }

        public IEnumerable<TModel>? GetChildModels(TModel model)
        {
            return _childSelector(model);
        }

        public Comparison<TModel?>? GetComparison(ListSortDirection direction)
        {
            return _inner.GetComparison(direction);
        }

        void IExpanderColumn<TModel>.SetModelIsExpanded(IExpanderRow<TModel> row)
        {
            _isExpandedBinding?.Write!.Invoke(row.Model, row.IsExpanded);
        }

        double IUpdateColumnLayout.CellMeasured(double width, int rowIndex)
        {
            return ((IUpdateColumnLayout)_inner).CellMeasured(width, rowIndex);
        }

        bool IUpdateColumnLayout.CommitActualWidth()
        {
            var result = ((IUpdateColumnLayout)_inner).CommitActualWidth();
            ActualWidth = _inner.ActualWidth;
            return result;
        }

        void IUpdateColumnLayout.CalculateStarWidth(double availableWidth, double totalStars)
        {
            ((IUpdateColumnLayout)_inner).CalculateStarWidth(availableWidth, totalStars);
            ActualWidth = _inner.ActualWidth;
        }

        void IUpdateColumnLayout.SetWidth(GridLength width) => SetWidth(width);

        private void OnInnerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CanUserResize) ||
                e.PropertyName == nameof(Header) ||
                e.PropertyName == nameof(SortDirection) ||
                e.PropertyName == nameof(Width))
                RaisePropertyChanged(e);
        }

        private void SetWidth(GridLength width)
        {
            ((IUpdateColumnLayout)_inner).SetWidth(width);

            if (width.IsAbsolute)
                ActualWidth = width.Value;
        }
    }
}
