using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls.Utils;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// An <see cref="IRows"/> collection which supports sorting.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TRow">The row type.</typeparam>
    public abstract class SortableRowsBase<TModel, TRow> : ReadOnlyListBase<TRow>, IDisposable
        where TRow : IRow<TModel>, IModelIndexableRow, IDisposable
    {
        private readonly Comparison<int> _compareItemsByIndex;
        private TreeDataGridItemsSourceView<TModel> _items;
        private Comparison<TModel>? _comparison;
        private List<TRow>? _unsortedRows;
        private List<int>? _sortedIndexes;

        public SortableRowsBase(TreeDataGridItemsSourceView<TModel> items, Comparison<TModel>? comparison)
        {
            _items = items;
            _items.CollectionChanged += OnItemsCollectionChanged;
            _comparison = comparison;
            _compareItemsByIndex = CompareItemsByIndex;
        }

        public override int Count => _unsortedRows?.Count ?? _items.Count;

        public override TRow this[int index]
        {
            get
            {
                if (_sortedIndexes is null)
                    return UnsortedRows[index];
                else
                    return UnsortedRows[_sortedIndexes[index]];
            }
        }

        private List<TRow> UnsortedRows => GetOrCreateRows();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public virtual void Dispose()
        {
            SetItems(TreeDataGridItemsSourceView<TModel>.Empty);
            GC.SuppressFinalize(this);
        }

        public override IEnumerator<TRow> GetEnumerator()
        {
            IEnumerator<TRow> GetSortedEnumerator()
            {
                var rows = UnsortedRows;

                foreach (var i in _sortedIndexes!)
                    yield return rows[i];
            }

            GetOrCreateRows();
            return _sortedIndexes is not null ? GetSortedEnumerator() : UnsortedRows.GetEnumerator();
        }

        public void SetItems(TreeDataGridItemsSourceView<TModel> items)
        {
            _items.CollectionChanged -= OnItemsCollectionChanged;
            _items = items;

            if (!ReferenceEquals(items, TreeDataGridItemsSourceView<TModel>.Empty))
                _items.CollectionChanged += OnItemsCollectionChanged;

            OnItemsCollectionChanged(null, CollectionExtensions.ResetEvent);
        }

        public virtual void Sort(Comparison<TModel>? comparison)
        {
            _comparison = comparison;

            if (_unsortedRows is not null)
            {
                if (comparison is not null)
                    _sortedIndexes = StableSort.SortedMap(_items, _compareItemsByIndex);
                else
                    _sortedIndexes = null;

                var foo = this.ToArray();
                CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
            }
        }

        protected abstract TRow CreateRow(int modelIndex, TModel model);

        protected int ModelIndexToRowIndex(int modelIndex)
        {
            if (_sortedIndexes is null)
                return modelIndex >= 0 && modelIndex < _items.Count ? modelIndex : -1;
            else
                return SortHelper<int>.BinarySearch(_sortedIndexes, modelIndex, _compareItemsByIndex);
        }

        protected int RowIndexToModelIndex(int rowIndex) => _sortedIndexes?[rowIndex] ?? rowIndex;

        private List<TRow> GetOrCreateRows()
        {
            if (_unsortedRows is null)
            {
                _unsortedRows = new List<TRow>(_items.Count);

                for (var i = 0; i < _items.Count; ++i)
                    _unsortedRows.Add(CreateRow(i, _items[i]));

                if (_comparison is not null)
                    _sortedIndexes = StableSort.SortedMap(_items, _compareItemsByIndex);
            }

            return _unsortedRows;
        }

        private void ResetRows()
        {
            if (_unsortedRows is not null)
            {
                foreach (var row in _unsortedRows)
                    row.Dispose();
            }

            _unsortedRows = null;
            _sortedIndexes = null;
        }

        private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_comparison is null)
                OnItemsCollectionChangedUnsorted(e);
            else
                OnItemsCollectionChangedSorted(e);
        }

        private void OnItemsCollectionChangedUnsorted(NotifyCollectionChangedEventArgs e)
        {
            if (_unsortedRows is null)
                return;

            void Add(int index, IList items)
            {
                foreach (TModel model in items)
                {
                    _unsortedRows.Insert(index, CreateRow(index, model));
                    ++index;
                }

                while (index < _unsortedRows.Count)
                    _unsortedRows[index++].UpdateModelIndex(items.Count);
            }

            void Remove(int index, int count)
            {
                for (var i = index; i < index + count; ++i)
                    _unsortedRows[i].Dispose();

                _unsortedRows.RemoveRange(index, count);

                while (index < _unsortedRows.Count)
                    _unsortedRows[index++].UpdateModelIndex(-count);
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewStartingIndex, e.NewItems!);
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            new ListSpan(_unsortedRows, e.NewStartingIndex, e.NewItems!.Count),
                            e.NewStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        var oldItems = CollectionChanged is not null ?
                            _unsortedRows.Slice(e.OldStartingIndex, e.OldItems!.Count) : null;
                        Remove(e.OldStartingIndex, e.OldItems!.Count);
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                oldItems,
                                e.OldStartingIndex));
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        var index = e.OldStartingIndex;
                        var count = e.OldItems!.Count;
                        var oldItems = CollectionChanged is not null ? _unsortedRows.Slice(index, count) : null;
                        
                        for (var i = 0; i < count; ++i)
                        {
                            _unsortedRows[index + i] = CreateRow(index + i, (TModel)e.NewItems![i]!);
                        }

                        CollectionChanged?.Invoke(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Replace,
                                new ListSpan(_unsortedRows, index, count),
                                oldItems!,
                                index));
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    Remove(e.OldStartingIndex, e.OldItems!.Count);
                    Add(e.NewStartingIndex, e.NewItems!);
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Move,
                            new ListSpan(_unsortedRows, e.NewStartingIndex, e.NewItems!.Count),
                            e.NewStartingIndex,
                            e.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetRows();
                    CollectionChanged?.Invoke(this, e);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void OnItemsCollectionChangedSorted(NotifyCollectionChangedEventArgs e)
        {
            if (_unsortedRows is null)
                return;

            void Add(int startIndex, int count)
            {
                // Add the new rows to the unsorted rows.
                for (var i = startIndex; i < startIndex + count; ++i)
                    _unsortedRows.Insert(i, CreateRow(i, _items[i]));
                
                // Update the indexes of subsequent rows.
                for (var i = startIndex + count; i < _unsortedRows.Count; ++i)
                    _unsortedRows[i].UpdateModelIndex(count);

                // Update the indexes of subsequent sorted indexes.
                for (var i = 0; i < _sortedIndexes!.Count; i++)
                {
                    var ix = _sortedIndexes[i];
                    if (ix >= startIndex)
                        _sortedIndexes[i] = ix + count;
                }

                // Insert the new row into the correct place in the sorted indexes.
                for (var i = 0; i < count; ++i)
                {
                    var index = SortHelper<int>.BinarySearch(_sortedIndexes, startIndex + i, _compareItemsByIndex);
                    if (index < 0)
                        index = ~index;
                    _sortedIndexes.Insert(index, startIndex + i);
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            _unsortedRows[startIndex + i],
                            index));
                }
            }

            void Remove(int startIndex, IList removed)
            {
                var count = removed.Count;
                var endIndex = startIndex + count;

                // Dispose the removed rows.
                for (var i = 0; i < count; ++i)
                    _unsortedRows[startIndex + i].Dispose();

                // Remove the rows from the unsorted rows.
                _unsortedRows.RemoveRange(startIndex, count);

                // Iterate the sorted indexes, raising a collection changed event for the
                // items removed, and updating the indexes of the subsequent items.
                for (var i = 0; i < _sortedIndexes!.Count; i++)
                {
                    var ix = _sortedIndexes[i];
                    if (ix >= startIndex && ix < endIndex)
                    {
                        _sortedIndexes.RemoveAt(i);
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                (TModel)removed[ix - startIndex]!,
                                i));
                        --i;
                    }
                    else if (ix >= endIndex)
                    {
                        _sortedIndexes[i] = ix - count;
                        _unsortedRows[_sortedIndexes[i]].UpdateModelIndex(-removed.Count);
                    }
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewStartingIndex, e.NewItems!.Count);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldStartingIndex, e.OldItems!);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    Remove(e.OldStartingIndex, e.OldItems!);
                    Add(e.NewStartingIndex, e.NewItems!.Count);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetRows();
                    CollectionChanged?.Invoke(this, e);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private int CompareItemsByIndex(int index1, int index2)
        {
            var c = _comparison!(_items[index1], _items[index2]);

            if (c == 0)
            {
                return index1 - index2; // ensure stability of sort
            }

            // -c will result in a negative value for int.MinValue (-int.MinValue == int.MinValue).
            // Flipping keys earlier is more likely to trigger something strange in a comparer,
            // particularly as it comes to the sort being stable.
            return (c > 0) ? 1 : -1;
        }
    }
}
