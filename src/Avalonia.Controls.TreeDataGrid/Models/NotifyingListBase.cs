using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Models
{
    public class NotifyingListBase<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string IndexerName = "Item[]";
        private readonly SimpleMonitor _monitor = new();
        private BatchUpdateType _batchUpdate;

        public NotifyingListBase()
        {
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public void InsertRange(int index, Action<Action<T>> action)
        {
            CheckReentrancy();
            _batchUpdate = BatchUpdateType.Insert;

            var i = index;

            try
            {
                void DoInsert(T item)
                {
                    Insert(i, item);
                    ++i;
                }
                
                action(DoInsert);
            }
            finally
            {
                _batchUpdate = BatchUpdateType.None;
            }

            if (i > index)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
                using var monitor = BlockReentrancy();
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        new ListSpan(this, index, i - index),
                        index));
            }
        }

        public void RemoveRange(int index, int count)
        {
            if (count == 0)
                return;

            CheckReentrancy();
            _batchUpdate = BatchUpdateType.Remove;

            var removed = new T[count];

            try
            {
                for (var i = 0; i < count; ++i)
                    removed[i] = this[index + i];
                for (var i = 0; i < count; ++i)
                    RemoveAt(index);
            }
            finally
            {
                _batchUpdate = BatchUpdateType.None;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            using var monitor = BlockReentrancy();
            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    removed,
                    index));
        }

        public void Reset(Action<IList<T>> action)
        {
            CheckReentrancy();
            _batchUpdate = BatchUpdateType.Reset;

            try
            {
                action(this);
            }
            finally
            {
                _batchUpdate = BatchUpdateType.None;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            using var monitor = BlockReentrancy();
            CollectionChanged?.Invoke(this, TreeDataGrid.CollectionExtensions.ResetEvent);
        }

        protected override void ClearItems()
        {
            CheckReentrancy();
            base.ClearItems();

            if (_batchUpdate == BatchUpdateType.None)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
                using var monitor = BlockReentrancy();
                CollectionChanged?.Invoke(this, TreeDataGrid.CollectionExtensions.ResetEvent);
            }
            else if (_batchUpdate != BatchUpdateType.Reset)
            {
                throw new InvalidOperationException("Operation not permitted during a batch update.");
            }
        }

        protected override void RemoveItem(int index)
        {
            CheckReentrancy();
            
            var removedItem = this[index];

            base.RemoveItem(index);

            if (_batchUpdate == BatchUpdateType.None)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
                using var monitor = BlockReentrancy();
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
            }
            else if (_batchUpdate != BatchUpdateType.Reset && _batchUpdate != BatchUpdateType.Remove)
            {
                throw new InvalidOperationException("Operation not permitted during a batch update.");
            }
        }

        protected override void InsertItem(int index, T item)
        {
            CheckReentrancy();
            
            base.InsertItem(index, item);

            if (_batchUpdate == BatchUpdateType.None)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
                using var monitor = BlockReentrancy();
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
            else if (_batchUpdate != BatchUpdateType.Reset && _batchUpdate != BatchUpdateType.Insert)
            {
                throw new InvalidOperationException("Operation not permitted during a batch update.");
            }
        }

        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();
            
            var originalItem = this[index];
            base.SetItem(index, item);

            if (_batchUpdate == BatchUpdateType.None)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
                using var monitor = BlockReentrancy();
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, originalItem, item, index));
            }
            else if (_batchUpdate != BatchUpdateType.Reset)
            {
                throw new InvalidOperationException("Operation not permitted during a batch update.");
            }
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();
            
            var removedItem = this[oldIndex];

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);

            if (_batchUpdate == BatchUpdateType.None)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
                using var monitor = BlockReentrancy();
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex));
            }
            else if (_batchUpdate != BatchUpdateType.Reset)
            {
                throw new InvalidOperationException("Operation not permitted during a batch update.");
            }
        }

        private IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        private void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args
                // invalid for later listeners.  This keeps existing code working
                // (e.g. Selector.SelectedItems).
                if ((CollectionChanged != null) && (CollectionChanged.GetInvocationList().Length > 1))
                    throw new InvalidOperationException("NotifyingList reentrancy not allowed.");
            }
        }

        private class SimpleMonitor : IDisposable
        {
            private int _busyCount;
            public void Enter() => ++_busyCount;
            public void Dispose() => --_busyCount;
            public bool Busy => _busyCount > 0;
        }

        private enum BatchUpdateType
        {
            None,
            Insert,
            Remove,
            Reset,
        }
    }
}
