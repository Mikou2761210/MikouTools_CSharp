using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    public class WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue> : ConcurrentMultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, WPFMultiLevelCascadeCollection<FilterKey, ItemValue>, WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>>, INotifyCollectionChanged
               where FilterKey : notnull
               where ItemValue : notnull
    {
        private readonly ManualResetEventSlim _loadingWaitEvent = new(false);

        private bool _suppressNotification = false;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressNotification)
                return;

            // If the UI context has not been captured, call it directly.
            if (UIContext == null || SynchronizationContext.Current == UIContext)
            {
                CollectionChanged?.Invoke(this, e);
            }
            else
            {
                // If the current thread is not a UI thread, post to the UI context.
                UIContext.Send(_ => CollectionChanged?.Invoke(this, e), null);
            }
        }

        /// <summary>
        /// Assign a UI thread context to update the UI after an asynchronous operation
        /// </summary>
        public SynchronizationContext? UIContext;


        /// <summary>
        /// Suppress notifications during multiple updates
        /// </summary>
        public void BeginBulkUpdate() => _suppressNotification = true;

        /// <summary>
        /// Cancels notification suppression and issues a Reset notification
        /// </summary>
        public void EndBulkUpdate()
        {
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private WPFMultiLevelCascadeCollection<FilterKey, ItemValue> _base;
        internal WPFMultiLevelCascadeFilteredView(WPFMultiLevelCascadeCollection<FilterKey, ItemValue> @base, WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null) : base(@base, parent)
        {
            _base = @base;
            base.Initialize(filter, comparer);
            _loadingWaitEvent.Set();
        }


        public void WaitForInitialization()
        {
            _loadingWaitEvent.Wait();
        }

        protected override WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(WPFMultiLevelCascadeCollection<FilterKey, ItemValue> @base, WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>(@base, parent, filter, comparer);
        }


        internal int NotifyChildrenOfReplace(int id, ItemValue newValue, ItemValue oldValue)
        {
            WaitForInitialization();
            lock (_lock)
            {
                int index = _idList.IndexOf(id);
                if (index != -1)
                {

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newValue, oldValue, index));
                    foreach (var child in _children)
                    {
                        child.Value.NotifyChildrenOfReplace(id, newValue, oldValue);
                    }
                    return index;
                }
                return -1;
            }
        }

        public new IEnumerator<ItemValue> GetEnumerator()
        {
            WaitForInitialization();
            return base.GetEnumerator();
        }

        public new int Count
        {
            get
            {
                WaitForInitialization();
                return base.Count;
            }
        }

        public new ItemValue this[int index]
        {
            get
            {
                WaitForInitialization();
                return base[index];
            }
            set
            {
                WaitForInitialization();
                base[index] = value;
            }
        }

        internal new bool Add(int id)
        {
            WaitForInitialization();
            lock (_lock)
            {
                if (BaseThis.Add(id))
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _base[id], base.Count - 1));
                    return true;
                }
                return false;
            }
        }
        internal new bool AddRange(IEnumerable<int> ids)
        {
            WaitForInitialization();
            if (base.AddRange(ids))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return true;
            }
            return false;
        }

        internal new int InsertItemInOrder(int id)
        {
            WaitForInitialization();

            lock (_lock)
            {
                int index = BaseThis.InsertItemInOrder(id);
                if (index != -1)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _base[id], index));
                    return index;
                }
                return index;
            }
        }

        internal new bool Remove(int id)
        {
            WaitForInitialization();
            lock (_lock)
            {
                int index = _idList.IndexOf(id);
                if (index != -1)
                {
                    BaseThis.RemoveAt(index);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _base[id], index));
                    return true;
                }
                return false;
            }

        }

        public new int IndexOf(ItemValue item)
        {
            WaitForInitialization();
            return base.IndexOf(item);
        }

        public new int Move(int fromIndex, int toIndex)
        {
            WaitForInitialization();
            lock (_lock)
            {
                int result = BaseThis.Move(fromIndex, toIndex);
                if (result != -1)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, _base[_idList[result]], fromIndex, result));
                }
                return result;
            }
        }


        public new bool Sort(int index, int count, IComparer<ItemValue>? comparer)
        {
            WaitForInitialization();
            if(base.Sort(index, count, comparer))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return true;    
            }
            return false;
        }

        public new bool RedoLastSort()
        {
            WaitForInitialization();

            if (base.RedoLastSort())
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return true;
            }
            return false;
        }

        public new bool RedoLastSortRecursively()
        {
            WaitForInitialization();
            if (base.RedoLastSortRecursively())
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return true;
            }
            return false;
        }

        public new bool ChangeFilter(Func<ItemValue, bool>? filterFunc)
        {
            WaitForInitialization();
            if (base.ChangeFilter(filterFunc))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return true;
            }
            return false;
        }

        public new WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>? GetFilterView(FilterKey filterName)
        {
            WaitForInitialization();
            return base.GetFilterView(filterName);
        }

        public new WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue> AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            WaitForInitialization();
            return base.AddFilterView(filterName, filter, comparer);
        }


        public new void RemoveFilterView(FilterKey filterName)
        {
            WaitForInitialization();
            base.RemoveFilterView(filterName);
        }

        #region Async

        public new async Task<bool> ChangeFilterAsync(Func<ItemValue, bool>? filterFunc)
        {
            WaitForInitialization();
            return await Task.Run(() =>
            {
                bool result = base.ChangeFilter(filterFunc);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return result;
            }).ConfigureAwait(false);
        }
        public new async Task<WPFMultiLevelCascadeFilteredView<FilterKey,ItemValue>> AddFilterViewAsync(FilterKey filterName, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            WaitForInitialization();
            return await base.AddFilterViewAsync(filterName, filter, comparer);
        }

        public new async Task<bool> SortAsync(IComparer<ItemValue>? comparer)
        {
            WaitForInitialization();
            return await Task<bool>.Run(() =>
            {
                bool result = base.Sort(comparer);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return result;
            }).ConfigureAwait(false);
        }

        #endregion

    }
}
