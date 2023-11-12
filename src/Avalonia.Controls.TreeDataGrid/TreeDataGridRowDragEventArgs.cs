using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalonia.Controls
{
    public enum TreeDataGridRowDropPosition
    {
        None,
        Before,
        After,
        Inside,
    }

    /// <summary>
    /// Provides data for the <see cref="TreeDataGrid.RowDragOver"/> and
    /// <see cref="TreeDataGrid.RowDrop"/> events.
    /// </summary>
    public class TreeDataGridRowDragEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeDataGridRowDragEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The event being raised.</param>
        /// <param name="row">The row that is being dragged over.</param>
        /// <param name="inner">The inner drag event args.</param>
        public TreeDataGridRowDragEventArgs(RoutedEvent routedEvent, TreeDataGridRow row, DragEventArgs inner)
            : base(routedEvent)
        {
            TargetRow = row;
            Inner = inner;
        }

        /// <summary>
        /// Gets the <see cref="DragEventArgs"/> that describes the drag/drop operation.
        /// </summary>
        public DragEventArgs Inner { get; }

        /// <summary>
        /// Gets the row being dragged over.
        /// </summary>
        public TreeDataGridRow TargetRow { get; }

        /// <summary>
        /// Gets or sets a value indicating the how the data should be dropped into
        /// the <see cref="TargetRow"/>.
        /// </summary>
        /// <remarks>
        /// For drag operations, the value of this property controls the adorner displayed when
        /// dragging. For drop operations, controls the final location of the drop.
        /// </remarks>
        public TreeDataGridRowDropPosition Position { get; set; }
    }
}
