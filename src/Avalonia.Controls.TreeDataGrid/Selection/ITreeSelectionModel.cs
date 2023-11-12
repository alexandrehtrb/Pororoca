using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Avalonia.Controls.Selection
{
    /// <summary>
    /// Stores and manipulates the selection state of of items in a hierarchical data source.
    /// </summary>
    public interface ITreeSelectionModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the root of the hierarchical data.
        /// </summary>
        IEnumerable? Source { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the model supports single or multiple selection.
        /// </summary>
        bool SingleSelect { get; set; }

        /// <summary>
        /// Gets or sets the selected index within the data.
        /// </summary>
        /// <remarks>
        /// In the case of multiple selected items, returns the index of the item that was first
        /// selected.
        /// </remarks>
        IndexPath SelectedIndex { get; set; }

        /// <summary>
        /// Gets the indexes of the selected items.
        /// </summary>
        IReadOnlyList<IndexPath> SelectedIndexes { get; }

        /// <summary>
        /// Gets the selected item.
        /// </summary>
        /// <remarks>
        /// In the case of multiple selected items, returns the item that was first selected.
        /// </remarks>
        object? SelectedItem { get; }

        /// <summary>
        /// Gets the selected items.
        /// </summary>
        IReadOnlyList<object?> SelectedItems { get; }

        /// <summary>
        /// Gets or sets the anchor index.
        /// </summary>
        /// <remarks>
        /// The anchor index holds the index of the item from which non-ranged keyboard selection
        /// will start, i.e. in a vertical list it will hold the item from which selection should
        /// be moved by pressing the up or down arrow key. This is usually the last selected item.
        /// 
        /// <see cref="AnchorIndex"/> is automatically set when selecting an item via
        /// <see cref="SelectedIndex"/> or <see cref="Select(IndexPath)"/>.
        /// </remarks>
        IndexPath AnchorIndex { get; set; }

        /// <summary>
        /// Gets or sets the range anchor index.
        /// </summary>
        /// <remarks>
        /// The range anchor index holds the index of the item from which ranged selection will
        /// start, i.e. when shift-clicking an item it represents the start of the range to be 
        /// selected whereas the shift-clicked item will be the end of the range.
        /// 
        /// <see cref="RangeAnchorIndex"/> is set when selecting an item via
        /// <see cref="SelectedIndex"/> but not via <see cref="Select(IndexPath)"/>.
        /// </remarks>
        IndexPath RangeAnchorIndex { get; set; }

        /// <summary>
        /// Gets the number of selected items.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Occurs when the selection changes due to items being selected/deselected, or selected
        /// items being removed from the source.
        /// </summary>
        /// <remarks>
        /// Note that due to limitations of the <see cref="INotifyCollectionChanged"/> feature in
        /// the .NET BCL, this event will not be raised when a
        /// <see cref="NotifyCollectionChangedAction.Reset"/> signal is retreived due to e.g.
        /// calling <see cref="IList.Clear()"/> on an <see cref="ObservableCollection{T}"/>.
        /// For this reason it is advised that all subscriptions to <see cref="SelectionChanged"/>
        /// are paired with a subscription to <see cref="SourceReset"/>.
        /// /// </remarks>
        event EventHandler<TreeSelectionModelSelectionChangedEventArgs>? SelectionChanged;

        /// <summary>
        /// Occurs when the indexes of the selected items are changed due to items being added or
        /// removed from the source data.
        /// </summary>
        event EventHandler<TreeSelectionModelIndexesChangedEventArgs>? IndexesChanged;

        /// <summary>
        /// Occurs when a <see cref="NotifyCollectionChangedAction.Reset"/> signal is retreived on
        /// the source data.
        /// </summary>
        event EventHandler<TreeSelectionModelSourceResetEventArgs>? SourceReset;

        /// <summary>
        /// Clears the selection.
        /// </summary>
        void Clear();

        /// <summary>
        /// Selects an item by its index in the source data.
        /// </summary>
        /// <param name="index">The index into the source data.</param>
        void Select(IndexPath index);

        /// <summary>
        /// Deselects an item by its index in the source data.
        /// </summary>
        /// <param name="index">The index into the source data.</param>
        void Deselect(IndexPath index);

        /// <summary>
        /// Queries whether an item is currently selected.
        /// </summary>
        /// <param name="index">The index of the item in the source data.</param>
        /// <returns>True if the item is selected; otherwise false.</returns>
        bool IsSelected(IndexPath index);

        /// <summary>
        /// Begins a batch update of the selection.
        /// </summary>
        /// <remarks>
        /// During a batch update no changes to the selection will be reflected in the model's
        /// properties and no events will be raised until <see cref="EndBatchUpdate"/> is called.
        /// 
        /// <see cref="BeginBatchUpdate"/> may be called multiple times, even when a batch update
        /// is already in progress; each call must have a corresponding call to
        /// <see cref="EndBatchUpdate"/> in order to finish the operation.
        /// </remarks>
        void BeginBatchUpdate();

        /// <summary>
        /// Ends a batch update started by <see cref="BeginBatchUpdate"/>.
        /// </summary>
        /// <see cref="BeginBatchUpdate"/>.
        void EndBatchUpdate();
    }
}
