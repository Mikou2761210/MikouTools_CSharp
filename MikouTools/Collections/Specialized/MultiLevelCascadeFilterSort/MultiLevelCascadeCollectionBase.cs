using MikouTools.Collections.Optimized;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{

    public abstract class MultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered> where FilterKey : notnull where ItemValue : notnull where TCollection : MultiLevelCascadeCollectionBase<FilterKey, ItemValue,TCollection, TFiltered> where TFiltered : MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        internal DualKeyDictionary<int, ItemValue> _baseList = [];

        protected readonly Dictionary<FilterKey, TFiltered> _children = [];
        private readonly Stack<int> _availableIds = [];
        private int _nextId = 0;


        protected abstract TFiltered CreateChildCollection(TCollection @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null);
        protected abstract TFiltered CreateChildCollection(TCollection @base, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null);

        public virtual IEnumerable<int> GetIDs() => _baseList.Keys;
        public virtual IEnumerable<ItemValue> GetValues() => _baseList.Values;
        public virtual ItemValue this[int id]
        {
            get
            {
                return _baseList[id];
            }

            set
            {
                _baseList[id] = value;
            }
        }

        private int NewId()
        {
            if (_availableIds.Count > 0)
            {
                return _availableIds.Pop();
            }
            return _nextId++;
        }

        public virtual int Add(ItemValue item)
        {
            int id = NewId();
            _baseList.Add(id, item);
            foreach (var child in _children)
            {
                child.Value.Add(id);
            }
            return id;
        }
        public virtual void AddRange(IEnumerable<ItemValue> items)
        {
            foreach (ItemValue item in items) Add(item);
        }

        public virtual bool Remove(ItemValue item)
        {
            if (_baseList.TryGetKey(item,out int id))
            {
                return RemoveId(id);
            }
            return false;
        }

        public virtual bool RemoveId(int id)
        {
            if (_baseList.Remove(id))
            {
                _availableIds.Push(id);
                foreach (var child in _children)
                {
                    child.Value.Remove(id);
                }
                return true;
            }
            return false;
        }



        //Filter
        public virtual TFiltered? GetFilterView(FilterKey Key)
        {
            if (_children.TryGetValue(Key, out TFiltered? value))
                return value;
            return null;
        }
        public virtual void AddFilterView(FilterKey filterKey, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            _children.Add(filterKey, CreateChildCollection((TCollection)this, filter, comparer));
        }

        public virtual void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            _children.Add(filterName, CreateChildCollection((TCollection)this, filter, comparison));
        }

        public virtual void RemoveFilterView(FilterKey Key)
        {
            _children.Remove(Key);
        }

    }
}
