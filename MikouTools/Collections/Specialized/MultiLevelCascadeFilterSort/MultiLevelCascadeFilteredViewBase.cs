using MikouTools.Collections.DirtySort;
using System.Collections;
using System.Diagnostics;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    /// <summary>
    /// Represents a base filtered view that supports multi-level cascade filtering and sorting.
    /// This abstract class manages a view of items filtered from a base collection and supports cascading
    /// filters as well as propagating sorting changes. It implements <see cref="IEnumerable{ItemValue}"/>
    /// to allow iteration over the filtered items.
    /// </summary>
    /// <typeparam name="FilterKey">
    /// The type used as a key for identifying filter views.
    /// </typeparam>
    /// <typeparam name="ItemValue">
    /// The type of items contained in the collection.
    /// </typeparam>
    /// <typeparam name="TCollection">
    /// The type of the base collection. Must derive from <see cref="MultiLevelCascadeCollectionBase{FilterKey, ItemValue, TCollection, TFiltered}"/>.
    /// </typeparam>
    /// <typeparam name="TFiltered">
    /// The type of the filtered view. Must derive from <see cref="MultiLevelCascadeFilteredViewBase{FilterKey, ItemValue, TCollection, TFiltered}"/>.
    /// </typeparam>
    public abstract partial class MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered> : IMultiLevelCascadeFilteredView<FilterKey, ItemValue, TFiltered>, IEnumerable<ItemValue>
        where FilterKey : notnull
        where ItemValue : notnull
        where TCollection : MultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered>
        where TFiltered : MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        // The base collection from which items are drawn.
        private readonly TCollection _base;
        // The parent filtered view (if any) for cascading filters.
        private readonly TFiltered? _parent;

        /// <summary>
        /// Gets the filter function used to determine which items are included in this view.
        /// If null, no filtering is applied.
        /// </summary>
        public Func<ItemValue, bool>? FilterFunc { get; private set; } = null;

        // List of item IDs included in this filtered view.
        // Note: The initializer "[]" is a placeholder and should be replaced with an actual collection initializer (e.g., new DirtySortList<int>()).
        readonly internal DirtySortList<int> _idList = [];

        // Dictionary that holds child filtered views associated with specific filter keys.
        protected readonly Dictionary<FilterKey, TFiltered> _children = [];

        /// <summary>
        /// Creates a new child filtered view using an optional filter function and comparer.
        /// </summary>
        /// <param name="base">The base collection instance.</param>
        /// <param name="parent">The parent filtered view (if any).</param>
        /// <param name="filter">An optional filter function to select items.</param>
        /// <param name="comparer">An optional comparer for sorting the items.</param>
        /// <returns>A new instance of a filtered view.</returns>
        protected abstract TFiltered CreateChildCollection(TCollection @base, TFiltered? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null);

        /// <summary>
        /// Creates a new child filtered view using an optional filter function and a comparison delegate.
        /// </summary>
        /// <param name="base">The base collection instance.</param>
        /// <param name="parent">The parent filtered view (if any).</param>
        /// <param name="filter">An optional filter function to select items.</param>
        /// <param name="comparison">An optional comparison delegate for sorting the items.</param>
        /// <returns>A new instance of a filtered view.</returns>
        protected abstract TFiltered CreateChildCollection(TCollection @base, TFiltered? parent = null, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null);

        /// <summary>
        /// Returns an enumerator that iterates through the filtered items.
        /// </summary>
        /// <returns>An enumerator for the filtered items.</returns>
        public virtual IEnumerator<ItemValue> GetEnumerator() => new FilterEnumerator(_base, _idList);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new FilterEnumerator(_base, _idList);

        /// <summary>
        /// Gets the number of items in the filtered view.
        /// </summary>
        public virtual int Count => _idList.Count;

        /// <summary>
        /// Initializes a new instance of the filtered view with an optional filter and comparer.
        /// </summary>
        /// <param name="base">The base collection instance.</param>
        /// <param name="parent">The parent filtered view (if any).</param>
        /// <param name="filter">An optional filter function to select items.</param>
        /// <param name="comparer">An optional comparer for sorting the items.</param>
        internal MultiLevelCascadeFilteredViewBase(TCollection @base, TFiltered? parent = null, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            _base = @base;
            _parent = parent;
            ChangeFilter(filter);
            Sort(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the filtered view with an optional filter and comparison delegate.
        /// </summary>
        /// <param name="base">The base collection instance.</param>
        /// <param name="parent">The parent filtered view (if any).</param>
        /// <param name="filter">An optional filter function to select items.</param>
        /// <param name="comparison">An optional comparison delegate for sorting the items.</param>
        internal MultiLevelCascadeFilteredViewBase(TCollection @base, TFiltered? parent = null, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
            : this(@base, parent, filter, comparison != null ? Comparer<ItemValue>.Create(comparison) : null)
        {
        }

        /// <summary>
        /// Gets the item at the specified index in the filtered view.
        /// </summary>
        /// <param name="index">The zero-based index in the filtered view.</param>
        /// <returns>The item corresponding to the specified index.</returns>
        public virtual ItemValue this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_idList.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _base[_idList[index]];
            }
        }

        /// <summary>
        /// Checks whether the item with the given ID satisfies the current filter function.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <returns>True if the item passes the filter or if no filter is set; otherwise, false.</returns>
        private bool FilterCheck(int id) => (FilterFunc == null || FilterFunc(_base[id]));

        /// <summary>
        /// Attempts to add an item by its ID to the filtered view.
        /// </summary>
        /// <param name="id">The unique identifier of the item to add.</param>
        /// <returns>True if the item is added; otherwise, false.</returns>
        internal virtual bool Add(int id)
        {
            if (FilterCheck(id))
            {
                _idList.Add(id);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Adds a range of item IDs to the filtered view.
        /// </summary>
        /// <param name="ids">An enumerable collection of item IDs.</param>
        /// <returns>True if at least one item is added; otherwise, false.</returns>
        internal virtual bool AddRange(IEnumerable<int> ids)
        {
            bool result = false;
            foreach (int id in ids)
            {
                if (Add(id))
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Removes the item with the specified ID from the filtered view.
        /// </summary>
        /// <param name="id">The unique identifier of the item to remove.</param>
        /// <returns>True if the item is removed; otherwise, false.</returns>
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

        /// <summary>
        /// Removes the item at the specified index from the filtered view.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
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

        /// <summary>
        /// Returns the index of the specified item in the filtered view.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>
        /// The index of the item if found; otherwise, -1.
        /// </returns>
        public virtual int IndexOf(ItemValue item)
        {
            if (_base._baseList.TryGetKey(item, out int id))
            {
                return _idList.IndexOf(id);
            }
            return -1;
        }

        #region Sorting Methods

        /// <summary>
        /// Sorts the filtered view using the default comparer.
        /// </summary>
        /// <returns>True if the sort operation is successful; otherwise, false.</returns>
        public virtual bool Sort() => Sort(0, _idList.Count, null);

        /// <summary>
        /// Sorts the filtered view using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer used to sort the items.</param>
        /// <returns>True if the sort operation is successful; otherwise, false.</returns>
        public virtual bool Sort(IComparer<ItemValue>? comparer) => Sort(0, _idList.Count, comparer);

        /// <summary>
        /// Sorts a range of items in the filtered view using the specified comparer.
        /// </summary>
        /// <param name="index">The starting index of the range to sort.</param>
        /// <param name="count">The number of items to sort.</param>
        /// <param name="comparer">The comparer used to sort the items.</param>
        /// <returns>True if the sort operation is successful; otherwise, false.</returns>
        public virtual bool Sort(int index, int count, IComparer<ItemValue>? comparer) => _idList.Sort(index, count, comparer != null ? new FilterComparer(_base, comparer) : null);

        /// <summary>
        /// Sorts the filtered view using the specified comparison delegate.
        /// </summary>
        /// <param name="comparison">The comparison delegate used to sort the items.</param>
        /// <returns>True if the sort operation is successful; otherwise, false.</returns>
        public virtual bool Sort(Comparison<ItemValue> comparison) => this.Sort(Comparer<ItemValue>.Create(comparison));

        /// <summary>
        /// Adds an item by its ID to the filtered view and re-sorts it into the correct position.
        /// This method also propagates the change to all child filtered views.
        /// </summary>
        /// <param name="id">The unique identifier of the item to add and sort.</param>
        /// <returns>True if the item is added and re-sorted; otherwise, false.</returns>
        internal virtual int AddRedoLastSort(int id)
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
                return index;
            }
            return -1;
        }

        /// <summary>
        /// Re-sorts the filtered view using the last used sorting logic.
        /// </summary>
        /// <returns>True if the re-sort operation is successful; otherwise, false.</returns>
        public virtual bool RedoLastSort() => _idList.RedoLastSort();

        /// <summary>
        /// Recursively re-sorts the filtered view and all its child filtered views.
        /// </summary>
        /// <returns>True if the re-sort operation is successful; otherwise, false.</returns>
        public virtual bool RedoLastSortRecursively()
        {
            bool result = RedoLastSort();
            foreach (var child in _children)
            {
                child.Value.RedoLastSortRecursively();
            }
            return result;
        }

        #endregion Sorting Methods

        #region Filter Methods

        private bool _currentFilterAll = false;
        /// <summary>
        /// Changes the filter function applied to this view and updates the view accordingly.
        /// Items that no longer satisfy the filter are removed, and items that now satisfy
        /// the filter are added.
        /// </summary>
        /// <param name="filterFunc">The new filter function to apply. If null, all items are included.</param>
        /// <returns>True if the filter function was changed; otherwise, false.</returns>
        public virtual bool ChangeFilter(Func<ItemValue, bool>? filterFunc)
        {
            if ((filterFunc == null && !_currentFilterAll) || FilterFunc != filterFunc)
            {
                FilterFunc = filterFunc;
                // Create a set of IDs from the base collection (or parent's filtered list if available).
                HashSet<int> parentIdHashSet = _parent == null ? [.. _base._baseList.Keys] : [.. _parent._idList];
                // Create a set of IDs that are currently in this view.
                HashSet<int> idHashSet = [.. _idList];

                if (FilterFunc == null)
                {
                    _currentFilterAll = true;
                    Debug.WriteLine($"{parentIdHashSet.Count} : {idHashSet.Count}");
                    // If no filter is specified, add all items from the base that are not already in the view.
                    foreach (int addId in parentIdHashSet.Except(idHashSet))
                    {
                        //Debug.WriteLine(addId);
                        _idList.Add(addId);
                    }
                }
                else
                {
                    _currentFilterAll = false;
                    // Update the view by removing items that do not satisfy the filter and adding those that do.
                    foreach (int baseId in parentIdHashSet)
                    {
                        if (idHashSet.Contains(baseId))
                        {
                            if (!FilterFunc(_base[baseId]))
                                _idList.Remove(baseId);
                        }
                        else
                        {
                            if (FilterFunc(_base[baseId]))
                                _idList.Add(baseId);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the child filtered view associated with the specified filter key.
        /// </summary>
        /// <param name="filterName">The key identifying the filtered view.</param>
        /// <returns>
        /// The child filtered view if it exists; otherwise, null.
        /// </returns>
        public virtual TFiltered? GetFilterView(FilterKey filterName)
        {
            if (_children.TryGetValue(filterName, out TFiltered? value))
                return value;
            return null;
        }

        /// <summary>
        /// Adds a new child filtered view to this view using a filter function and an optional comparer.
        /// </summary>
        /// <param name="filterName">The key that identifies the new filtered view.</param>
        /// <param name="filter">An optional filter function to select items.</param>
        /// <param name="comparer">An optional comparer for sorting the items in the child view.</param>
        public virtual void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            _children.Add(filterName, CreateChildCollection(_base, (TFiltered)this, filter, comparer));
        }

        /// <summary>
        /// Adds a new child filtered view to this view using a filter function and an optional comparison delegate.
        /// </summary>
        /// <param name="filterName">The key that identifies the new filtered view.</param>
        /// <param name="filter">An optional filter function to select items.</param>
        /// <param name="comparison">An optional comparison delegate for sorting the items in the child view.</param>
        public virtual void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter, Comparison<ItemValue> comparison)
        {
            _children.Add(filterName, CreateChildCollection(_base, (TFiltered)this, filter, comparison));
        }

        /// <summary>
        /// Removes the child filtered view associated with the specified filter key.
        /// </summary>
        /// <param name="filterName">The key identifying the filtered view to remove.</param>
        public virtual void RemoveFilterView(FilterKey filterName)
        {
            _children.Remove(filterName);
        }

        #endregion Filter Methods
    }

    #region FilterEnumerator Struct

    /// <summary>
    /// Enumerator for iterating over the items in a <see cref="MultiLevelCascadeFilteredViewBase{FilterKey, ItemValue, TCollection, TFiltered}"/>.
    /// </summary>
    public abstract partial class MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        public struct FilterEnumerator : IEnumerator<ItemValue>, System.Collections.IEnumerator
        {
            // Reference to the base collection to access items by ID.
            private readonly TCollection _base;
            // The list of item IDs included in the filtered view.
            private readonly DirtySortList<int> _idList;

            /// <summary>
            /// Initializes a new instance of the <see cref="FilterEnumerator"/>.
            /// </summary>
            /// <param name="base">The base collection instance.</param>
            /// <param name="idList">The list of item IDs in the filtered view.</param>
            internal FilterEnumerator(TCollection @base, DirtySortList<int> @idList)
            {
                _base = @base;
                _idList = @idList;
                _index = 0;
                _current = default;
            }

            // The current index in the enumeration.
            private int _index;
            // The current item being enumerated.
            private ItemValue? _current;

            /// <summary>
            /// Releases any resources used by the enumerator.
            /// </summary>
            public readonly void Dispose()
            {
            }

            /// <summary>
            /// Advances the enumerator to the next element.
            /// </summary>
            /// <returns>True if the enumerator was successfully advanced; otherwise, false.</returns>
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

            /// <summary>
            /// Handles the termination of enumeration.
            /// </summary>
            /// <returns>False, indicating the end of the collection.</returns>
            private bool MoveNextRare()
            {
                _index = _idList.Count + 1;
                _current = default;
                return false;
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            public readonly ItemValue Current => _current!;

            /// <summary>
            /// Gets the current element in the collection (non-generic).
            /// </summary>
            readonly object? System.Collections.IEnumerator.Current
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

            /// <summary>
            /// Resets the enumerator to its initial state.
            /// </summary>
            void System.Collections.IEnumerator.Reset()
            {
                _index = 0;
                _current = default;
            }
        }
    }

    #endregion FilterEnumerator Struct

    #region FilterComparer Class

    /// <summary>
    /// Provides a comparer for sorting item IDs in a filtered view by comparing the underlying items.
    /// </summary>
    public abstract partial class MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        private partial class FilterComparer(TCollection @base, IComparer<ItemValue>? comparer) : IComparer<int>
        {
            // The comparer used to compare the actual items.
            private readonly IComparer<ItemValue> _comparer = comparer ?? Comparer<ItemValue>.Default;

            /// <summary>
            /// Compares two item IDs by comparing the corresponding items from the base collection.
            /// </summary>
            /// <param name="x">The first item ID.</param>
            /// <param name="y">The second item ID.</param>
            /// <returns>
            /// A signed integer that indicates the relative values of the items; less than zero if x is less than y,
            /// zero if x equals y, and greater than zero if x is greater than y.
            /// </returns>
            public int Compare(int x, int y)
            {
                return _comparer.Compare(@base._baseList[x], @base._baseList[y]);
            }
        }
    }

    #endregion FilterComparer Class
}

