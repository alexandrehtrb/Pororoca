using Avalonia.Media;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a text cell in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    public interface ITextCell : ICell
    {
        /// <summary>
        /// Gets or sets the cell's value as a string.
        /// </summary>
        string? Text { get; set; }

        /// <summary>
        /// Gets the cell's text trimming mode.
        /// </summary>
        TextTrimming TextTrimming { get; }

        /// <summary>
        /// Gets the cell's text wrapping mode.
        /// </summary>
        TextWrapping TextWrapping { get; }
        
        /// Gets the cell's text alignment mode.
        /// </summary>
        TextAlignment TextAlignment { get; }
    }
}
