namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a row in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    public interface IRow
    {
        /// <summary>
        /// Gets the row header.
        /// </summary>
        object? Header { get; }

        /// <summary>
        /// Gets the height of the row.
        /// </summary>
        GridLength Height { get; set; }

        /// <summary>
        /// Gets the row model.
        /// </summary>
        object? Model { get; }
    }
}
