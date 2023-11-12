using System;

namespace Avalonia.Controls.Selection
{
    public class TreeSelectionModelIndexesChangedEventArgs : EventArgs
    {
        public TreeSelectionModelIndexesChangedEventArgs(IndexPath parentIndex, int startIndex, int delta)
        {
            ParentIndex = parentIndex;
            StartIndex = startIndex;
            Delta = delta;
        }

        public IndexPath ParentIndex { get; }
        public int StartIndex { get; }
        public int Delta { get; }
    }
}
