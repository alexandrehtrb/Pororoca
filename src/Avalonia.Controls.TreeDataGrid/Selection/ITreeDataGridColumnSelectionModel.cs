using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Selection
{
    public interface ITreeDataGridColumnSelectionModel : ISelectionModel
    {
        new IReadOnlyList<IColumn?> SelectedItems { get; }
        new IColumn? SelectedItem { get; set; }
    }
}
