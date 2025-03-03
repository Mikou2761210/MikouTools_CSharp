namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    [Obsolete("This class was previously part of MikouTools but is now maintained as a standalone repository. It is recommended to use the new repository.")]
    public class ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> : ConcurrentMultiLevelCascadeCollectionBase<FilterKey, ItemValue, ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue>, ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>> where FilterKey : notnull where ItemValue : notnull
    {

        protected override ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new ConcurrentMultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparer);
        }
    }
}
