using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Defines a column whose cells show an expander to reveal nested data.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IExpanderColumn<TModel> : IColumn<TModel>
    {
        /// <summary>
        /// Gets a value indicating whether the column has nested data.
        /// </summary>
        /// <param name="model">The parent model.</param>
        bool HasChildren(TModel model);

        /// <summary>
        /// Gets the child models which represent the nested data for this column.
        /// </summary>
        /// <param name="model">The parent model.</param>
        /// <returns>The child models if available.</returns>
        IEnumerable<TModel>? GetChildModels(TModel model);

        /// <summary>
        /// Called by an <see cref="IExpanderRow{TModel}"/> to write its 
        /// <see cref="IExpander.IsExpanded"/> state to the underlying model.
        /// </summary>
        /// <param name="row">The row.</param>
        void SetModelIsExpanded(IExpanderRow<TModel> row);
    }
}
