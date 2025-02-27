namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{   
    public class AsyncMultiLevelCascadeCollection<FilterKey, ItemValue> : ConcurrentMultiLevelCascadeCollectionBase<FilterKey, ItemValue, AsyncMultiLevelCascadeCollection<FilterKey, ItemValue>, AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>> where FilterKey : notnull where ItemValue : notnull
    {

        protected override AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(AsyncMultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparer);
        }

        protected override AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(AsyncMultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            return new AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparison);
        }

        public override void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            base.AddFilterView(filterName, filter, comparer);
        }
        public override void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter, Comparison<ItemValue> comparison)
        {
            base.AddFilterView(filterName, filter, comparison);
        }
        public Task AddFilterViewAsync(FilterKey filterName, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            return Task.Run(() => base.AddFilterView(filterName, filter, comparer));
        }
        public Task AddFilterViewAsync(FilterKey filterName, Func<ItemValue, bool>? filter, Comparison<ItemValue> comparison)
        {
            return Task.Run(() => base.AddFilterView(filterName, filter, comparison));
        }
    }
}
