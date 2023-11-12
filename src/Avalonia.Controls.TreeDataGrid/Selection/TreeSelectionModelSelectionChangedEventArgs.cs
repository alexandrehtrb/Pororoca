using System;
using System.Collections;
using System.Collections.Generic;

namespace Avalonia.Controls.Selection
{
    public abstract class TreeSelectionModelSelectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the indexes of the items that were removed from the selection.
        /// </summary>
        public abstract IReadOnlyList<IndexPath> DeselectedIndexes { get; }

        /// <summary>
        /// Gets the indexes of the items that were added to the selection.
        /// </summary>
        public abstract IReadOnlyList<IndexPath> SelectedIndexes { get; }

        /// <summary>
        /// Gets the items that were removed from the selection.
        /// </summary>
        public IReadOnlyList<object?> DeselectedItems => GetUntypedDeselectedItems();

        /// <summary>
        /// Gets the items that were added to the selection.
        /// </summary>
        public IReadOnlyList<object?> SelectedItems => GetUntypedSelectedItems();

        protected abstract IReadOnlyList<object?> GetUntypedDeselectedItems();
        protected abstract IReadOnlyList<object?> GetUntypedSelectedItems();
    }

    public class TreeSelectionModelSelectionChangedEventArgs<T> : TreeSelectionModelSelectionChangedEventArgs
    {
        private IReadOnlyList<object?>? _deselectedItems;
        private IReadOnlyList<object?>? _selectedItems;

        public TreeSelectionModelSelectionChangedEventArgs(
            IReadOnlyList<IndexPath>? deselectedIndexes = null,
            IReadOnlyList<IndexPath>? selectedIndexes = null,
            IReadOnlyList<T?>? deselectedItems = null,
            IReadOnlyList<T?>? selectedItems = null)
        {
            DeselectedIndexes = deselectedIndexes ?? Array.Empty<IndexPath>();
            SelectedIndexes = selectedIndexes ?? Array.Empty<IndexPath>();
            DeselectedItems = deselectedItems ?? Array.Empty<T>();
            SelectedItems = selectedItems ?? Array.Empty<T>();
        }

        /// <summary>
        /// Gets the indexes of the items that were removed from the selection.
        /// </summary>
        public override IReadOnlyList<IndexPath> DeselectedIndexes { get; }

        /// <summary>
        /// Gets the indexes of the items that were added to the selection.
        /// </summary>
        public override IReadOnlyList<IndexPath> SelectedIndexes { get; }

        /// <summary>
        /// Gets the items that were removed from the selection.
        /// </summary>
        public new IReadOnlyList<T?> DeselectedItems { get; }

        /// <summary>
        /// Gets the items that were added to the selection.
        /// </summary>
        public new IReadOnlyList<T?> SelectedItems { get; }

        protected override IReadOnlyList<object?> GetUntypedDeselectedItems()
        {
            return _deselectedItems ??= (DeselectedItems as IReadOnlyList<object?>) ??
                new Untyped(DeselectedItems);
        }

        protected override IReadOnlyList<object?> GetUntypedSelectedItems()
        {
            return _selectedItems ??= (SelectedItems as IReadOnlyList<object?>) ??
                new Untyped(SelectedItems);
        }

        private class Untyped : IReadOnlyList<object?>
        {
            private readonly IReadOnlyList<T?> _source;
            public Untyped(IReadOnlyList<T?> source) => _source = source;
            public object? this[int index] => _source[index];
            public int Count => _source.Count;
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<object?> GetEnumerator()
            {
                foreach (var i in _source)
                {
                    yield return i;
                }
            }
        }
    }
}
