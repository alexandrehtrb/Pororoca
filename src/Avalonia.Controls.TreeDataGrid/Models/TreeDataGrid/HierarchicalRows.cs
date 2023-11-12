using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls.Utils;
using Avalonia.Utilities;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class HierarchicalRows<TModel> : ReadOnlyListBase<HierarchicalRow<TModel>>,
        IRows,
        IDisposable,
        IExpanderRowController<TModel>
    {
        private readonly IExpanderRowController<TModel> _controller;
        private readonly RootRows _roots;
        private readonly IExpanderColumn<TModel> _expanderColumn;
        private readonly List<HierarchicalRow<TModel>> _flattenedRows;
        private Comparison<TModel>? _comparison;

        public HierarchicalRows(
            IExpanderRowController<TModel> controller,
            TreeDataGridItemsSourceView<TModel> items,
            IExpanderColumn<TModel> expanderColumn,
            Comparison<TModel>? comparison)
        {
            _controller = controller;
            _roots = new RootRows(this, items, comparison);
            _roots.CollectionChanged += OnRootsCollectionChanged;
            _expanderColumn = expanderColumn;
            _comparison = comparison;
            _flattenedRows = new List<HierarchicalRow<TModel>>();
            InitializeRows();
        }

        public override HierarchicalRow<TModel> this[int index] => _flattenedRows[index];
        IRow IReadOnlyList<IRow>.this[int index] => _flattenedRows[index];
        public override int Count => _flattenedRows.Count;

        public void Dispose()
        {
            _roots.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Expand(IndexPath index)
        {
            var count = index.Count;
            var rows = (IReadOnlyList<HierarchicalRow<TModel>>?)_roots;

            for (var i = 0; i < count; ++i)
            {
                if (rows is null)
                    break;

                var modelIndex = index[i];
                var found = false;

                foreach (var row in rows)
                {
                    if (row.ModelIndex == modelIndex)
                    {
                        row.IsExpanded = true;
                        rows = row.Children;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    break;
            }
        }

        public void Collapse(IndexPath index)
        {
            var count = index.Count;
            var rows = (IReadOnlyList<HierarchicalRow<TModel>>?)_roots;

            for (var i = 0; i < count; ++i)
            {
                if (rows is null)
                    break;

                var modelIndex = index[i];
                var found = false;

                foreach (var row in rows)
                {
                    if (row.ModelIndex == modelIndex)
                    {
                        if (i == count - 1)
                            row.IsExpanded = false;
                        rows = row.Children;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    break;
            }
        }

        public (int index, double y) GetRowAt(double y)
        {
            if (MathUtilities.IsZero(y))
                return (0, 0);
            return (-1, -1);
        }

        public ICell RealizeCell(IColumn column, int columnIndex, int rowIndex)
        {
            if (column is IColumn<TModel> c)
                return c.CreateCell(this[rowIndex]);
            else
                throw new InvalidOperationException("Invalid column.");
        }

        public void SetItems(TreeDataGridItemsSourceView<TModel> items)
        {
            _roots.SetItems(items);
        }

        public void Sort(Comparison<TModel>? comparison)
        {
            _comparison = comparison;
            _roots.Sort(comparison);
            _flattenedRows.Clear();
            InitializeRows();
            CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);

            foreach (var row in _roots)
            {
                row.SortChildren(comparison);
            }
        }

        public void UnrealizeCell(ICell cell, int rowIndex, int columnIndex)
        {
            (cell as IDisposable)?.Dispose();
        }

        public int GetParentRowIndex(IndexPath modelIndex)
        {
            return ModelIndexToRowIndex(modelIndex[..^1]);
        }

        public int ModelIndexToRowIndex(IndexPath modelIndex)
        {
            if (modelIndex == default)
                return -1;
            
            for (var i = 0; i < _flattenedRows.Count; ++i)
            {
                if (_flattenedRows[i].ModelIndexPath == modelIndex)
                    return i;
            }

            return -1;
        }

        public IndexPath RowIndexToModelIndex(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < _flattenedRows.Count)
                return _flattenedRows[rowIndex].ModelIndexPath;
            return default;
        }

        public override IEnumerator<HierarchicalRow<TModel>> GetEnumerator() => _flattenedRows.GetEnumerator();
        IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => _flattenedRows.GetEnumerator();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        void IExpanderRowController<TModel>.OnBeginExpandCollapse(IExpanderRow<TModel> row)
        {
            _controller.OnBeginExpandCollapse(row);
        }

        void IExpanderRowController<TModel>.OnEndExpandCollapse(IExpanderRow<TModel> row)
        {
            _controller.OnEndExpandCollapse(row);
        }

        void IExpanderRowController<TModel>.OnChildCollectionChanged(
            IExpanderRow<TModel> row,
            NotifyCollectionChangedEventArgs e)
        {
            if (row is HierarchicalRow<TModel> h)
                OnCollectionChanged(h.ModelIndexPath, e);
            else
                throw new NotSupportedException("Unexpected row type.");
        }

        internal bool TryGetRowIndex(in IndexPath modelIndex, out int rowIndex, int fromRowIndex = 0)
        {
            if (modelIndex.Count == 0)
            {
                rowIndex = -1;
                return true;
            }

            for (var i = fromRowIndex; i < _flattenedRows.Count; ++i)
            {
                if (modelIndex == _flattenedRows[i].ModelIndexPath)
                {
                    rowIndex = i;
                    return true;
                }
            }

            rowIndex = -1;
            return false;
        }

        private void InitializeRows()
        {
            var i = 0;

            foreach (var model in _roots)
            {
                i += AddRowsAndDescendants(i, model);
            }
        }

        private int AddRowsAndDescendants(int index, HierarchicalRow<TModel> row)
        {
            var i = index;
            _flattenedRows.Insert(i++, row);

            if (row.Children is object)
            {
                foreach (var childRow in row.Children)
                {
                    i += AddRowsAndDescendants(i, childRow);
                }
            }

            return i - index;
        }

        private void OnCollectionChanged(in IndexPath parentIndex, NotifyCollectionChangedEventArgs e)
        {
            void Add(int index, IEnumerable? items, bool raise)
            {
                if (items is null)
                    return;

                var start = index;

                foreach (HierarchicalRow<TModel> row in items)
                {
                    index += AddRowsAndDescendants(index, row);
                }

                if (raise && index > start)
                {
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            new ListSpan(_flattenedRows, start, index - start),
                            start));
                }
            }

            void Remove(int index, int count, bool raise)
            {
                if (count == 0)
                    return;

                var oldItems = raise && CollectionChanged is not null ?
                    new HierarchicalRow<TModel>[count] : null;

                for (var i = 0; i < count; ++i)
                {
                    var row = _flattenedRows[i + index];
                    if (oldItems is not null)
                        oldItems[i] = row;
                }

                _flattenedRows.RemoveRange(index, count);
                
                if (oldItems is not null)
                {
                    CollectionChanged!(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove,
                            oldItems,
                            index));
                }
            }

            int Advance(int rowIndex, int count)
            {
                var i = rowIndex;

                while (count > 0)
                {
                    var row = _flattenedRows[i];
                    if (row.Children?.Count > 0)
                        i = Advance(i + 1, row.Children.Count);
                    else
                        i += + 1;
                    --count;
                }

                return i;
            }

            int GetDescendentRowCount(int rowIndex)
            {
                if (rowIndex == -1)
                    return _flattenedRows.Count;

                var row = _flattenedRows[rowIndex];
                var depth = row.ModelIndexPath.Count;
                var i = rowIndex + 1;

                while (i < _flattenedRows.Count && _flattenedRows[i].ModelIndexPath.Count > depth)
                    ++i;

                return i - (rowIndex + 1);
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (TryGetRowIndex(parentIndex, out var parentRowIndex))
                    {
                        var insert = Advance(parentRowIndex + 1, e.NewStartingIndex);
                        Add(insert, e.NewItems, true);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (TryGetRowIndex(parentIndex, out parentRowIndex))
                    {
                        var start = Advance(parentRowIndex + 1, e.OldStartingIndex);
                        var end = Advance(start, e.OldItems!.Count);
                        Remove(start, end - start, true);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (TryGetRowIndex(parentIndex, out parentRowIndex))
                    {
                        var start = Advance(parentRowIndex + 1, e.OldStartingIndex);
                        var end = Advance(start, e.OldItems!.Count);
                        Remove(start, end - start, true);
                        Add(start, e.NewItems, true);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (TryGetRowIndex(parentIndex, out parentRowIndex))
                    {
                        var fromStart = Advance(parentRowIndex + 1, e.OldStartingIndex);
                        var fromEnd = Advance(fromStart, e.OldItems!.Count);
                        var to = Advance(parentRowIndex + 1, e.NewStartingIndex);
                        Remove(fromStart, fromEnd - fromStart, true);
                        Add(to, e.NewItems, true);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (TryGetRowIndex(parentIndex, out parentRowIndex))
                    {
                        var children = parentRowIndex >= 0 ? _flattenedRows[parentRowIndex].Children : _roots;
                        var count = GetDescendentRowCount(parentRowIndex);
                        Remove(parentRowIndex + 1, count, true);
                        Add(parentRowIndex + 1, children, true);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void OnRootsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(default, e);
        }

        private class RootRows : SortableRowsBase<TModel, HierarchicalRow<TModel>>,
            IReadOnlyList<HierarchicalRow<TModel>>
        {
            private readonly HierarchicalRows<TModel> _owner;

            public RootRows(
                HierarchicalRows<TModel> owner,
                TreeDataGridItemsSourceView<TModel> items,
                Comparison<TModel>? comparison)
                : base(items, comparison)
            {
                _owner = owner;
            }

            protected override HierarchicalRow<TModel> CreateRow(int modelIndex, TModel model)
            {
                return new HierarchicalRow<TModel>(
                    _owner,
                    _owner._expanderColumn,
                    new IndexPath(modelIndex),
                    model,
                    _owner._comparison);
            }
        }
    }
}
