namespace Avalonia.Controls
{
    /// <summary>
    /// Represents a cell in a <see cref="TreeDataGrid"/>.
    /// </summary>
    /// <param name="ColumnIndex">
    /// The index of the cell in the <see cref="TreeDataGrid.Columns"/> collection.
    /// </param>
    /// <param name="RowIndex">
    /// The hierarchical index of the row model in the data source.
    /// </param>
    public readonly record struct CellIndex(int ColumnIndex, IndexPath RowIndex);
}
