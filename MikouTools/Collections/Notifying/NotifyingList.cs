using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Notifying
{
    public class NotifyingList<T> : Collection<T>, INotifyCollectionChanged
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

        #region Collection<T> override
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {
            T removedItem = this[index];
            base.RemoveItem(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem = this[index];
            base.SetItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion
    }

}
