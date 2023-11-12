namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents an element which may expand.
    /// </summary>
    public interface IExpander
    {
        /// <summary>
        /// Gets or sets a value indicating whether the element is expanded.
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether expander should be shown.
        /// </summary>
        bool ShowExpander { get; }
    }
}
