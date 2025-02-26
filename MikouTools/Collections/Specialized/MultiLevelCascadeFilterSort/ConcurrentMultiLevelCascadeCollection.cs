namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    public class ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> : ConcurrentMultiLevelCascadeCollectionBase<FilterKey, ItemValue, ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue>, ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>> where FilterKey : notnull where ItemValue : notnull
    {

        protected override ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparer);
        }

        protected override ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            return new ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparison);
        }
    }
}
