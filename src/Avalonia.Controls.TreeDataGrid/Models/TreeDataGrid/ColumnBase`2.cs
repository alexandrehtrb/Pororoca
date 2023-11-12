using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using Avalonia.Experimental.Data;
using Avalonia.Experimental.Data.Core;
using Avalonia.Reactive;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns which select cell values from a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TModel">The value type.</typeparam>
    public abstract class ColumnBase<TModel, TValue> : ColumnBase<TModel>
        where TModel : class
    {
        private readonly bool _canUserSort;
        private readonly Comparison<TModel?>? _sortAscending;
        private readonly Comparison<TModel?>? _sortDescending;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase{TModel, TValue}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="getter">
        /// An expression which given a row model, returns a cell value for the column.
        /// </param>
        /// <param name="setter">
        /// A method which given a row model and a cell value, writes the cell value to the
        /// row model. If null, the column will be read-only.
        /// </param>
        /// <param name="width">
        /// The column width. If null defaults to <see cref="GridLength.Auto"/>.
        /// </param>
        /// <param name="options">Additional column options.</param>
        public ColumnBase(
            object? header,
            Expression<Func<TModel, TValue?>> getter,
            Action<TModel, TValue?>? setter,
            GridLength? width,
            ColumnOptions<TModel> options)
            : base(header, width, options)
        {
            ValueSelector = getter.Compile();
            Binding = setter is null ? 
                TypedBinding<TModel>.OneWay(getter) :
                TypedBinding<TModel>.TwoWay(getter, setter);
            _canUserSort = options?.CanUserSortColumn ?? true;
            _sortAscending = options?.CompareAscending ?? DefaultSortAscending;
            _sortDescending = options?.CompareDescending ?? DefaultSortDescending;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase{TModel, TValue}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="valueSelector">
        /// the function which selects the column value from the model..
        /// </param>
        /// <param name="binding">
        /// a binding which selects the column value from the model.
        /// </param>
        /// <param name="width">
        /// The column width. If null defaults to <see cref="GridLength.Auto"/>.
        /// </param>
        /// <param name="options">Additional column options.</param>
        public ColumnBase(
            object? header,
            Func<TModel, TValue?> valueSelector,
            TypedBinding<TModel, TValue?> binding,
            GridLength? width,
            ColumnOptions<TModel>? options)
            : base(header, width, options ?? new())
        {
            ValueSelector = valueSelector;
            Binding = binding;
            _canUserSort = options?.CanUserSortColumn ?? true;
            _sortAscending = options?.CompareAscending ?? DefaultSortAscending;
            _sortDescending = options?.CompareDescending ?? DefaultSortDescending;
        }

        /// <summary>
        /// Gets the function which selects the column value from the model.
        /// </summary>
        public Func<TModel, TValue?> ValueSelector { get; }

        /// <summary>
        /// Gets a binding which selects the column value from the model.
        /// </summary>
        public TypedBinding<TModel, TValue?> Binding { get; }

        public override Comparison<TModel?>? GetComparison(ListSortDirection direction)
        {
            if (!_canUserSort)
                return null;
            
            return direction switch
            {
                ListSortDirection.Ascending => _sortAscending,
                ListSortDirection.Descending => _sortDescending,
                _ => null,
            };
        }

        protected TypedBindingExpression<TModel, TValue?> CreateBindingExpression(TModel model)
        {
            return Binding.Instance(model);
        }

        private int DefaultSortAscending(TModel? x, TModel? y)
        {
            if (x is null || y is null)
                return Comparer<TModel>.Default.Compare(x, y);
            var a = ValueSelector(x);
            var b = ValueSelector(y);
            return Comparer<TValue>.Default.Compare(a, b);
        }

        private int DefaultSortDescending(TModel? x, TModel? y)
        {
            if (x is null || y is null)
                return -Comparer<TModel>.Default.Compare(x, y);
            var a = ValueSelector(x);
            var b = ValueSelector(y);
            return Comparer<TValue>.Default.Compare(b, a);
        }
    }
}
