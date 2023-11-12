using System;

namespace Avalonia.Controls.Selection
{
    public class TreeSelectionModelSourceResetEventArgs : EventArgs
    {
        public TreeSelectionModelSourceResetEventArgs(IndexPath parentIndex)
        {
            ParentIndex = parentIndex;
        }

        public IndexPath ParentIndex { get; }
    }
}
