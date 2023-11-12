using System.Collections;
using System.Collections.Generic;

namespace Avalonia.Controls.Selection
{
    internal class TreeSelectionChangedItems<T> : IReadOnlyList<T>
    {
        private readonly TreeSelectionModelBase<T> _owner;
        private readonly IndexRanges _ranges;

        public TreeSelectionChangedItems(TreeSelectionModelBase<T> owner, IndexRanges ranges)
        {
            _owner = owner;
            _ranges = ranges;
        }

        public T this[int index] => _owner.GetSelectedItemAt(_ranges[index]);
        public int Count => _ranges.Count;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var index in _ranges)
                yield return _owner.GetSelectedItemAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static TreeSelectionChangedItems<T>? Create(TreeSelectionModelBase<T> owner, IndexRanges? ranges)
        {
            return ranges is not null ? new TreeSelectionChangedItems<T>(owner, ranges) : null;
        }
    }
}
