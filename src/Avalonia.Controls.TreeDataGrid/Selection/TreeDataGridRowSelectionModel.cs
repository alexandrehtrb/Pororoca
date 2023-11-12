using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Selection
{
    public class TreeDataGridRowSelectionModel<TModel> : TreeSelectionModelBase<TModel>,
        ITreeDataGridRowSelectionModel<TModel>,
        ITreeDataGridSelectionInteraction
        where TModel : class
    {
        private static readonly Point s_InvalidPoint = new(double.NegativeInfinity, double.NegativeInfinity);
        private readonly ITreeDataGridSource<TModel> _source;
        private EventHandler? _viewSelectionChanged;
        private Point _pressedPoint = s_InvalidPoint;
        private bool _raiseViewSelectionChanged;
        private int _lastCharPressedTime;
        private string _typedWord = "";

        public TreeDataGridRowSelectionModel(ITreeDataGridSource<TModel> source)
            : base(source.Items)
        {
            _source = source;
            SelectionChanged += (s, e) =>
            {
                if (!IsSourceCollectionChanging)
                    _viewSelectionChanged?.Invoke(this, e);
                else
                    _raiseViewSelectionChanged = true;
            };
        }

        event EventHandler? ITreeDataGridSelectionInteraction.SelectionChanged
        {
            add => _viewSelectionChanged += value;
            remove => _viewSelectionChanged -= value;
        }

        IEnumerable? ITreeDataGridSelection.Source
        {
            get => Source;
            set => Source = value;
        }

        bool ITreeDataGridSelectionInteraction.IsRowSelected(IRow rowModel)
        {
            if (rowModel is IModelIndexableRow indexable)
                return IsSelected(indexable.ModelIndexPath);
            return false;
        }

        bool ITreeDataGridSelectionInteraction.IsRowSelected(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < _source.Rows.Count)
            {
                if (_source.Rows[rowIndex] is IModelIndexableRow indexable)
                    return IsSelected(indexable.ModelIndexPath);
            }

            return false;
        }

        void ITreeDataGridSelectionInteraction.OnKeyDown(TreeDataGrid sender, KeyEventArgs e)
        {
            if (sender.RowsPresenter is null)
                return;

            if (!e.Handled)
            {
                var direction = e.Key.ToNavigationDirection();
                var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
                var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

                if (direction.HasValue)
                {
                    var anchorRowIndex = _source.Rows.ModelIndexToRowIndex(AnchorIndex);

                    sender.RowsPresenter.BringIntoView(anchorRowIndex);

                    var anchor = sender.TryGetRow(anchorRowIndex);

                    if (anchor is not null && !ctrl)
                    {
                        e.Handled = TryKeyExpandCollapse(sender, direction.Value, anchor);
                    }

                    if (!e.Handled && (!ctrl || shift))
                    {
                        e.Handled = MoveSelection(sender, direction.Value, shift, anchor);
                    }

                    if (!e.Handled && direction == NavigationDirection.Left
                        && anchor?.Rows is HierarchicalRows<TModel> hierarchicalRows && anchorRowIndex > 0)
                    {
                        var newIndex = hierarchicalRows.GetParentRowIndex(AnchorIndex);
                        UpdateSelection(sender, newIndex, true);
                        FocusRow(sender, sender.RowsPresenter.BringIntoView(newIndex));
                    }

                    if (!e.Handled && direction == NavigationDirection.Right
                       && anchor?.Rows is HierarchicalRows<TModel> hierarchicalRows2 && hierarchicalRows2[anchorRowIndex].IsExpanded)
                    {
                        var newIndex = anchorRowIndex + 1;
                        UpdateSelection(sender, newIndex, true);
                        sender.RowsPresenter.BringIntoView(newIndex);
                    }
                }
            }
        }

        protected void HandleTextInput(string? text, TreeDataGrid treeDataGrid, int selectedRowIndex)
        {
            if (text != null && treeDataGrid.Columns != null)
            {
                var typedChar = text.ToUpper()[0];

                int now = Environment.TickCount;
                int time = 0;
                if (_lastCharPressedTime > 0)
                {
                    time = now - _lastCharPressedTime;
                }

                string candidatePattern;
                if (time < 500)
                {
                    if (_typedWord.Length == 1 && typedChar == _typedWord[0])
                    {
                        candidatePattern = _typedWord;
                    }
                    else
                    {
                        candidatePattern = _typedWord + typedChar;
                    }
                }
                else
                {
                    candidatePattern = typedChar.ToString();
                }
                foreach (var column in treeDataGrid.Columns)
                {
                    if (column is ITextSearchableColumn<TModel> textSearchableColumn && textSearchableColumn.IsTextSearchEnabled)
                    {
                        Search(treeDataGrid, candidatePattern, selectedRowIndex, textSearchableColumn);
                    }
                    else if (column is HierarchicalExpanderColumn<TModel> hierarchicalColumn &&
                        hierarchicalColumn.Inner is ITextSearchableColumn<TModel> textSearchableColumn2 &&
                        textSearchableColumn2.IsTextSearchEnabled)
                    {
                        Search(treeDataGrid, candidatePattern, selectedRowIndex, textSearchableColumn2);
                    }

                }
                _lastCharPressedTime = now;
            }
        }

        private void Search(TreeDataGrid treeDataGrid, string candidatePattern, int selectedRowIndex, ITextSearchableColumn<TModel> column)
        {
            var found = false;
            for (int i = candidatePattern.Length == 1 ? selectedRowIndex + 1 : selectedRowIndex; i <= _source.Rows.Count - 1; i++)
            {
                found = SearchAndSelectRow(treeDataGrid, candidatePattern, i, (TModel?)_source.Rows[i].Model, column.SelectValue);
                if (found)
                {
                    break;
                }
            }
            if (!found)
            {
                for (int i = 0; i <= selectedRowIndex; i++)
                {
                    found = SearchAndSelectRow(treeDataGrid, candidatePattern, i, (TModel?)_source.Rows[i].Model, column.SelectValue);
                    if (found)
                    {
                        break;
                    }
                }
            }
        }

        void ITreeDataGridSelectionInteraction.OnPreviewKeyDown(TreeDataGrid sender, KeyEventArgs e)
        {

            static bool IsRowFullyVisibleToUser(TreeDataGridRow row)
            {
                var scrollContentPresenter = row.FindAncestorOfType<ScrollContentPresenter>();
                if (scrollContentPresenter != null)
                {
                    var transform = row.TransformToVisual(scrollContentPresenter);
                    if (transform != null)
                    {
                        var transformedBounds = new Rect(row.Bounds.Size).TransformToAABB((Matrix)transform);
                        if (scrollContentPresenter.Bounds.Contains(transformedBounds.TopLeft) && scrollContentPresenter.Bounds.Contains(transformedBounds.BottomRight))
                        {
                            return true;
                        }
                    }
                }
                return false;

            }

            static bool GetRowIndexIfFullyVisible(Control? control, out int index)
            {
                if (control is TreeDataGridRow row &&
                    IsRowFullyVisibleToUser(row))
                {
                    index = row.RowIndex;
                    return true;
                }
                index = -1;
                return false;
            }

            void UpdateSelectionAndBringIntoView(int newIndex)
            {
                UpdateSelection(sender, newIndex, true);
                FocusRow(sender, sender.RowsPresenter?.BringIntoView(newIndex));
                e.Handled = true;
            }

            if ((e.Key == Key.PageDown || e.Key == Key.PageUp) && sender.RowsPresenter?.Items != null)
            {
                var children = sender.RowsPresenter.RealizedElements;
                var childrenCount = children.Count;
                if (childrenCount > 0)
                {
                    var newIndex = 0;
                    var isIndexSet = false;
                    int selectedIndex = sender.Rows!.ModelIndexToRowIndex(SelectedIndex);
                    if (e.Key == Key.PageDown)
                    {
                        for (int i = childrenCount - 1; i >= 0; i--)
                        {
                            if (GetRowIndexIfFullyVisible(children[i], out var index))
                            {
                                newIndex = index;
                                isIndexSet = true;
                                break;
                            }
                        }
                        if (isIndexSet &&
                            selectedIndex != newIndex &&
                            sender.TryGetRow(selectedIndex) is TreeDataGridRow row &&
                            IsRowFullyVisibleToUser(row))
                        {
                            UpdateSelectionAndBringIntoView(newIndex);
                            return;
                        }
                        else if (childrenCount + selectedIndex - 1 <= sender.RowsPresenter.Items.Count)
                        {
                            newIndex = childrenCount + selectedIndex - 2;
                        }
                        else
                        {
                            newIndex = sender.RowsPresenter.Items.Count - 1;
                        }
                    }
                    else if (e.Key == Key.PageUp)
                    {
                        for (int i = 0; i <= childrenCount - 1; i++)
                        {
                            if (GetRowIndexIfFullyVisible(children[i], out var index))
                            {
                                newIndex = index;
                                isIndexSet = true;
                                break;
                            }
                        }
                        if (isIndexSet &&
                            selectedIndex != newIndex &&
                            sender.TryGetRow(selectedIndex) is TreeDataGridRow row &&
                            IsRowFullyVisibleToUser(row))
                        {
                            UpdateSelectionAndBringIntoView(newIndex);
                            return;
                        }
                        else if (isIndexSet && selectedIndex - childrenCount + 2 > 0)
                        {
                            newIndex = selectedIndex - childrenCount + 2;
                        }
                        else
                        {
                            newIndex = 0;
                        }
                    }
                    UpdateSelectionAndBringIntoView(newIndex);
                }
            }
        }

        private bool SearchAndSelectRow(TreeDataGrid treeDataGrid,
            string candidatePattern, int newIndex, TModel? model, Func<TModel, string?>? valueSelector)
        {
            if (valueSelector != null && model != null)
            {
                var value = valueSelector(model);
                if (value != null && value.ToUpper().StartsWith(candidatePattern))
                {
                    UpdateSelection(treeDataGrid, newIndex, true);
                    treeDataGrid.RowsPresenter?.BringIntoView(newIndex);
                    _typedWord = candidatePattern;
                    return true;
                }
            }
            return false;
        }

        void ITreeDataGridSelectionInteraction.OnPointerPressed(TreeDataGrid sender, PointerPressedEventArgs e)
        {
            // Select a row on pointer pressed if:
            //
            // - It's a mouse click, not touch: we don't want to select on touch scroll gesture start
            // - The row isn't already selected: we don't want to deselect an existing multiple selection
            //   if the user is trying to drag multiple rows
            //
            // Otherwise select on pointer release.
            if (!e.Handled &&
                e.Pointer.Type == PointerType.Mouse &&
                e.Source is Control source &&
                sender.TryGetRow(source, out var row) &&
                _source.Rows.RowIndexToModelIndex(row.RowIndex) is { } modelIndex &&
                !IsSelected(modelIndex))
            {
                PointerSelect(sender, row, e);
                _pressedPoint = s_InvalidPoint;
            }
            else
            {
                _pressedPoint = e.GetPosition(sender);
            }
        }

        void ITreeDataGridSelectionInteraction.OnPointerReleased(TreeDataGrid sender, PointerReleasedEventArgs e)
        {
            if (!e.Handled &&
                _pressedPoint != s_InvalidPoint &&
                e.Source is Control source &&
                sender.TryGetRow(source, out var row))
            {
                var p = e.GetPosition(sender);
                if (Math.Abs(p.X - _pressedPoint.X) <= 3 || Math.Abs(p.Y - _pressedPoint.Y) <= 3)
                    PointerSelect(sender, row, e);
            }
        }

        void ITreeDataGridSelectionInteraction.OnTextInput(TreeDataGrid sender, TextInputEventArgs e)
        {
            HandleTextInput(e.Text, sender, _source.Rows.ModelIndexToRowIndex(AnchorIndex));
        }

        protected internal override IEnumerable<TModel>? GetChildren(TModel node)
        {
            if (_source is HierarchicalTreeDataGridSource<TModel> treeSource)
            {
                return treeSource.GetModelChildren(node);
            }

            return null;
        }

        protected internal override bool TryGetItemAt(IndexPath index, out TModel? result)
        {
            if (_source is FlatTreeDataGridSource<TModel> treeSource)
            {
                bool valid = index.Any(i => i >= treeSource.Items.Count()) == false;
                result = valid ? treeSource.Items.ElementAt(index[0]) : null;
                return valid;
            }

            var items = (IReadOnlyList<TModel>?)Root.ItemsView;
            var count = index.Count;

            for (var i = 0; i < count; ++i)
            {
                if (items is null)
                {
                    result = default;
                    return false;
                }

                var j = index[i];

                if (j < items.Count)
                {
                    if (i == count - 1)
                    {
                        result = items[j];
                        return true;
                    }
                    else
                        items = GetChildren(items[j]) as IReadOnlyList<TModel>;
                }
            }

            result = default;
            return false;
        }

        protected override void OnSourceCollectionChangeFinished()
        {
            if (_raiseViewSelectionChanged)
            {
                _viewSelectionChanged?.Invoke(this, EventArgs.Empty);
                _raiseViewSelectionChanged = false;
            }
        }

        private void PointerSelect(TreeDataGrid sender, TreeDataGridRow row, PointerEventArgs e)
        {
            var point = e.GetCurrentPoint(sender);

            var commandModifiers = TopLevel.GetTopLevel(sender)?.PlatformSettings?.HotkeyConfiguration.CommandModifiers;
            var toggleModifier = commandModifiers is not null && e.KeyModifiers.HasFlag(commandModifiers);
            var isRightButton = point.Properties.PointerUpdateKind is PointerUpdateKind.RightButtonPressed or
                PointerUpdateKind.RightButtonReleased;

            UpdateSelection(
                sender,
                row.RowIndex,
                select: true,
                rangeModifier: e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                toggleModifier: toggleModifier,
                rightButton: isRightButton);
            e.Handled = true;
        }

        private void UpdateSelection(
            TreeDataGrid treeDataGrid,
            int rowIndex,
            bool select = true,
            bool rangeModifier = false,
            bool toggleModifier = false,
            bool rightButton = false)
        {
            var modelIndex = _source.Rows.RowIndexToModelIndex(rowIndex);

            if (modelIndex == default)
                return;

            var mode = SingleSelect ? SelectionMode.Single : SelectionMode.Multiple;
            var multi = (mode & SelectionMode.Multiple) != 0;
            var toggle = (toggleModifier || (mode & SelectionMode.Toggle) != 0);
            var range = multi && rangeModifier;

            if (!select)
            {
                if (IsSelected(modelIndex) && !treeDataGrid.QueryCancelSelection())
                    Deselect(modelIndex);
            }
            else if (rightButton)
            {
                if (IsSelected(modelIndex) == false && !treeDataGrid.QueryCancelSelection())
                    SelectedIndex = modelIndex;
            }
            else if (range)
            {
                if (!treeDataGrid.QueryCancelSelection())
                {
                    var anchor = RangeAnchorIndex;
                    var i = Math.Max(_source.Rows.ModelIndexToRowIndex(anchor), 0);
                    var step = i < rowIndex ? 1 : -1;

                    using (BatchUpdate())
                    {
                        Clear();

                        while (true)
                        {
                            var m = _source.Rows.RowIndexToModelIndex(i);
                            Select(m);
                            anchor = m;
                            if (i == rowIndex)
                                break;
                            i += step;
                        }
                    }
                }
            }
            else if (multi && toggle)
            {
                if (!treeDataGrid.QueryCancelSelection())
                {
                    if (IsSelected(modelIndex) == true)
                        Deselect(modelIndex);
                    else
                        Select(modelIndex);
                }
            }
            else if (toggle)
            {
                if (!treeDataGrid.QueryCancelSelection())
                    SelectedIndex = (SelectedIndex == rowIndex) ? -1 : modelIndex;
            }
            else if (SelectedIndex != modelIndex || Count > 1)
            {
                if (!treeDataGrid.QueryCancelSelection())
                    SelectedIndex = modelIndex;
            }
        }

        private bool TryKeyExpandCollapse(
            TreeDataGrid treeDataGrid,
            NavigationDirection direction,
            TreeDataGridRow focused)
        {
            if (treeDataGrid.RowsPresenter is null || focused.RowIndex < 0)
                return false;

            var row = _source.Rows[focused.RowIndex];

            if (row is IExpander expander)
            {
                if (direction == NavigationDirection.Right && !expander.IsExpanded)
                {
                    expander.IsExpanded = true;
                    return true;
                }
                else if (direction == NavigationDirection.Left && expander.IsExpanded)
                {
                    expander.IsExpanded = false;
                    return true;
                }
            }

            return false;
        }

        private bool MoveSelection(
            TreeDataGrid treeDataGrid,
            NavigationDirection direction,
            bool rangeModifier,
            TreeDataGridRow? focused)
        {
            if (treeDataGrid.RowsPresenter is null || _source.Columns.Count == 0 || _source.Rows.Count == 0)
                return false;

            var currentRowIndex = focused?.RowIndex ?? _source.Rows.ModelIndexToRowIndex(SelectedIndex);
            int newRowIndex;

            if (direction == NavigationDirection.First || direction == NavigationDirection.Last)
            {
                newRowIndex = direction == NavigationDirection.First ? 0 : _source.Rows.Count - 1;
            }
            else
            {
                (var x, var y) = direction switch
                {
                    NavigationDirection.Up => (0, -1),
                    NavigationDirection.Down => (0, 1),
                    NavigationDirection.Left => (-1, 0),
                    NavigationDirection.Right => (1, 0),
                    _ => (0, 0)
                };

                newRowIndex = Math.Max(0, Math.Min(currentRowIndex + y, _source.Rows.Count - 1));
            }

            if (newRowIndex != currentRowIndex)
                UpdateSelection(treeDataGrid, newRowIndex, true, rangeModifier);

            if (newRowIndex != currentRowIndex)
            {
                treeDataGrid.RowsPresenter?.BringIntoView(newRowIndex);
                FocusRow(treeDataGrid, treeDataGrid.TryGetRow(newRowIndex));
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void FocusRow(TreeDataGrid owner, Control? control)
        {
            if (!owner.TryGetRow(control, out var row) || row.CellsPresenter is null)
                return;

            // Get the column index of the currently focused cell if possible: we'll try to focus the
            // same column in the new row.
            if (TopLevel.GetTopLevel(owner)?.FocusManager is { } focusManager &&
                focusManager.GetFocusedElement() is Control currentFocus &&
                owner.TryGetCell(currentFocus, out var currentCell) &&
                row.TryGetCell(currentCell.ColumnIndex) is { }  newCell &&
                newCell.Focusable)
            {
                newCell.Focus();
            }
            else
            {
                // Otherwise, just focus the first focusable cell in the row.
                foreach (var cell in row.CellsPresenter.GetRealizedElements())
                {
                    if (cell.Focusable)
                    {
                        cell.Focus();
                        break;
                    }
                }
            }
        }
    }
}
