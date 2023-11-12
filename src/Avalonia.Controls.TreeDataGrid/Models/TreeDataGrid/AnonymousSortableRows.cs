using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls.Utils;
using Avalonia.Utilities;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Exposes a sortable collection of models as anonymous rows.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <remarks>
    /// In a flat grid where rows cannot be resized, it is not necessary to persist any information
    /// about rows; the same row object can be updated and reused when a new row is requested.
    /// </remarks>
    public class AnonymousSortableRows<TModel> : ReadOnlyListBase<IRow<TModel>>, IRows, IDisposable
    {
        private readonly AnonymousRow<TModel> _row;
        private readonly Comparison<int> _compareItemsByIndex;
        private TreeDataGridItemsSourceView<TModel> _items;
        private IComparer<TModel>? _comparer;
        private List<int>? _sortedIndexes;

        public AnonymousSortableRows(
            TreeDataGridItemsSourceView<TModel> items,
            IComparer<TModel>? comparer)
        {
            _items = items;
            _items.CollectionChanged += OnItemsCollectionChanged;
            _comparer = comparer;
            _row = new AnonymousRow<TModel>();
            _compareItemsByIndex = CompareItemsByIndex;
        }

        public override IRow<TModel> this[int index]
        {
            get
            {
                if (_comparer is null)
                    return _row.Update(index, _items[index]);

                _sortedIndexes ??= CreateSortedIndexes();
                var modelIndex = _sortedIndexes[index];
                return _row.Update(modelIndex, _items[modelIndex]);
            }
        }

        IRow IReadOnlyList<IRow>.this[int index] => this[index];
        public override int Count => _sortedIndexes?.Count ?? _items.Count;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void Dispose()
        {
            SetItems(TreeDataGridItemsSourceView<TModel>.Empty);
            GC.SuppressFinalize(this);
        }

        public (int index, double y) GetRowAt(double y)
        {
            // Rows in an AnonymousSortableRows collection have Auto height so we only
            // know the start position of the first row.
            if (MathUtilities.IsZero(y))
                return (0, 0);
            return (-1, -1);
        }

        public override IEnumerator<IRow<TModel>> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return this[i];
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
            _items.CollectionChanged -= OnItemsCollectionChanged;
            _items = items;

            if (!ReferenceEquals(items, TreeDataGridItemsSourceView<TModel>.Empty))
                _items.CollectionChanged += OnItemsCollectionChanged;

            OnItemsCollectionChanged(null, CollectionExtensions.ResetEvent);
        }

        public int ModelIndexToRowIndex(IndexPath modelIndex)
        {
            if (modelIndex.Count != 1)
                return -1;

            var i = modelIndex[0];

            if (_sortedIndexes is null)
                return i >= 0 && i < _items.Count ? modelIndex[0] : -1;
            else
                return SortHelper<int>.BinarySearch(_sortedIndexes, i, _compareItemsByIndex);

        }

        public IndexPath RowIndexToModelIndex(int rowIndex) => _sortedIndexes?[rowIndex] ?? rowIndex;

        public void Sort(IComparer<TModel>? comparer)
        {
            _comparer = comparer;
            _sortedIndexes = comparer is object ? CreateSortedIndexes() : null;
        }

        public void UnrealizeCell(ICell cell, int columnIndex, int rowIndex)
        {
            (cell as IDisposable)?.Dispose();
        }

        IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => GetEnumerator();

        private List<int> CreateSortedIndexes()
        {
            return StableSort.SortedMap(_items, _compareItemsByIndex);
        }

        private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_comparer is null)
                OnItemsCollectionChangedUnsorted(e);
            else
                OnItemsCollectionChangedSorted(e);
        }

        private void OnItemsCollectionChangedUnsorted(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged is null)
                return;

            var ev = e.Action switch
            {
                NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(
                    e.Action,
                    new AnonymousRowItems<TModel>(e.NewItems!),
                    e.NewStartingIndex),
                NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(
                    e.Action,
                    new AnonymousRowItems<TModel>(e.OldItems!),
                    e.OldStartingIndex),
                NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(
                    e.Action,
                    new AnonymousRowItems<TModel>(e.NewItems!),
                    new AnonymousRowItems<TModel>(e.OldItems!),
                    e.OldStartingIndex),
                NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(
                    e.Action,
                    new AnonymousRowItems<TModel>(e.NewItems!),
                    e.NewStartingIndex,
                    e.OldStartingIndex),
                NotifyCollectionChangedAction.Reset => e,
                _ => throw new NotSupportedException(),
            };

            CollectionChanged(this, ev);
        }

        private void OnItemsCollectionChangedSorted(NotifyCollectionChangedEventArgs e)
        {
            // If the rows have not yet been read then the type of collection change shouldn't be
            // important; the only thing we need to do is inform the presenter that the collection
            // has changed so that it can display the new items if the previous items were empty.
            if (_sortedIndexes is null)
            {
                CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
                return;
            }

            void Add(int startIndex, int count)
            {
                for (var i = 0; i < _sortedIndexes.Count; i++)
                {
                    var ix = _sortedIndexes[i];
                    if (ix >= startIndex)
                        _sortedIndexes[i] = ix + count;
                }

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
                            _row.Update(startIndex + i, _items[startIndex + i]),
                            index));
                }
            }

            void Remove(int startIndex, IList removed)
            {
                var count = removed.Count;
                var endIndex = startIndex + count;

                for (var i = 0; i < _sortedIndexes.Count; i++)
                {
                    var ix = _sortedIndexes[i];
                    if (ix >= startIndex && ix < endIndex)
                    {
                        _sortedIndexes.RemoveAt(i);
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                _row.Update(ix, (TModel)removed[ix - startIndex]!),
                                i));
                        --i;
                    }
                    else if (ix >= endIndex)
                    {
                        _sortedIndexes[i] = ix - count;
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
                    _sortedIndexes = CreateSortedIndexes();
                    CollectionChanged?.Invoke(this, e);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private int CompareItemsByIndex(int index1, int index2)
        {
            var c = _comparer!.Compare(_items[index1], _items[index2]);

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
