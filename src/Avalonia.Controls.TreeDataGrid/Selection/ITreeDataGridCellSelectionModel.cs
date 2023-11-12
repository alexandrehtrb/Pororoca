using System;
using System.Collections.Generic;

namespace Avalonia.Controls.Selection
{
    /// <summary>
    /// Maintains the cell selection state for an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    public interface ITreeDataGridCellSelectionModel : ITreeDataGridSelection
    {
        /// <summary>
        /// Occurs when the cell selection changes.
        /// </summary>
        event EventHandler<TreeDataGridCellSelectionChangedEventArgs>? SelectionChanged;
    }

    /// <summary>
    /// Maintains the cell selection state for an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    public interface ITreeDataGridCellSelectionModel<T> : ITreeDataGridCellSelectionModel
        where T : class
    {
        /// <summary>
        /// Gets or sets a value indicating whether only a single cell can be selected at a time.
        /// </summary>
        bool SingleSelect { get; set; }

        /// <summary>
        /// Gets or sets the index of the currently selected cell.
        /// </summary>
        CellIndex SelectedIndex { get; set; }

        /// <summary>
        /// Gets the indexes of the currently selected cells.
        /// </summary>
        IReadOnlyList<CellIndex> SelectedIndexes { get; }

        /// <summary>
        /// Occurs when the cell selection changes.
        /// </summary>
        new event EventHandler<TreeDataGridCellSelectionChangedEventArgs<T>>? SelectionChanged;

        /// <summary>
        /// Checks whether the specified cell is selected.
        /// </summary>
        /// <param name="index">The index of the cell.</param>
        bool IsSelected(CellIndex index);

        /// <summary>
        /// Sets the current selection to the specified range of cells.
        /// </summary>
        /// <param name="start">The index of the cell from which the selection should start.</param>
        /// <param name="columnCount">The number of columns in the selection.</param>
        /// <param name="rowCount">The number of rows in the selection.</param>
        /// <remarks>
        /// This method clears the current selection and selects the specified range of cells.
        /// Note that if the <see cref="ITreeDataGridSource"/> is currently sorted then the
        /// resulting selection may not be contiguous in the data source.
        /// </remarks>
        void SetSelectedRange(CellIndex start, int columnCount, int rowCount);
    }
}
