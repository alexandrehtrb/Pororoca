using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Avalonia.Controls
{
    /// <summary>
    /// Represents a data source for a <see cref="TreeDataGrid"/> control.
    /// </summary>
    public interface ITreeDataGridSource : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the columns to be displayed.
        /// </summary>
        IColumns Columns { get; }

        /// <summary>
        /// Gets the rows to be displayed.
        /// </summary>
        IRows Rows { get; }

        /// <summary>
        /// Gets the selection model.
        /// </summary>
        ITreeDataGridSelection? Selection { get; }

        /// <summary>
        /// Gets a value indicating whether the data source is hierarchical.
        /// </summary>
        bool IsHierarchical { get; }

        /// <summary>
        /// Gets a value indicating whether the data source is currently sorted.
        /// </summary>
        bool IsSorted { get; }

        /// <summary>
        /// Event which would be triggered after SortBy method execution.
        /// </summary>
        event Action Sorted;

        /// <summary>
        /// Executes a row drag/drop operation.
        /// </summary>
        /// <param name="source">The source of the dragged rows.</param>
        /// <param name="indexes">The model indexes of the rows being dragged.</param>
        /// <param name="targetIndex">The model index of the drop target.</param>
        /// <param name="position">The position relative to the drop target.</param>
        /// <param name="effects">The requested drop effects.</param>
        void DragDropRows(
            ITreeDataGridSource source,
            IEnumerable<IndexPath> indexes,
            IndexPath targetIndex,
            TreeDataGridRowDropPosition position,
            DragDropEffects effects);

        /// <summary>
        /// Gets the items in the data source.
        /// </summary>
        IEnumerable<object> Items { get; }

        /// <summary>
        /// Gets the children of a model, if any.
        /// </summary>
        /// <param name="model">The model from which to get the children.</param>
        /// <returns>The children of the model. If there are no children, it will return an empty enumerable.</returns>
        IEnumerable<object>? GetModelChildren(object model);

        /// <summary>
        /// Requests to sort the source by the specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="direction">The sort direction.</param>
        /// <returns>True if the sort could be performed; otherwise false.</returns>
        bool SortBy(IColumn column, ListSortDirection direction);
    }

    /// <summary>
    /// Represents a typed data source for a <see cref="TreeDataGrid"/> control.
    /// </summary>
    public interface ITreeDataGridSource<TModel> : ITreeDataGridSource
    {
        /// <summary>
        /// Gets the items in the data source.
        /// </summary>
        new IEnumerable<TModel> Items { get; }
    }
}
