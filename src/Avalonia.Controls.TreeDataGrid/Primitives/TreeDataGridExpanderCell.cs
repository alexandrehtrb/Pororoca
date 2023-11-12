using System;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridExpanderCell : TreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridExpanderCell, int> IndentProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, int>(
                nameof(Indent),
                o => o.Indent);

        public static readonly DirectProperty<TreeDataGridExpanderCell, bool> IsExpandedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, bool>(
                nameof(IsExpanded),
                o => o.IsExpanded,
                (o, v) => o.IsExpanded = v);

        public static readonly DirectProperty<TreeDataGridExpanderCell, bool> ShowExpanderProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, bool>(
                nameof(ShowExpander),
                o => o.ShowExpander);

        private Decorator? _contentContainer;
        private Type? _contentType;
        private TreeDataGridElementFactory? _factory;
        private int _indent;
        private bool _isExpanded;
        private IExpanderCell? _model;
        private bool _showExpander;

        public int Indent
        {
            get => _indent;
            private set => SetAndRaise(IndentProperty, ref _indent, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { if (_model is object) _model.IsExpanded = value; }
        }

        public bool ShowExpander
        {
            get => _showExpander;
            private set => SetAndRaise(ShowExpanderProperty, ref _showExpander, value);
        }

        public override void Realize(
            TreeDataGridElementFactory factory,
            ITreeDataGridSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            if (_model is object)
                throw new InvalidOperationException("Cell is already realized.");

            if (model is IExpanderCell expanderModel)
            {
                _factory = factory;
                _model = expanderModel;
                Indent = (_model.Row as IIndentedRow)?.Indent ?? 0;
                ShowExpander = _model.ShowExpander;

                // We can't go via the `IsExpanded` property here as that contains the implementation
                // for changing the expanded state by user action; it signals to the model that the
                // state is changed but here we need to update our state from the model.
                SetAndRaise(IsExpandedProperty, ref _isExpanded, _model.IsExpanded);

                if (expanderModel is INotifyPropertyChanged inpc)
                    inpc.PropertyChanged += ModelPropertyChanged;
            }
            else
            {
                throw new InvalidOperationException("Invalid cell model.");
            }

            base.Realize(factory, selection, model, columnIndex, rowIndex);
            UpdateContent(_factory);
        }

        public override void Unrealize()
        {
            if (_model is INotifyPropertyChanged inpc)
                inpc.PropertyChanged -= ModelPropertyChanged;
            _model = null;
            base.Unrealize();
            if (_factory is object)
                UpdateContent(_factory);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _contentContainer = e.NameScope.Find<Decorator>("PART_Content");
            if (_factory is object)
                UpdateContent(_factory);
        }

        private void UpdateContent(TreeDataGridElementFactory factory)
        {
            if (_contentContainer is null)
                return;

            if (_model?.Content is ICell innerModel)
            {
                var contentType = innerModel.GetType();

                if (contentType != _contentType)
                {
                    var element = factory.GetOrCreateElement(innerModel, this);
                    element.IsVisible = true;
                    _contentContainer.Child = element;
                    _contentType = contentType;
                }

                if (_contentContainer.Child is ITreeDataGridCell innerCell)
                    innerCell.Realize(factory, null, innerModel, ColumnIndex, RowIndex);
            }
            else if (_contentContainer.Child is ITreeDataGridCell innerCell)
            {
                innerCell.Unrealize();
            }
        }

        private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_model is null)
                return;

            if (e.PropertyName == nameof(_model.IsExpanded))
                SetAndRaise(IsExpandedProperty, ref _isExpanded, _model.IsExpanded);
            if (e.PropertyName == nameof(_model.ShowExpander))
                ShowExpander = _model.ShowExpander;
        }
    }
}
