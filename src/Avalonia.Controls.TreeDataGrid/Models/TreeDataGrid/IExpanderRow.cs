using System.ComponentModel;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a row which can be expanded to reveal nested data.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IExpanderRow<TModel> : IRow<TModel>, IExpander, INotifyPropertyChanged
    {
        /// <summary>
        /// Called by an <see cref="IExpanderCell"/> when it receives a notification that the
        /// row's <see cref="IExpander.ShowExpander"/> state should be changed.
        /// </summary>
        /// <param name="cell">The cell whose property has changed.</param>
        /// <param name="value">The new value.</param>
        void UpdateShowExpander(IExpanderCell cell, bool value);
    }
}
