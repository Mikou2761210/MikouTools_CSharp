using System;
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
            if (!_suppressNotification)
            {
                CollectionChanged?.Invoke(this, e);
            }
        }


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

        internal WPFMultiLevelCascadeFilteredView(WPFMultiLevelCascadeCollection<FilterKey, ItemValue> @base, WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null) : base(@base, parent, filter, comparer)
        {
            _loadingWaitEvent.Set();
        }
        internal WPFMultiLevelCascadeFilteredView(WPFMultiLevelCascadeCollection<FilterKey, ItemValue> @base, WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null) : base(@base, parent, filter, comparison)
        {
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

        protected override WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(WPFMultiLevelCascadeCollection<FilterKey, ItemValue> @base, WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            return new WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>(@base, parent, filter, comparison);
        }

        internal int NotifyChildrenOfReplace(int id, ItemValue newValue, ItemValue oldValue)
        {
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

        public override IEnumerator<ItemValue> GetEnumerator()
        {
            WaitForInitialization();
            return base.GetEnumerator();
        }

        public override int Count
        {
            get
            {
                WaitForInitialization();
                return base.Count;
            }
        }

        public override ItemValue this[int index]
        {
            get
            {
                WaitForInitialization();
                return base[index];
            }
            set
            {
                base[index] = value;
            }
        }

        internal override bool Add(int id)
        {
            if (base.Add(id))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, base[id], base.Count - 1));
                return true;
            }
            return false;
        }
        internal override bool AddRange(IEnumerable<int> ids)
        {
            if (base.AddRange(ids))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return true;
            }
            return false;
        }

        internal override bool Remove(int id)
        {
            int index = _idList.IndexOf(id);
            if (index != -1)
            {
                base.RemoveAt(index);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, base[id], index));
                return true;
            }
            return false;
        }

        public override int IndexOf(ItemValue item)
        {
            WaitForInitialization();
            return base.IndexOf(item);
        }


        public override bool Sort(int index, int count, IComparer<ItemValue>? comparer)
        {
            WaitForInitialization();
            if(base.Sort(index, count, comparer))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return true;    
            }
            return false;
        }
        internal override int AddRedoLastSort(int id)
        {
            WaitForInitialization();
            int index = base.AddRedoLastSort(id);
            if (index != -1)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return index;
            }
            return index;
        }

        public override bool RedoLastSort()
        {
            WaitForInitialization();

            if (base.RedoLastSort())
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return true;
            }
            return false;
        }

        public override bool RedoLastSortRecursively()
        {
            WaitForInitialization();
            return base.RedoLastSortRecursively();
        }

        public override bool ChangeFilter(Func<ItemValue, bool>? filterFunc)
        {
            WaitForInitialization();
            if (base.ChangeFilter(filterFunc))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return true;
            }
            return false;
        }

        public override WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>? GetFilterView(FilterKey filterName)
        {
            WaitForInitialization();
            return base.GetFilterView(filterName);
        }

        public override void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            WaitForInitialization();
            base.AddFilterView(filterName, filter, comparer);
        }
        public override void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter, Comparison<ItemValue> comparison)
        {
            WaitForInitialization();
            base.AddFilterView(filterName, filter, comparison);
        }
        public Task AddFilterViewAsync(FilterKey filterName, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            WaitForInitialization();
            return Task.Run(() => base.AddFilterView(filterName, filter, comparer));
        }
        public Task AddFilterViewAsync(FilterKey filterName, Func<ItemValue, bool>? filter , Comparison<ItemValue> comparison)
        {
            WaitForInitialization();
            return Task.Run(() => base.AddFilterView(filterName, filter, comparison));
        }

        public override void RemoveFilterView(FilterKey filterName)
        {
            WaitForInitialization();
            base.RemoveFilterView(filterName);
        }


    }
}
