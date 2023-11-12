using System;
using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A row that can be reused for situations where creating a separate object for each row is
    /// not necessary.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <remarks>
    /// In a flat grid where rows cannot be resized, it is not necessary to persist any information
    /// about rows; the same row object can be updated and reused when a new row is requested.
    /// </remarks>
    internal class AnonymousRow<TModel> : IRow<TModel>, IModelIndexableRow
    {
        private int _modelIndex;
        [AllowNull] private TModel _model;

        public object? Header => _modelIndex;
        public TModel Model => _model;
        public int ModelIndex => _modelIndex;
        public IndexPath ModelIndexPath => _modelIndex;

        public GridLength Height
        {
            get => GridLength.Auto;
            set { }
        }

        public AnonymousRow<TModel> Update(int modelIndex, TModel model)
        {
            _modelIndex = modelIndex;
            _model = model;
            return this;
        }

        public void UpdateModelIndex(int delta)
        {
            throw new NotSupportedException();
        }
    }
}
