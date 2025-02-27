namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{

    public class AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue> : ConcurrentMultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, AsyncMultiLevelCascadeCollection<FilterKey, ItemValue>, AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>>
               where FilterKey : notnull
               where ItemValue : notnull
    {
        private readonly ManualResetEventSlim _loadingWaitEvent = new(false);


        internal AsyncMultiLevelCascadeFilteredView(AsyncMultiLevelCascadeCollection<FilterKey, ItemValue> @base, AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null) : base(@base, parent, filter, comparer)
        {
            _loadingWaitEvent.Set();
        }
        internal AsyncMultiLevelCascadeFilteredView(AsyncMultiLevelCascadeCollection<FilterKey, ItemValue> @base, AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null) : base(@base, parent, filter, comparison)
        {
            _loadingWaitEvent.Set();
        }


        public void WaitForInitialization()
        {
            _loadingWaitEvent.Wait();
        }

        protected override AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(AsyncMultiLevelCascadeCollection<FilterKey, ItemValue> @base, AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>(@base, parent, filter, comparer);
        }

        protected override AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(AsyncMultiLevelCascadeCollection<FilterKey, ItemValue> @base, AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            return new AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>(@base, parent, filter, comparison);
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
        }

        public override int IndexOf(ItemValue item)
        {
            WaitForInitialization();
            return base.IndexOf(item);
        }

        public override bool Sort()
        {
            WaitForInitialization();
            return base.Sort();
        }

        public override bool Sort(IComparer<ItemValue>? comparer)
        {
            WaitForInitialization();
            return base.Sort(comparer);
        }

        public override bool Sort(int index, int count, IComparer<ItemValue>? comparer)
        {
            WaitForInitialization();
            return base.Sort(index, count, comparer);
        }

        public override bool Sort(Comparison<ItemValue> comparison)
        {
            WaitForInitialization();
            return base.Sort(comparison);
        }

        internal override int AddRedoLastSort(int id)
        {
            WaitForInitialization();
            return base.AddRedoLastSort(id);
        }

        public override bool RedoLastSort()
        {
            WaitForInitialization();
            return base.RedoLastSort();
        }

        public override bool RedoLastSortRecursively()
        {
            WaitForInitialization();
            return base.RedoLastSortRecursively();
        }

        public override bool ChangeFilter(Func<ItemValue, bool>? filterFunc)
        {
            WaitForInitialization();
            return base.ChangeFilter(filterFunc);
        }

        public override AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>? GetFilterView(FilterKey filterName)
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
        public Task AddFilterViewAsync(FilterKey filterName, Func<ItemValue, bool>? filter, Comparison<ItemValue> comparison)
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
