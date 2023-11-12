using System;
using System.Linq.Expressions;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A column in an <see cref="ITreeDataGridSource"/> which displays its values as text.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TValue">The column data type.</typeparam>
    public class TextColumn<TModel, TValue> : ColumnBase<TModel, TValue>, ITextSearchableColumn<TModel>
        where TModel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextColumn{TModel, TValue}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="getter">
        /// An expression which given a row model, returns a cell value for the column.
        /// </param>
        /// <param name="width">
        /// The column width. If null defaults to <see cref="GridLength.Auto"/>.
        /// </param>
        /// <param name="options">Additional column options.</param>
        public TextColumn(
            object? header,
            Expression<Func<TModel, TValue?>> getter,
            GridLength? width = null,
            TextColumnOptions<TModel>? options = null)
            : base(header, getter, null, width, options ?? new())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextColumn{TModel, TValue}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="getter">
        /// An expression which given a row model, returns a cell value for the column.
        /// </param>
        /// <param name="setter">
        /// A method which given a row model and a cell value, writes the cell value to the
        /// row model.
        /// </param>
        /// <param name="width">
        /// The column width. If null defaults to <see cref="GridLength.Auto"/>.
        /// </param>
        /// <param name="options">Additional column options.</param>
        public TextColumn(
            object? header,
            Expression<Func<TModel, TValue?>> getter,
            Action<TModel, TValue?> setter,
            GridLength? width = null,
            TextColumnOptions<TModel>? options = null)
            : base(header, getter, setter, width, options ?? new())
        {
        }

        public new TextColumnOptions<TModel> Options => (TextColumnOptions<TModel>)base.Options;

        bool ITextSearchableColumn<TModel>.IsTextSearchEnabled => Options?.IsTextSearchEnabled ?? false;

        public override ICell CreateCell(IRow<TModel> row)
        {
            return new TextCell<TValue?>(CreateBindingExpression(row.Model), Binding.Write is null, Options);
        }

        string? ITextSearchableColumn<TModel>.SelectValue(TModel model)
        {
            return ValueSelector(model)?.ToString();
        }
    }
}
