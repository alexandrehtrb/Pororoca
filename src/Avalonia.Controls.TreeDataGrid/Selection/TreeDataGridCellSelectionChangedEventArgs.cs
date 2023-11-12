using System;

namespace Avalonia.Controls.Selection
{
    /// <summary>
    /// Provides data for the <see cref="ITreeDataGridCellSelectionModel.SelectionChanged"/> event.
    /// </summary>
    public class TreeDataGridCellSelectionChangedEventArgs : EventArgs
    {
    }

    /// <summary>
    /// Provides data for the <see cref="ITreeDataGridCellSelectionModel{T}.SelectionChanged"/> event.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public class TreeDataGridCellSelectionChangedEventArgs<T> : TreeDataGridCellSelectionChangedEventArgs
        where T : class
    {
    }
}
