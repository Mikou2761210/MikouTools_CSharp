using MikouTools.Collections.DirtySort;
using System.Collections;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{

    public abstract partial class MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered> : IEnumerable<ItemValue> where FilterKey : notnull where ItemValue : notnull where TCollection : MultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered> where TFiltered : MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue,TCollection, TFiltered>
    {
        private readonly TCollection _base;
        private readonly TFiltered? _parent;
        public Func<ItemValue, bool>? FilterFunc { get; private set; } = null;

        readonly internal DirtySortList<int> _idList = [];

        private readonly Dictionary<FilterKey, TFiltered> _children= [];


        protected abstract TFiltered CreateChildCollection(TCollection @base, TFiltered? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null);
        protected abstract TFiltered CreateChildCollection(TCollection @base, TFiltered? parent = null, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null);

        public virtual IEnumerator<ItemValue> GetEnumerator() => new FilterEnumerator(_base, _idList);
        IEnumerator IEnumerable.GetEnumerator() => new FilterEnumerator(_base, _idList);

        public virtual int Count => _idList.Count;



        internal MultiLevelCascadeFilteredViewBase(TCollection @base, TFiltered? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            _base = @base;
            _parent = parent;
            ChangeFilter(filter);
            Sort(comparer);
        }

        internal MultiLevelCascadeFilteredViewBase(TCollection @base, TFiltered? parent = null, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null) : this(@base, parent, filter, comparison != null ? Comparer<ItemValue>.Create(comparison) : null) { }


        public virtual ItemValue this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_idList.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _base[_idList[index]];
            }
        }

        private bool FilterCheck(int id) => (FilterFunc == null || FilterFunc(_base[id]));


        internal bool Add(int id) { return AddCore(id); }
        private bool AddCore(int id)
        {
            if (FilterCheck(id))
            {
                _idList.Add(id);
                return true;
            }
            return false;   
        }

        internal virtual bool AddRange(IEnumerable<int> ids)
        {
            bool result = false;
            foreach (int id in ids)
            {
                if (AddCore(id))
                {
                    result = true;
                }
            }
            return result;
        }

        internal virtual bool Remove(int id)
        {
            if (_idList.Remove(id))
            {
                foreach (var child in _children)
                {
                    child.Value.Remove(id);
                }
                return true;
            }
            return false;
        }
        internal virtual void RemoveAt(int index)
        {

            if ((uint)index >= (uint)_idList.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            int removeId = _idList[index];
            _idList.RemoveAt(index);
            foreach (var child in _children)
            {
                child.Value.Remove(removeId);
            }
        }



        public virtual int IndexOf(ItemValue item)
        {
            if (_base._baseList.TryGetKey(item , out int id))
            {
                return _idList.IndexOf(id);
            }
            return -1;
        }



        //Sort
        public virtual bool Sort() => Sort(0, _idList.Count, null);

        public virtual bool Sort(IComparer<ItemValue>? comparer) => Sort(0, _idList.Count, comparer);

        public virtual bool Sort(int index, int count, IComparer<ItemValue>? comparer) => _idList.Sort(index, count, comparer != null ? new FilterComparer(_base, comparer) : null);

        public virtual bool Sort(Comparison<ItemValue> comparison) => this.Sort(Comparer<ItemValue>.Create(comparison));

        public virtual bool AddRedoLastSort(int id)
        {
            if (FilterCheck(id))
            {
                int index = _idList.BinarySearch(id, _idList.LastComparer);
                if (index < 0)
                    index = ~index;
                bool DirtySave = _idList.IsDirty;
                _idList.Insert(index, id);
                _idList.IsDirty = DirtySave;
                foreach (var child in _children)
                {
                    child.Value.AddRedoLastSort(id);
                }
                return true;
            }
            return false;
        }

        public virtual bool RedoLastSort()  => _idList.RedoLastSort();
        public virtual bool RedoLastSortRecursively()
        {
            bool result = RedoLastSort();
            foreach (var child in _children)
            {
                child.Value.RedoLastSortRecursively();
            }
            return result;
        }



        //Filter
        public virtual bool ChangeFilter(Func<ItemValue, bool>? filterFunc)
        {
            if (FilterFunc != filterFunc)
            {
                FilterFunc = filterFunc;
                HashSet<int> baseIdHashSet = _parent == null ? [.. _base._baseList.Keys] : [.._parent._idList];
                HashSet<int> idHashSet = [.. _idList];

                if (FilterFunc == null)
                {
                    foreach(int addid in baseIdHashSet.Except(idHashSet))
                    {
                        Add(addid);
                    }
                }
                else
                {
                    foreach (int baseId in baseIdHashSet)
                    {
                        if (idHashSet.Contains(baseId))
                        {
                            if (!FilterFunc(_base[baseId]))
                                Remove(baseId);
                        }
                        else
                        {
                            if (FilterFunc(_base[baseId]))
                                Add(baseId);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public virtual TFiltered? GetFilterView(FilterKey filterName)
        {
            if (_children.TryGetValue(filterName, out TFiltered? value))
                return value;
            return null;
        }
        public virtual void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            _children.Add(filterName, CreateChildCollection(_base, (TFiltered)this, filter, comparer));
        }
        public virtual void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            _children.Add(filterName, CreateChildCollection(_base, (TFiltered)this, filter, comparison));
        }

        public virtual void RemoveFilterView(FilterKey filterName)
        {
            _children.Remove(filterName);
        }
    }
    
    //FilterEnumerator
    public abstract partial class MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        public struct FilterEnumerator : IEnumerator<ItemValue>, IEnumerator
        {
            private readonly TCollection _base;
            private readonly DirtySortList<int> _idList;
            internal FilterEnumerator(TCollection @base, DirtySortList<int> @idList)
            {
                _base = @base;
                _idList = @idList;
                _index = 0;
                _current = default;
            }

            private int _index;
            private ItemValue? _current;


            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {

                if (((uint)_index < (uint)_idList.Count))
                {
                    _current = _base[_idList[_index]];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                _index = _idList.Count + 1;
                _current = default;
                return false;
            }

            public readonly ItemValue Current => _current!;

            readonly object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _idList.Count + 1)
                    {
                        throw new InvalidOperationException();
                    }
                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default;
            }
        }
    }

    //Filter Comparer
    public abstract partial class MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        private partial class FilterComparer(TCollection @base, IComparer<ItemValue>? comparer) : IComparer<int>
        {
            private readonly IComparer<ItemValue> _comparer = comparer ?? Comparer<ItemValue>.Default;

            public int Compare(int x, int y)
            {
                return _comparer.Compare(@base._baseList[x], @base._baseList[y]);
            }
        }
    }
}
