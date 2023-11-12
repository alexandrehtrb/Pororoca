using System;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridTemplateCell : TreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridTemplateCell, object?> ContentProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTemplateCell, object?>(
                nameof(Content),
                x => x.Content);

        public static readonly DirectProperty<TreeDataGridTemplateCell, IDataTemplate?> ContentTemplateProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTemplateCell, IDataTemplate?>(
                nameof(ContentTemplate),
                x => x.ContentTemplate);

        public static readonly DirectProperty<TreeDataGridTemplateCell, IDataTemplate?> EditingTemplateProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTemplateCell, IDataTemplate?>(
                nameof(EditingTemplate),
                x => x.EditingTemplate);

        private object? _content;
        private IDataTemplate? _contentTemplate;
        private IDataTemplate? _editingTemplate;
        private ContentPresenter? _editingContentPresenter;

        public object? Content 
        { 
            get => _content;
            private set => SetAndRaise(ContentProperty, ref _content, value);
        }

        public IDataTemplate? ContentTemplate 
        { 
            get => _contentTemplate;
            set => SetAndRaise(ContentTemplateProperty, ref _contentTemplate, value);
        }

        public IDataTemplate? EditingTemplate
        {
            get => _editingTemplate;
            set => SetAndRaise(EditingTemplateProperty, ref _editingTemplate, value);
        }

        public override void Realize(
            TreeDataGridElementFactory factory,
            ITreeDataGridSelectionInteraction? selection, 
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            DataContext = model;
            base.Realize(factory, selection, model, columnIndex, rowIndex);
        }

        public override void Unrealize()
        {
            DataContext = null;
            base.Unrealize();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_editingContentPresenter is not null)
                _editingContentPresenter.LostFocus -= EditingContentPresenterLostFocus;

            _editingContentPresenter = e.NameScope.Find<ContentPresenter>("PART_EditingContentPresenter");

            if (_editingContentPresenter is not null)
            {
                _editingContentPresenter.UpdateChild();

                var focus = (IInputElement?)_editingContentPresenter.GetVisualDescendants()
                    .FirstOrDefault(x => (x as IInputElement)?.Focusable == true);
                focus?.Focus();

                _editingContentPresenter.LostFocus += EditingContentPresenterLostFocus;
            }
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);

            if (ContentTemplate is null && DataContext is TemplateCell cell)
            {
                ContentTemplate = cell.GetCellTemplate(this);
                EditingTemplate = cell.GetCellEditingTemplate?.Invoke(this);
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            var cell = DataContext as TemplateCell;

            // If DataContext is null, we're unrealized. Don't clear the content template for unrealized
            // cells because this will mean that when the cell is realized again the template will need
            // to be rebuilt, slowing everything down.
            if (cell is not null)
            {
                Content = cell.Value;

                if (((ILogical)this).IsAttachedToLogicalTree)
                {
                    ContentTemplate = cell.GetCellTemplate(this);
                    EditingTemplate = cell.GetCellEditingTemplate?.Invoke(this);
                }
            }
            else
            {
                Content = null;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (EndEditIfFocusLost())
            {
                base.OnLostFocus(e);
            }
        }

        private void EditingContentPresenterLostFocus(object? sender, RoutedEventArgs e) => EndEditIfFocusLost();

        private bool EndEditIfFocusLost()
        {
            if (TopLevel.GetTopLevel(this) is { } topLevel &&
                topLevel?.FocusManager?.GetFocusedElement() is Control newFocus &&
                !IsDescendent(newFocus))
            {
                EndEdit();
                return true;
            }

            return false;
        }

        private bool IsDescendent(Control c)
        {
            if (this.IsVisualAncestorOf(c))
                return true;

            // If the control is not a direct visual descendent, then check to make sure it's not
            // hosted in a popup that is a descendent of the cell.
            if (TopLevel.GetTopLevel(c)?.Parent is Control host)
                return this.IsVisualAncestorOf(host);

            return false;
        }
    }
}
