using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;

namespace Avalonia.Controls.Primitives
{
    internal interface ITreeDataGridCell
    {
        int ColumnIndex { get; }

        void Realize(
            TreeDataGridElementFactory factory,
            ITreeDataGridSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex);

        void Unrealize();
    }
}
