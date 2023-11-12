namespace Avalonia.Controls.Models.TreeDataGrid
{
    public interface ITextSearchableColumn<TModel>
    {
        public bool IsTextSearchEnabled { get; }
        internal string? SelectValue(TModel model);
    }
}
