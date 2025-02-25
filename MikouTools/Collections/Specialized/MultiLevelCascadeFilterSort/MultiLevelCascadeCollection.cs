namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    public class MultiLevelCascadeCollection<FilterKey, ItemValue> : MultiLevelCascadeCollectionBase<FilterKey, ItemValue, MultiLevelCascadeCollection<FilterKey, ItemValue>, MultiLevelCascadeFilteredView<FilterKey, ItemValue>> where FilterKey : notnull where ItemValue : notnull
    {

        protected override MultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(MultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new MultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparer);
        }

        protected override MultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(MultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            return new MultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparison);
        }
    }
}
