namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    public class ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue> : ConcurrentMultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue>, ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>>
            where FilterKey : notnull
            where ItemValue : notnull
    {

        protected override ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> @base, ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>(@base, parent, filter, comparer);
        }

        protected override ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> @base, ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            return new ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>(@base, parent, filter, comparison);
        }


        internal ConcurrentMultiLevelCascadeFilteredView(ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> @base, ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null) : base(@base, parent, filter, comparer)
        {

        }
        internal ConcurrentMultiLevelCascadeFilteredView(ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> @base, ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null) : base(@base, parent, filter, comparison)
        {

        }
    }
}
