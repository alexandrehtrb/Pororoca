
using System;
using System.ComponentModel;
using Avalonia.Controls.Templates;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class TemplateCell : ICell, IEditableObject
    {
        private ITemplateCellOptions? _options;

        public TemplateCell(
            object? value,
            Func<Control, IDataTemplate> getCellTemplate,
            Func<Control, IDataTemplate>? getCellEditingTemplate,
            ITemplateCellOptions? options)
        {
            GetCellTemplate = getCellTemplate;
            GetCellEditingTemplate = getCellEditingTemplate;
            Value = value;
            _options = options;
        }

        public bool CanEdit => GetCellEditingTemplate is not null;
        public BeginEditGestures EditGestures => _options?.BeginEditGestures ?? BeginEditGestures.Default;
        public Func<Control, IDataTemplate> GetCellTemplate { get; }
        public Func<Control, IDataTemplate>? GetCellEditingTemplate { get; }
        public object? Value { get; }

        void IEditableObject.BeginEdit() => (Value as IEditableObject)?.BeginEdit();
        void IEditableObject.CancelEdit() => (Value as IEditableObject)?.CancelEdit();
        void IEditableObject.EndEdit() => (Value as IEditableObject)?.EndEdit();
    }
}
