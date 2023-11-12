using Avalonia.Media;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public interface ICellOptions
    {
        /// <summary>
        /// Gets the gesture(s) that will cause the cell to enter edit mode.
        /// </summary>
        BeginEditGestures BeginEditGestures { get; }
    }
}
