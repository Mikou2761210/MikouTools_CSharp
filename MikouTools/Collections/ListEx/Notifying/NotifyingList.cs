using MikouTools.Collections.ListEx.Overrideable;
using MikouTools.Collections.WPF;
using System.Collections.Specialized;

namespace MikouTools.Collections.ListEx.Notifying
{
    public class NotifyingList<T> : OverrideableList<T>, IExtendNotifyCollectionChanged
    {
        #region UI
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        private bool _suppressNotification = false;
        public SynchronizationContext? UIContext;

        protected virtual internal void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressNotification)
                return;

            if (UIContext == null || SynchronizationContext.Current == UIContext)
            {
                CollectionChanged?.Invoke(this, e);
            }
            else
            {
                UIContext.Send(_ => CollectionChanged?.Invoke(this, e), null);
            }
        }


        public virtual void BeginBulkUpdate() => _suppressNotification = true;


        public virtual void EndBulkUpdate()
        {
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion

        #region List<T> override
        public override void Add(T item)
        {
            base.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Count - 1));
        }

        public override void Insert(int index, T item)
        {
            base.Insert(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public override bool Remove(T item)
        {
            int index = IndexOf(item);
            bool removed = base.Remove(item);
            if (removed)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
            return removed;
        }

        public override void RemoveAt(int index)
        {
            T removedItem = this[index];
            base.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
        }

        public override void Clear()
        {
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public override T this[int index]
        {
            get => base[index];
            set
            {
                T oldItem = base[index];
                base[index] = value;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
            }
        }


        public override void Sort(int index, int count, IComparer<T>? comparer)
        {
            base.Sort(index, count, comparer);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion
    }

}
