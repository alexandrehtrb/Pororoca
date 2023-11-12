using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a controller which receives notifications about an
    /// <see cref="IExpanderRow{TModel}"/>'s state.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IExpanderRowController<TModel>
    {
        /// <summary>
        /// Method called by a row before it starts expanding or collapsing.
        /// </summary>
        /// <param name="row">The row.</param>
        void OnBeginExpandCollapse(IExpanderRow<TModel> row);

        /// <summary>
        /// Method called by a row when it finishes expanding or collapsing.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="oldValue">
        /// The old value for <see cref="IExpander.IsExpanded"/>
        /// </param>
        /// <remarks>
        /// This method should always be called, even if expanding the row failed due to there
        /// being no children.
        /// </remarks>
        void OnEndExpandCollapse(IExpanderRow<TModel> row);

        /// <summary>
        /// Method called by a row when its children change.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="e"></param>
        void OnChildCollectionChanged(IExpanderRow<TModel> row, NotifyCollectionChangedEventArgs e);
    }
}
