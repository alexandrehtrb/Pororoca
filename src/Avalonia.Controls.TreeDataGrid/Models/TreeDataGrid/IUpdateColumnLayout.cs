namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Internal low-level interface for layout interactions between <see cref="IColumns"/> and
    /// <see cref="IColumn"/>.
    /// </summary>
    public interface IUpdateColumnLayout : IColumn
    {
        /// <summary>
        /// Gets the minimum actual width of the column.
        /// </summary>
        /// <returns>
        /// The minimum width of the column in pixels.
        /// </returns>
        double MinActualWidth { get; }

        /// <summary>
        /// Gets the maximum actual width of the column.
        /// </summary>
        /// <returns>
        /// The maximum width of the column in pixels, or <see cref="double.PositiveInfinity"/> if the
        /// column has no maximum width.
        /// </returns>
        double MaxActualWidth { get; }

        /// <summary>
        /// Gets a value indicating whether the column is a star-width column and its width
        /// was constrained by its min/max width in the last call to
        /// <see cref="CalculateStarWidth(double, double))"/>.
        /// </summary>
        bool StarWidthWasConstrained { get; }

        /// <summary>
        /// Notifies the column that a cell has been measured.
        /// </summary>
        /// <param name="width">The measured width, in pixels; as returned by the cell DesiredSize.</param>
        /// <param name="rowIndex">The cell row index or -1 for a column header.</param>
        /// <returns>
        /// The width of the cell updated with the column width.
        /// </returns>
        double CellMeasured(double width, int rowIndex);

        /// <summary>
        /// Requests a star-width column to calculate its <see cref="IColumn.ActualWidth"/>.
        /// </summary>
        /// <param name="availableWidth">
        /// The available width to be shared by star-width columns.
        /// </param>
        /// <param name="totalStars">The sum of the star units of all columns.</param>
        /// <remarks>
        /// This method may be called multiple times during a layout pass; the width calculated by
        /// this method should not be comitted to <see cref="IColumn.ActualWidth"/> until
        /// <see cref="CommitActualWidth"/> is called. This method should update the value of the
        /// <see cref="StarWidthWasConstrained"/> property.
        /// </remarks>
        void CalculateStarWidth(double availableWidth, double totalStars);

        /// <summary>
        /// Requests a column to set its final <see cref="IColumn.ActualWidth"/> based on the
        /// size calculated via <see cref="CellMeasured(double, int)"/> and
        /// <see cref="CalculateStarWidth(double, double)"/>.
        /// </summary>
        /// <returns>
        /// True if the column's actual width has changed; otherwise false.
        /// </returns>
        bool CommitActualWidth();

        /// <summary>
        /// Notifies the column of a change to its preferred width.
        /// </summary>
        /// <param name="width">The width.</param>
        void SetWidth(GridLength width);
    }
}
