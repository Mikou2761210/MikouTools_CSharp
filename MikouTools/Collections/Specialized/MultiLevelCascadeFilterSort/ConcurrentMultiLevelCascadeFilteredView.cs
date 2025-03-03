namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    [Obsolete("This class was previously part of MikouTools but is now maintained as a standalone repository. It is recommended to use the new repository.")]
    public class ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue> : ConcurrentMultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue>, ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>>
            where FilterKey : notnull
            where ItemValue : notnull
    {

        protected override ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> @base, ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>(@base, parent, filter, comparer);
        }


        internal ConcurrentMultiLevelCascadeFilteredView(ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> @base, ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null) : base(@base, parent)
        {
            base.Initialize(filter, comparer);
        }
    }
}
