using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Observable collection supporting batch operations.
    /// Matches Silverlight's BatchObservableCollection(T).
    /// </summary>
    public class BatchObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification;

        /// <summary>Add multiple items, raising a single CollectionChanged event.</summary>
        public void AddRange(IEnumerable<T> items)
        {
            _suppressNotification = true;
            try
            {
                foreach (var item in items)
                    Items.Add(item);
            }
            finally
            {
                _suppressNotification = false;
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        /// <summary>Replace all items atomically.</summary>
        public void ReplaceAll(IEnumerable<T> items)
        {
            _suppressNotification = true;
            try
            {
                Items.Clear();
                foreach (var item in items)
                    Items.Add(item);
            }
            finally
            {
                _suppressNotification = false;
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        /// <summary>Remove multiple items, raising a single CollectionChanged event.</summary>
        public void RemoveRange(IEnumerable<T> items)
        {
            _suppressNotification = true;
            try
            {
                foreach (var item in items)
                    Items.Remove(item);
            }
            finally
            {
                _suppressNotification = false;
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }
    }

    /// <summary>
    /// Observable collection that adds items incrementally over multiple cycles.
    /// Matches Silverlight's GradualObservableCollection(T).
    /// </summary>
    public class GradualObservableCollection<T> : ObservableCollection<T>
    {
        private readonly Queue<T> _pendingItems = new Queue<T>();

        /// <summary>Number of items to add per cycle. Default 10.</summary>
        public int ItemsPerCycle { get; set; } = 10;

        /// <summary>Whether there are pending items to add.</summary>
        public bool HasPendingItems => _pendingItems.Count > 0;

        /// <summary>Number of items waiting to be added.</summary>
        public int PendingCount => _pendingItems.Count;

        /// <summary>Enqueue items for gradual addition.</summary>
        public void EnqueueRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                _pendingItems.Enqueue(item);
        }

        /// <summary>
        /// Process one cycle, adding up to ItemsPerCycle items.
        /// Returns true if there are still pending items.
        /// </summary>
        public bool ProcessCycle()
        {
            int count = Math.Min(ItemsPerCycle, _pendingItems.Count);
            for (int i = 0; i < count; i++)
            {
                Add(_pendingItems.Dequeue());
            }
            return _pendingItems.Count > 0;
        }

        /// <summary>Add all pending items at once.</summary>
        public void Flush()
        {
            while (_pendingItems.Count > 0)
                Add(_pendingItems.Dequeue());
        }
    }
}
