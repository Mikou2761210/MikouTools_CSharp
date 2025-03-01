namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    public class MultiLevelCascadeFilteredView<FilterKey, ItemValue> : MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, MultiLevelCascadeCollection<FilterKey, ItemValue>, MultiLevelCascadeFilteredView<FilterKey, ItemValue>>
            where FilterKey : notnull
            where ItemValue : notnull
    {

        protected override MultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(MultiLevelCascadeCollection<FilterKey, ItemValue> @base, MultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new MultiLevelCascadeFilteredView<FilterKey, ItemValue>(@base, parent, filter, comparer);
        }


        internal MultiLevelCascadeFilteredView(MultiLevelCascadeCollection<FilterKey, ItemValue> @base, MultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null) : base(@base, parent)
        {
            base.Initialize(filter, comparer);
        }
    }
}
