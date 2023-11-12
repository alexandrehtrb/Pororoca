using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Controls.Models;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Input;

namespace Avalonia.Controls
{
    /// <summary>
    /// A data source for a <see cref="TreeDataGrid"/> which displays a hierarchial tree where each
    /// row may have multiple columns.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class HierarchicalTreeDataGridSource<TModel> : NotifyingBase,
        ITreeDataGridSource<TModel>,
        IDisposable,
        IExpanderRowController<TModel>
        where TModel: class
    {
        private IEnumerable<TModel> _items;
        private TreeDataGridItemsSourceView<TModel> _itemsView;
        private IExpanderColumn<TModel>? _expanderColumn;
        private HierarchicalRows<TModel>? _rows;
        private Comparison<TModel>? _comparison;
        private ITreeDataGridSelection? _selection;
        private bool _isSelectionSet;

        public HierarchicalTreeDataGridSource(TModel item)
            : this(new[] { item })
        {
        }

        public HierarchicalTreeDataGridSource(IEnumerable<TModel> items)
        {
            _items = items;
            _itemsView = TreeDataGridItemsSourceView<TModel>.GetOrCreate(items);
            Columns = new ColumnList<TModel>();
            Columns.CollectionChanged += OnColumnsCollectionChanged;
        }

        public IEnumerable<TModel> Items 
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    _itemsView = TreeDataGridItemsSourceView<TModel>.GetOrCreate(value);
                    _rows?.SetItems(_itemsView);
                    if (_selection is object)
                        _selection.Source = value;
                }
            }
        }

        public IRows Rows => GetOrCreateRows();
        public ColumnList<TModel> Columns { get; }

        public ITreeDataGridSelection? Selection
        {
            get
            {
                if (_selection == null && !_isSelectionSet)
                    _selection = new TreeDataGridRowSelectionModel<TModel>(this);
                return _selection;
            }
            set
            {
                if (_selection != value)
                {
                    if (value?.Source != _items)
                        throw new InvalidOperationException("Selection source must be set to Items.");
                    _selection = value;
                    _isSelectionSet = true;
                    RaisePropertyChanged();
                }
            }
        }

        IEnumerable<object> ITreeDataGridSource.Items => Items;

        public ITreeDataGridCellSelectionModel<TModel>? CellSelection => Selection as ITreeDataGridCellSelectionModel<TModel>;
        public ITreeDataGridRowSelectionModel<TModel>? RowSelection => Selection as ITreeDataGridRowSelectionModel<TModel>;
        public bool IsHierarchical => true;
        public bool IsSorted => _comparison is not null;

        IColumns ITreeDataGridSource.Columns => Columns;

        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowExpanding;
        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowExpanded;
        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowCollapsing;
        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowCollapsed;
        public event Action? Sorted;

        public void Dispose()
        {
            _rows?.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Expand(IndexPath index) => GetOrCreateRows().Expand(index);
        public void Collapse(IndexPath index) => GetOrCreateRows().Collapse(index);

        public bool TryGetModelAt(IndexPath index, [NotNullWhen(true)] out TModel? result)
        {
            if (_expanderColumn is null)
                throw new InvalidOperationException("No expander column defined.");

            var items = (IEnumerable<TModel>?)Items;
            var count = index.Count;

            for (var depth = 0; depth < count; ++depth)
            {
                var i = index[depth];

                if (i < items?.Count())
                {
                    var e = items.ElementAt(i)!;

                    if (depth < count - 1)
                    {
                        items = _expanderColumn.GetChildModels(e);
                    }
                    else
                    {
                        result = e;
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }

            result = default;
            return false;
        }

        public void Sort(Comparison<TModel>? comparison)
        {
            _comparison = comparison;
            _rows?.Sort(_comparison);
        }

        IEnumerable<object>? ITreeDataGridSource.GetModelChildren(object model)
        {
            return GetModelChildren((TModel)model);
        }

        public bool SortBy(IColumn? column, ListSortDirection direction)
        {
            if (column is IColumn<TModel> columnBase &&
                Columns.Contains(columnBase) &&
                columnBase.GetComparison(direction) is Comparison<TModel> comparison)
            {
                Sort(comparison);
                Sorted?.Invoke();
                foreach (var c in Columns)
                    c.SortDirection = c == column ? (ListSortDirection?)direction : null;
                return true;
            }

            return false;
        }

        void ITreeDataGridSource.DragDropRows(
            ITreeDataGridSource source,
            IEnumerable<IndexPath> indexes,
            IndexPath targetIndex,
            TreeDataGridRowDropPosition position,
            DragDropEffects effects)
        {
            IList<TModel> GetItems(IndexPath path)
            {
                IEnumerable<TModel>? children;

                if (path.Count == 0)
                    children = _items;
                else if (TryGetModelAt(path, out var parent))
                    children = GetModelChildren(parent);
                else
                    throw new IndexOutOfRangeException();

                if (children is null)
                    throw new InvalidOperationException("The requested drop target has no children.");

                return children as IList<TModel> ??
                    throw new InvalidOperationException("Items does not implement IList<T>.");
            }

            if (effects != DragDropEffects.Move)
                throw new NotSupportedException("Only move is currently supported for drag/drop.");
            if (IsSorted)
                throw new NotSupportedException("Drag/drop is not supported on sorted data.");

            IList<TModel> targetItems;
            int ti;

            if (position == TreeDataGridRowDropPosition.Inside)
            {
                targetItems = GetItems(targetIndex);
                ti = targetItems.Count;
            }
            else
            {
                targetItems = GetItems(targetIndex[..^1]);
                ti = targetIndex[^1];
            }

            if (position == TreeDataGridRowDropPosition.After)
                ++ti;

            var sourceItems = new List<TModel>();

            foreach (var g in indexes.GroupBy(x => x[..^1]))
            {
                var items = GetItems(g.Key);

                foreach (var i in g.Select(x => x[^1]).OrderByDescending(x => x))
                {
                    sourceItems.Add(items[i]);

                    if (items == targetItems && i < ti)
                        --ti;
                    
                    items.RemoveAt(i);
                }
            }

            for (var si = sourceItems.Count - 1; si >= 0; --si)
            {
                targetItems.Insert(ti++, sourceItems[si]);
            }
        }

        void IExpanderRowController<TModel>.OnBeginExpandCollapse(IExpanderRow<TModel> row)
        {
            if (row is HierarchicalRow<TModel> r)
            {
                if (!row.IsExpanded)
                    RowExpanding?.Invoke(this, RowEventArgs.Create(r));
                else
                    RowCollapsing?.Invoke(this, RowEventArgs.Create(r));
            }
        }

        void IExpanderRowController<TModel>.OnEndExpandCollapse(IExpanderRow<TModel> row)
        {
            if (row is HierarchicalRow<TModel> r)
            {
                if (row.IsExpanded)
                    RowExpanded?.Invoke(this, RowEventArgs.Create(r));
                else
                    RowCollapsed?.Invoke(this, RowEventArgs.Create(r));
            }
        }

        void IExpanderRowController<TModel>.OnChildCollectionChanged(
            IExpanderRow<TModel> row,
            NotifyCollectionChangedEventArgs e)
        {
        }

        internal IEnumerable<TModel>? GetModelChildren(TModel model)
        {
            _ = _expanderColumn ?? throw new InvalidOperationException("No expander column defined.");
            return _expanderColumn.GetChildModels(model);
        }

        internal int GetRowIndex(in IndexPath index, int fromRowIndex = 0)
        {
            var result = -1;
            _rows?.TryGetRowIndex(index, out result, fromRowIndex);
            return result;
        }

        private HierarchicalRows<TModel> GetOrCreateRows()
        {
            if (_rows is null)
            {
                if (Columns.Count == 0)
                    throw new InvalidOperationException("No columns defined.");
                if (_expanderColumn is null)
                    throw new InvalidOperationException("No expander column defined.");
                _rows = new HierarchicalRows<TModel>(this, _itemsView, _expanderColumn, _comparison);
            }

            return _rows;
        }

        private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (_expanderColumn is null && e.NewItems is object)
                    {
                        foreach (var i in e.NewItems)
                        {
                            if (i is IExpanderColumn<TModel> expander)
                            {
                                _expanderColumn = expander;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
