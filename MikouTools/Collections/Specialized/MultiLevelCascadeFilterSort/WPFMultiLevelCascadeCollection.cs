using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    public class WPFMultiLevelCascadeCollection<FilterKey, ItemValue> : ConcurrentMultiLevelCascadeCollectionBase<FilterKey, ItemValue, WPFMultiLevelCascadeCollection<FilterKey, ItemValue>, WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>> where FilterKey : notnull where ItemValue : notnull
    {


        protected override WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(WPFMultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            return new WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparer);
        }

        protected override WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue> CreateChildCollection(WPFMultiLevelCascadeCollection<FilterKey, ItemValue> @base, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            return new WPFMultiLevelCascadeFilteredView<FilterKey, ItemValue>(this, null, filter, comparison);
        }


        public override ItemValue this[int id]
        {
            get
            {
                return base[id];
            }
            set
            {
                lock (base._lock)
                {
                    ItemValue oldvalue = base[id];
                    base._baseList[id] = value; 
                    
                    foreach (var child in _children)
                    {
                        child.Value.NotifyChildrenOfReplace(id, value, oldvalue);
                    }
                }

            }
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
