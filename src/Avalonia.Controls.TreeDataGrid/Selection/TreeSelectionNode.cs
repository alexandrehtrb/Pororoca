using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls.Models.TreeDataGrid;

#nullable enable

namespace Avalonia.Controls.Selection
{
#pragma warning disable CS0436 // Type conflicts with imported type
    internal class TreeSelectionNode<T> : SelectionNodeBase<T>
#pragma warning restore CS0436 // Type conflicts with imported type
    {
        private readonly TreeSelectionModelBase<T> _owner;
        private List<TreeSelectionNode<T>?>? _children;

        public TreeSelectionNode(TreeSelectionModelBase<T> owner)
        {
            _owner = owner;
            RangesEnabled = true;
        }

        public TreeSelectionNode(
            TreeSelectionModelBase<T> owner,
            TreeSelectionNode<T> parent,
            int index)
            : this(owner)
        {
            Path = parent.Path.Append(index);
            if (parent.ItemsView is not null)
                Source = _owner.GetChildren(parent.ItemsView[index]);
        }

        public IndexPath Path { get; private set; }

        public new IEnumerable? Source
        {
            get => base.Source;
            set => base.Source = value;
        }

        public bool HasChildren
        {
            get
            {
                if (_children is null)
                    return false;

                foreach (var child in _children)
                {
                    if (child is not null)
                        return true;
                }

                return false;
            }
        }

        public IReadOnlyList<TreeSelectionNode<T>?>? Children => _children;

        public void Clear(TreeSelectionModelBase<T>.Operation operation)
        {
            if (Ranges.Count > 0)
            {
                operation.DeselectedRanges ??= new();
                foreach (var range in Ranges)
                    operation.DeselectedRanges.Add(Path, range);
            }

            if (_children is not null)
            {
                foreach (var child in _children)
                    child?.Clear(operation);
            }
        }

        public int CommitSelect(IndexRange range) => CommitSelect(range.Begin, range.End);
        public int CommitDeselect(IndexRange range) => CommitDeselect(range.Begin, range.End);
        public TreeSelectionNode<T>? GetChild(int index) => index < _children?.Count ? _children[index] : null;

        public TreeSelectionNode<T>? GetOrCreateChild(int index)
        {
            if (GetChild(index) is TreeSelectionNode<T> result)
                return result;

            var childCount = ItemsView is not null ? ItemsView.Count : Math.Max(_children?.Count ?? 0, index);

            if (index < childCount)
            {
                _children ??= new List<TreeSelectionNode<T>?>();
                Resize(_children, childCount);
                return _children[index] ??= new TreeSelectionNode<T>(_owner, this, index);
            }

            return null;
        }

        public void PruneEmptyChildren()
        {
            if (_children is null)
                return;

            for (var i = 0; i < _children.Count; ++i)
            {
                if (_children[i] is TreeSelectionNode<T> node)
                {
                    node.PruneEmptyChildren();
                    
                    if (node.Ranges.Count == 0 && !node.HasChildren)
                    {
                        node.Source = null;
                        _children[i] = null;
                    }
                }
            }
        }

        protected override void OnSourceCollectionChangeStarted()
        {
            _owner.OnNodeCollectionChangeStarted();
        }

        protected override void OnSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var shiftIndex = 0;
            var shiftDelta = 0;
            var indexesChanged = false;
            List<T?>? removed = null;

            // Adjust the selection in this node according to the collection change.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    shiftIndex = e.NewStartingIndex;
                    shiftDelta = e.NewItems!.Count;
                    indexesChanged = OnItemsAdded(shiftIndex, e.NewItems).ShiftDelta > 0;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    shiftIndex = e.OldStartingIndex;
                    shiftDelta = -e.OldItems!.Count;
                    var change = OnItemsRemoved(shiftIndex, e.OldItems);
                    indexesChanged = change.ShiftDelta != 0;
                    removed = change.RemovedItems;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var removeChange = OnItemsRemoved(e.OldStartingIndex, e.OldItems!);
                    var addChange = OnItemsAdded(e.NewStartingIndex, e.NewItems!);
                    shiftIndex = removeChange.ShiftIndex;
                    shiftDelta = removeChange.ShiftDelta + addChange.ShiftDelta;
                    indexesChanged = shiftDelta != 0;
                    removed = removeChange.RemovedItems;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnSourceReset();
                    break;
                default:
                    throw new NotSupportedException($"Collection {e.Action} not supported.");
            }

