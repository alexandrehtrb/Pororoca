using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Selection
{
    public class TreeDataGridColumnSelectionModel : SelectionModel<IColumn>,
        ITreeDataGridColumnSelectionModel
    {
        public TreeDataGridColumnSelectionModel(IColumns columns)
            : base(columns)
        {
        }
    }
}
