using System;

namespace Avalonia.Controls
{
    public class TreeDataGridCellEventArgs
    {
        public TreeDataGridCellEventArgs(Control cell, int columnIndex, int rowIndex)
        {
            Cell = cell;
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
        }

        internal TreeDataGridCellEventArgs()
        {
            Cell = null!;
        }

        public Control Cell { get; private set; }
        public int ColumnIndex { get; private set; }
        public int RowIndex { get; private set; }

        internal void Update(Control? cell, int columnIndex, int rowIndex)
        {
            if (cell is object && Cell is object)
                throw new NotSupportedException("Nested TreeDataGrid cell prepared/clearing detected.");

            Cell = cell!;
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
        }
    }
}
