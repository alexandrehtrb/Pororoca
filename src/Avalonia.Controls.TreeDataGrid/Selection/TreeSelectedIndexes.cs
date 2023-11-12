using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace Avalonia.Controls.Selection
{
    internal class TreeSelectedIndexes<T> : IReadOnlyList<IndexPath>
    {
        private readonly TreeSelectionModelBase<T> _owner;

        public TreeSelectedIndexes(TreeSelectionModelBase<T> owner) => _owner = owner;

        public int Count => _owner.Count;

        public IndexPath this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException("The index was out of range.");

                if (_owner.SingleSelect)
                    return _owner.SelectedIndex;
                else
                {
                    var next = 0;
                    TryGetElementAt(_owner.Root, index, ref next, out var result);
                    return result;
                }
            }
        }

        public IEnumerator<IndexPath> GetEnumerator()
        {
            if (_owner.SingleSelect)
            {
                if (_owner.SelectedIndex.Count != default)
                    yield return _owner.SelectedIndex;
            }
            else
            {
                foreach (var i in EnumerateNode(_owner.Root))
                    yield return i;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<IndexPath> EnumerateNode(TreeSelectionNode<T> node)
        {
            foreach (var range in node.Ranges)
            {
                for (var i = range.Begin; i <= range.End; ++i)
                    yield return node.Path.Append(i);
            }

            if (node.Children is object)
            {
                foreach (var child in node.Children)
                {
                    if (child is not null)
                    {
                        foreach (var i in EnumerateNode(child))
                            yield return i;
                    }
                }
            }
        }

        private bool TryGetElementAt(TreeSelectionNode<T> node, int target, ref int next, out IndexPath result)
        {
            var nodeCount = IndexRange.GetCount(node.Ranges);

            if (target < next + nodeCount)
            {
                result = node.Path.Append(IndexRange.GetAt(node.Ranges, target - next));
                return true;
            }

            next += nodeCount;

            if (node.Children is object)
            {
                foreach (var child in node.Children)
                {
                    if (child is not null && TryGetElementAt(child, target, ref next, out result))
                        return true;
                }
            }

            result = default;
            return false;
        }
    }
}
