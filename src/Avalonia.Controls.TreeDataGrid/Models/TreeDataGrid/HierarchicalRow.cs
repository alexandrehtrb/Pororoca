using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A row in a <see cref="HierarchicalTreeDataGridSource{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class HierarchicalRow<TModel> : NotifyingBase,
        IExpanderRow<TModel>,
        IIndentedRow,
        IModelIndexableRow,
        IDisposable
    {
        private readonly IExpanderRowController<TModel> _controller;
        private readonly IExpanderColumn<TModel> _expanderColumn;
        private Comparison<TModel>? _comparison;
        private IEnumerable<TModel>? _childModels;
        private ChildRows? _childRows;
        private bool _isExpanded;
        private bool? _showExpander;

        public HierarchicalRow(
            IExpanderRowController<TModel> controller,
            IExpanderColumn<TModel> expanderColumn,
            IndexPath modelIndex,
            TModel model,
            Comparison<TModel>? comparison)
        {
            if (modelIndex.Count == 0)
                throw new ArgumentException("Invalid model index");

            _controller = controller;
            _expanderColumn = expanderColumn;
            _comparison = comparison;
            ModelIndexPath = modelIndex;
            Model = model;
        }

        /// <summary>
        /// Gets the row's visible child rows.
        /// </summary>
        public IReadOnlyList<HierarchicalRow<TModel>>? Children => _isExpanded ? _childRows : null;

        /// <summary>
        /// Gets the index of the model relative to its parent.
        /// </summary>
        /// <remarks>
        /// To retrieve the index path to the model from the root data source, see
        /// <see cref="ModelIndexPath"/>.
        /// </remarks>
        public int ModelIndex => ModelIndexPath[^1];

        /// <summary>
        /// Gets the index path of the model in the data source.
        /// </summary>
        public IndexPath ModelIndexPath { get; private set; }

        public object? Header => ModelIndexPath;
        public int Indent => ModelIndexPath.Count - 1;
        public TModel Model { get; }

        public GridLength Height 
        {
            get => GridLength.Auto;
            set { }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    if (value)
                        Expand();
                    else
                        Collapse();
                }
            }
        }

        public bool ShowExpander
        {
            get => _showExpander ??= _expanderColumn.HasChildren(Model);
            private set => RaiseAndSetIfChanged(ref _showExpander, value);
        }

        public void Dispose() => _childRows?.Dispose();

        public void UpdateModelIndex(int delta)
        {
            ModelIndexPath = ModelIndexPath[..^1].Append(ModelIndexPath[^1] + delta);

            if (_childRows is null)
                return;

            var childCount = _childRows.Count;

            for (var i = 0; i < childCount; ++i)
                _childRows[i].UpdateParentModelIndex(ModelIndexPath);
        }

        public void UpdateParentModelIndex(IndexPath parentIndex)
        {
            ModelIndexPath = parentIndex.Append(ModelIndex);

            if (_childRows is null)
                return;

            var childCount = _childRows.Count;

            for (var i = 0; i < childCount; ++i)
                _childRows[i].UpdateParentModelIndex(ModelIndexPath);
        }

        void IExpanderRow<TModel>.UpdateShowExpander(IExpanderCell cell, bool value)
        {
            ShowExpander = value;
        }

        internal void SortChildren(Comparison<TModel>? comparison)
        {
            _comparison = comparison;

            if (_childRows is null)
                return;

            _childRows.Sort(comparison);

            foreach (var row in _childRows)
            {
                row.SortChildren(comparison);
            }
        }

        private void Expand()
        {
            if (!_expanderColumn.HasChildren(Model))
            {
                _expanderColumn.SetModelIsExpanded(this);
                return;
            }

            _controller.OnBeginExpandCollapse(this);

            var oldExpanded = _isExpanded;
            var childModels = _expanderColumn.GetChildModels(Model);

            if (_childModels != childModels)
            {
                _childModels = childModels;
                _childRows?.Dispose();
                _childRows = new ChildRows(
                    this,
                    TreeDataGridItemsSourceView<TModel>.GetOrCreate(childModels),
                    _comparison);
            }

            if (_childRows?.Count > 0)
                _isExpanded = true;
            else
                ShowExpander = false;

            _controller.OnChildCollectionChanged(this, CollectionExtensions.ResetEvent);

            if (_isExpanded != oldExpanded)
                RaisePropertyChanged(nameof(IsExpanded));

            _controller.OnEndExpandCollapse(this);
            _expanderColumn.SetModelIsExpanded(this);
        }

        private void Collapse()
        {
            _controller.OnBeginExpandCollapse(this);
            _isExpanded = false;
            _controller.OnChildCollectionChanged(this, CollectionExtensions.ResetEvent);
            RaisePropertyChanged(nameof(IsExpanded));
            _controller.OnEndExpandCollapse(this);
            _expanderColumn.SetModelIsExpanded(this);
        }

        private class ChildRows : SortableRowsBase<TModel, HierarchicalRow<TModel>>,
            IReadOnlyList<HierarchicalRow<TModel>>
        {
            private readonly HierarchicalRow<TModel> _owner;

            public ChildRows(
                HierarchicalRow<TModel> owner,
                TreeDataGridItemsSourceView<TModel> items,
                Comparison<TModel>? comparison)
                : base(items, comparison)
            {
                _owner = owner;
                CollectionChanged += OnCollectionChanged;
            }

            protected override HierarchicalRow<TModel> CreateRow(int modelIndex, TModel model)
            {
                return new HierarchicalRow<TModel>(
                    _owner._controller,
                    _owner._expanderColumn,
                    _owner.ModelIndexPath.Append(modelIndex),
                    model,
                    _owner._comparison);
            }

            private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                if (_owner.IsExpanded)
                    _owner._controller.OnChildCollectionChanged(_owner, e);
            }
        }
    }
}
