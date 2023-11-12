using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Input;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridCheckBoxCell : TreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridCheckBoxCell, bool> IsReadOnlyProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCheckBoxCell, bool>(
                nameof(IsReadOnly),
                o => o.IsReadOnly,
                (o, v) => o.IsReadOnly = v);

        public static readonly DirectProperty<TreeDataGridCheckBoxCell, bool> IsThreeStateProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCheckBoxCell, bool>(
                nameof(IsThreeState),
                o => o.IsThreeState,
                (o, v) => o.IsThreeState = v);

        public static readonly DirectProperty<TreeDataGridCheckBoxCell, bool?> ValueProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCheckBoxCell, bool?>(
                nameof(Value),
                o => o.Value,
                (o, v) => o.Value = v);

        private bool _isReadOnly;
        private bool _isThreeState;
        private bool? _value;

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => SetAndRaise(IsReadOnlyProperty, ref _isReadOnly, value);
        }

        public bool IsThreeState
        {
            get => _isThreeState;
            set => SetAndRaise(IsThreeStateProperty, ref _isThreeState, value);
        }

        public bool? Value
        {
            get => _value;
            set
            {
                if (SetAndRaise(ValueProperty, ref _value, value) && Model is CheckBoxCell cell)
                    cell.Value = value;
            }
        }

        public override void Realize(
            TreeDataGridElementFactory factory,
            ITreeDataGridSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            if (model is CheckBoxCell cell)
            {
                IsReadOnly = cell.IsReadOnly;
                IsThreeState = cell.IsThreeState;
                Value = cell.Value;
            }
            else
            {
                throw new InvalidOperationException("Invalid cell model.");
            }

            base.Realize(factory, selection, model, columnIndex, rowIndex);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
        }
    }
}
