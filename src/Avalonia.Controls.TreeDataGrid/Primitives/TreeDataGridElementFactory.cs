using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridElementFactory
    {
        private readonly Dictionary<object, List<Control>>  _recyclePool = new();

        public Control GetOrCreateElement(object? data, Control parent)
        {
            var recycleKey = GetDataRecycleKey(data);

            if (_recyclePool.TryGetValue(recycleKey, out var elements) && elements.Count > 0)
            {
                // First look for an element with the same parent.
                for (var i = 0; i < elements.Count; i++)
                { 
                    var e = elements[i];

                    if (e.Parent == parent)
                    {
                        elements.RemoveAt(i);
                        return e;
                    }
                }

                // Next look for an element with no parent or an element that we can reparent.
                for (var i = 0; i < elements.Count; i++)
                {
                    var e = elements[i];
                    var parentPanel = e.Parent as Panel;

                    if (e.Parent is null || parentPanel is not null)
                    {
                        parentPanel?.Children.Remove(e);
                        Debug.Assert(e.Parent is null);
                        elements.RemoveAt(i);
                        return e;
                    }
                }
            }

            // Otherwise create a new element.
            return CreateElement(data);
        }

        public void RecycleElement(Control element)
        {
            var recycleKey = GetElementRecycleKey(element);

            if (!_recyclePool.TryGetValue(recycleKey, out var elements))
            {
                elements = new();
                _recyclePool.Add(recycleKey, elements);
            }

            elements.Add(element);
        }

        protected virtual Control CreateElement(object? data)
        {
            return data switch
            {
                CheckBoxCell => new TreeDataGridCheckBoxCell(),
                TemplateCell => new TreeDataGridTemplateCell(),
                IExpanderCell => new TreeDataGridExpanderCell(),
                ICell => new TreeDataGridTextCell(),
                IColumn => new TreeDataGridColumnHeader(),
                IRow => new TreeDataGridRow(),
                _ => throw new NotSupportedException(),
            };
        }

        protected virtual string GetDataRecycleKey(object? data)
        {
            return data switch
            {
                CheckBoxCell => typeof(TreeDataGridCheckBoxCell).FullName!,
                TemplateCell => typeof(TreeDataGridTemplateCell).FullName!,
                IExpanderCell => typeof(TreeDataGridExpanderCell).FullName!,
                ICell => typeof(TreeDataGridTextCell).FullName!,
                IColumn => typeof(TreeDataGridColumnHeader).FullName!,
                IRow => typeof(TreeDataGridRow).FullName!,
                _ => throw new NotSupportedException(),
            };
        }

        protected virtual string GetElementRecycleKey(Control element)
        {
            return element.GetType().FullName!;
        }
    }
}
