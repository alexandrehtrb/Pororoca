namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a row from an integer indexed data source.
    /// </summary>
    public interface IModelIndexableRow : IRow
    {
        /// <summary>
        /// Gets the index of the model in its parent data source.
        /// </summary>
        int ModelIndex { get; }

        /// <summary>
        /// Gets the index of the model from the root data source.
        /// </summary>
        IndexPath ModelIndexPath { get; }
    }
}
