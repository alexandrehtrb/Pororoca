using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a collection of rows in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    /// <remarks>
    /// Note that items retrieved from an <see cref="IRows"/> collection may be reused, so the
    /// <see cref="IRow"/> should be treated as valid only until the next item is retrieved from
    /// the collection.
    /// </remarks>
    public interface IRows : IReadOnlyList<IRow>, INotifyCollectionChanged
    {
        /// <summary>
        /// Gets the index and Y position of the row at the specified Y position, if it can be
        /// calculated.
        /// </summary>
        /// <param name="y">The Y position</param>
        /// <returns>
        /// A tuple containing the row index and Y position of the row, or (-1,-1) if the row
        /// could not be calculated.
        /// </returns>
        (int index, double y) GetRowAt(double y);

        /// <summary>
        /// Given a model index, returns an index into <see cref="Rows"/>.
        /// </summary>
        /// <param name="modelIndex">The model index.</param>
        /// <returns>The row index, or -1 if the model index is not displayed.</returns>
        int ModelIndexToRowIndex(IndexPath modelIndex);

        /// <summary>
        /// Given a row index, returns a model index.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <returns>The row index, or an empty path if the row index is not valid.</returns>
        IndexPath RowIndexToModelIndex(int rowIndex);

        /// <summary>
        /// Realizes a cell model for display on-screen.
        /// </summary>
        /// <param name="column">The cell's column.</param>
        /// <param name="columnIndex">The index of the cell's column.</param>
        /// <param name="rowIndex">The index of the cell's row.</param>
        ICell RealizeCell(IColumn column, int columnIndex, int rowIndex);

        /// <summary>
        /// Unrealizes a cell model realized with <see cref="RealizeCell(IColumn, int, int)"/>.
        /// </summary>
        /// <param name="cell">The cell model.</param>
        /// <param name="columnIndex">The index of the cell's column.</param>
        /// <param name="rowIndex">The index of the cell's row.</param>
        void UnrealizeCell(ICell cell, int columnIndex, int rowIndex);
    }
}
