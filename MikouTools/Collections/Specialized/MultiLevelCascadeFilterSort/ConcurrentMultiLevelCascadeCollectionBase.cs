using MikouTools.Collections.Optimized;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    public abstract class ConcurrentMultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered> : MultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered>
        where FilterKey : notnull
        where ItemValue : notnull
        where TCollection : MultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered>
        where TFiltered : MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {

        internal readonly object _lock = new();
        protected override DualKeyDictionary<int, ItemValue> CreateBaseItemDictionary()
        {
            return new ConcurrentDualKeyDictionary<int,ItemValue>();
        }
        protected override Dictionary<FilterKey, TFiltered> CreateChildViews()
        {
            return [];
        }
        protected override abstract TFiltered CreateChildCollection(TCollection @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null);

        public ConcurrentMultiLevelCascadeCollectionBase() : base() { }


        public new IEnumerable<int> GetIDs()
        {
            lock (_lock)
            {
                // スレッドセーフに列挙するため、スナップショットを返す
                return [.. base.GetIDs()];
            }
        }

        public new IEnumerable<ItemValue> GetValues()
        {
            lock (_lock)
            {
                return [.. base.GetValues()];
            }
        }

        public new ItemValue this[int id]
        {
            get
            {
                lock (_lock)
                {
                    return base[id];
                }
            }
            set
            {
                lock (_lock)
                {
                    base[id] = value;
                }
            }
        }

        public new int Add(ItemValue item)
        {
            lock (_lock)
            {
                return base.Add(item);
            }
        }

        public new void AddRange(IEnumerable<ItemValue> items)
        {
            lock (_lock)
            {
                base.AddRange(items);
            }
        }

        public new bool Remove(ItemValue item)
        {
            lock (_lock)
            {
                return base.Remove(item);
            }
        }

        public new bool RemoveId(int id)
        {
            lock (_lock)
            {
                return base.RemoveId(id);
            }
        }

        public new TFiltered? GetFilterView(FilterKey key)
        {
            lock (_lock)
            {
                return base.GetFilterView(key);
            }
        }

        public new void AddFilterView(FilterKey filterKey, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            lock (_lock)
            {
                base.AddFilterView(filterKey, filter, comparer);
            }
        }

        public new void RemoveFilterView(FilterKey key)
        {
            lock (_lock)
            {
                base.RemoveFilterView(key);
            }
        }

        #region Async

        public async Task<int> AddAsync(ItemValue item)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    return base.Add(item);
                }
            }).ConfigureAwait(false);
        }

        public async Task<bool> RemoveAsync(ItemValue item)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    return base.Remove(item);
                }
            }).ConfigureAwait(false);
        }

        public async Task AddRangeAsync(IEnumerable<ItemValue> items)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    base.AddRange(items);
                }
            }).ConfigureAwait(false);
        }

        public async Task<TFiltered> AddFilterViewAsync(FilterKey filterName, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    return base.AddFilterView(filterName, filter, comparer);
                }
            }).ConfigureAwait(false);

        }

        #endregion
    }

}
