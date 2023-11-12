using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public abstract class RowEventArgs : EventArgs
    {
        public IRow Row => GetUntypedRow();
        protected abstract IRow GetUntypedRow();

        public static RowEventArgs<T> Create<T>(T row) where T : IRow
        {
            return new RowEventArgs<T>(row);
        }
    }

    public class RowEventArgs<TRow> : RowEventArgs
        where TRow : IRow
    {
        public RowEventArgs(TRow row) => Row = row;
        public new TRow Row { get; }
        protected override IRow GetUntypedRow() => Row;
    }
}
