namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{   
    /// <summary>
    /// The current structure of MultiLevelCascadeFilteredViewBase cannot be made thread-safe. It may simply be that my technical skills and knowledge are insufficient, but in order to implement thread safety, we have no choice but to either completely change the structure of MultiLevelCascadeFilteredViewBase or create a thread-safe version of it. Since there are no plans to use it at the moment, development has been halted.
    /// </summary>
    /// <typeparam name="FilterKey"></typeparam>
    /// <typeparam name="ItemValue"></typeparam>
    [ObsoleteAttribute("The current structure of MultiLevelCascadeFilteredViewBase cannot be made thread-safe. It may simply be that my technical skills and knowledge are insufficient, but in order to implement thread safety, we have no choice but to either completely change the structure of MultiLevelCascadeFilteredViewBase or create a thread-safe version of it. Since there are no plans to use it at the moment, development has been halted.", false)]
    public class AsyncMultiLevelCascadeCollection<FilterKey, ItemValue> : MultiLevelCascadeCollectionBase<FilterKey, ItemValue, AsyncMultiLevelCascadeCollection<FilterKey, ItemValue>, AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>> where FilterKey : notnull where ItemValue : notnull
    {

        protected override AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(AsyncMultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparer);
        }

        protected override AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(AsyncMultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            return new AsyncMultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparison);
        }

        public override void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            base.AddFilterView(filterName, filter, comparer);
        }
        public override void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            base.AddFilterView(filterName, filter, comparison);
        }
        public Task AddFilterViewAsync(FilterKey filterName, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return Task.Run(() => base.AddFilterView(filterName, filter, comparer));
        }
        public Task AddFilterViewAsync(FilterKey filterName, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            return Task.Run(() => base.AddFilterView(filterName, filter, comparison));
        }
    }
}
