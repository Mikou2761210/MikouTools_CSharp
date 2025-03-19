using MikouTools.Collections.List.DirtySort;
using System.Collections;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    [Obsolete("This class was previously part of MikouTools but is now maintained as a standalone repository. It is recommended to use the new repository.")]

    public abstract class ConcurrentMultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered> : MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>, IEnumerable
        where FilterKey : notnull
        where ItemValue : notnull
        where TCollection : ConcurrentMultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered>
        where TFiltered : ConcurrentMultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        internal readonly object _lock = new();


        protected override DirtySortList<int> CreateFilteredIdList()
        {
            return new ConcurrentDirtySortList<int>();
        }

        protected override Dictionary<FilterKey, TFiltered> CreateChildViews()
        {
            return [];
        }
        protected override abstract TFiltered CreateChildCollection(TCollection @base, TFiltered? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null);
        internal ConcurrentMultiLevelCascadeFilteredViewBase(TCollection @base, TFiltered? parent = null) : base(@base, parent){ }

        public new IEnumerator<ItemValue> GetEnumerator()
        {
            lock (_lock)
            {
                // 列挙中にコレクションが変化しないよう、スナップショットの列挙子を返す
                return this.ToList().GetEnumerator();
            }
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public new virtual ItemValue this[int index]
        {
            get
            {
                lock (_lock)
                {
                    return base[index];
                }
            }
            set
            {
                lock (_lock)
                {
                    base[index] = value;
                }
            }
        }

        internal new virtual bool Add(int id)
        {
            lock (_lock)
            {
                return base.Add(id);
            }
        }

        internal new virtual bool AddRange(IEnumerable<int> ids)
        {
            lock (_lock)
            {
                return base.AddRange(ids);
            }
        }

        internal new int InsertItemInOrder(int id)
        {
            lock (_lock)
            {
                return base.InsertItemInOrder(id);
            }
        }

        internal new virtual bool Remove(int id)
        {
            lock (_lock)
            {
                return base.Remove(id);
            }
        }

        internal new virtual void RemoveAt(int index)
        {
            lock (_lock)
            {
                base.RemoveAt(index);
            }
        }

        public new virtual int IndexOf(ItemValue item)
        {
            lock (_lock)
            {
                return base.IndexOf(item);
            }
        }

        public new int Move(int fromIndex, int toIndex)
        {
            lock (_lock)
            {
                return base.Move(fromIndex, toIndex);
            }
        }

        public new bool Sort()
        {
            lock (_lock)
            {
                return base.Sort();
            }
        }

        public new bool Sort(IComparer<ItemValue>? comparer)
        {
            lock (_lock)
            {
                return base.Sort(comparer);
            }
        }

        public new virtual bool Sort(int index, int count, IComparer<ItemValue>? comparer)
        {
            lock (_lock)
            {
                return base.Sort(index, count, comparer);
            }
        }

        public new bool Sort(Comparison<ItemValue> comparison)
        {
            lock (_lock)
            {
                return base.Sort(comparison);
            }
        }


        public new bool RedoLastSort()
        {
            lock (_lock)
            {
                return base.RedoLastSort();
            }
        }

        public new bool RedoLastSortRecursively()
        {
            lock (_lock)
            {
                return base.RedoLastSortRecursively();
            }
        }

        public new virtual bool ChangeFilter(Func<ItemValue, bool>? filterFunc)
        {
            lock (_lock)
            {
                return base.ChangeFilter(filterFunc);
            }
        }

        public new TFiltered? GetFilterView(FilterKey filterName)
        {
            lock (_lock)
            {
                return base.GetFilterView(filterName);
            }
        }

        public new TFiltered AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            lock (_lock)
            {
                return base.AddFilterView(filterName, filter, comparer);
            }
        }

        public new void RemoveFilterView(FilterKey filterName)
        {
            lock (_lock)
            {
                base.RemoveFilterView(filterName);
            }
        }

        #region Async

        public async Task<bool> ChangeFilterAsync(Func<ItemValue, bool>? filterFunc)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    return base.ChangeFilter(filterFunc);
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

        public async Task<bool> SortAsync(IComparer<ItemValue>? comparer)
        {
            return await Task.Run(() =>
            {
                return Sort(comparer);
            }).ConfigureAwait(false);
        }

        #endregion
    }
}

