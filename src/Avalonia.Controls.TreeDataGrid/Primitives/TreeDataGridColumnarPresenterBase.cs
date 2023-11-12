using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Layout;

namespace Avalonia.Controls.Primitives
{
    /// <summary>
    /// Base class for presenters which display data in virtualized columns.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <remarks>
    /// Implements common layout functionality between <see cref="TreeDataGridCellsPresenter"/>
    /// and <see cref="TreeDataGridColumnHeadersPresenter"/>.
    /// </remarks>
    public abstract class TreeDataGridColumnarPresenterBase<TItem> : TreeDataGridPresenterBase<TItem> 
    {
        protected IColumns? Columns => Items as IColumns;

        protected sealed override Size GetInitialConstraint(Control element, int index, Size availableSize)
        {
            var column = (IUpdateColumnLayout)Columns![index];
            return new Size(Math.Min(availableSize.Width, column.MaxActualWidth), availableSize.Height);
        }

        protected sealed override bool NeedsFinalMeasurePass(int firstIndex, IReadOnlyList<Control?> elements)
        {
            var columns = Columns!;

            columns.CommitActualWidths();

            // We need to do a second measure pass if any of the controls were measured with a width
            // that is greater than the final column width.
            for (var i = 0; i < elements.Count; i++)
            {
                var e = elements[i];
                if (e is not null)
                {
                    var previous = LayoutInformation.GetPreviousMeasureConstraint(e)!.Value;
                    if (previous.Width > columns[i + firstIndex].ActualWidth)
                        return true;
                }
            }

            return false;
        }

        protected sealed override (int index, double position) GetElementAt(double position)
        {
            return ((IColumns)Items!).GetColumnAt(position);
        }

        protected sealed override Size GetFinalConstraint(Control element, int index, Size availableSize)
        {
            var column = Columns![index];
            return new(column.ActualWidth, double.PositiveInfinity);
        }

        protected sealed override double CalculateSizeU(Size availableSize)
        {
            return Columns?.GetEstimatedWidth(availableSize.Width) ?? 0;
        }
    }
}