            // Adjust the paths of any child nodes.
            if (_children?.Count > 0 && shiftDelta != 0)
            {
                for (var i = shiftIndex; i < _children.Count; ++i)
                {
                    var child = _children[i];

                    if (shiftDelta < 1 && i >= shiftIndex && i < shiftIndex - shiftDelta)
                    {
                        child?.AncestorRemoved(ref removed);
                    }
                    else
                    {
                        child?.AncestorIndexChanged(Path, shiftIndex, shiftDelta);
                        indexesChanged = true;
                    }
                }

                if (shiftDelta > 0)
                    _children.InsertMany(shiftIndex, null, shiftDelta);
                else
                    _children.RemoveRange(shiftIndex, -shiftDelta);
            }

            if (shiftDelta != 0 || removed?.Count> 0)
                _owner.OnNodeCollectionChanged(Path, shiftIndex, shiftDelta, indexesChanged, removed);
        }

        protected override void OnSourceCollectionChangeFinished()
        {
            _owner.OnNodeCollectionChangeFinished();
        }

        protected override void OnSourceReset()
        {
            var removed = CommitDeselect(new IndexRange(0, int.MaxValue));

            if (_children is not null)
            {
                foreach (var child in _children)
                    child?.AncestorReset(ref removed);
                _children = null;
            }

            _owner.OnNodeCollectionReset(Path, removed);
        }

        private void AncestorIndexChanged(IndexPath parentIndex, int shiftIndex, int shiftDelta)
        {
            var path = Path;

            if (ShiftIndex(parentIndex, shiftIndex, shiftDelta, ref path))
                Path = path;

            if (_children is not null)
            {
                foreach (var child in _children)
                {
                    child?.AncestorIndexChanged(parentIndex, shiftIndex, shiftDelta);
                }
            }
        }

        private void AncestorRemoved(ref List<T?>? removed)
        {
            if (Ranges.Count > 0)
            {
                removed ??= new();

                foreach (var range in Ranges)
                {
                    for (var i = range.Begin; i <= range.End; i++)
                        removed.Add(ItemsView![i]);
                }
            }

            if (_children is not null)
            {
                foreach (var child in _children)
                    child?.AncestorRemoved(ref removed);
            }

            Source = null;
        }

        private void AncestorReset(ref int removedCount)
        {
            if (Ranges.Count > 0)
            {
                removedCount += CommitDeselect(0, int.MaxValue);
            }

            if (_children is not null)
            {
                foreach (var child in _children)
                    child?.AncestorReset(ref removedCount);
            }

            Source = null;
        }

        private static void Resize(List<TreeSelectionNode<T>?> list, int count)
        {
            var current = list.Count;

            if (count < current)
            {
                list.RemoveRange(count, current - count);
            }
            else if (count > current)
            {
                if (count > list.Capacity)
                {
                    list.Capacity = count;
                }

                list.InsertMany(0, null, count - current);
            }
        }

        internal static bool ShiftIndex(IndexPath parentIndex, int shiftIndex, int shiftDelta, ref IndexPath path)
        {
            if (path[parentIndex.Count] >= shiftIndex)
            {
                var indexes = path.ToArray();
                indexes[parentIndex.Count] += shiftDelta;
                path = new IndexPath(indexes);
                return true;
            }

            return false;
        }
    }
}
