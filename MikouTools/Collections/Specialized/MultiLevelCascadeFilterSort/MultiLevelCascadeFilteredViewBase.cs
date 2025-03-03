using MikouTools.Collections.DirtySort;
using System.Collections;
using System.Diagnostics;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    [Obsolete("This class was previously part of MikouTools but is now maintained as a standalone repository. It is recommended to use the new repository.")]
    /// <summary>
    /// Base class for a filtered view in a multi-level cascade collection.
    /// This class provides filtering, sorting, and child view management functionality.
    /// </summary>
    /// <typeparam name="FilterKey">Type used as a key for filtering views (must be non-null).</typeparam>
    /// <typeparam name="ItemValue">Type of the items stored (must be non-null).</typeparam>
    /// <typeparam name="TCollection">The concrete type of the base collection.</typeparam>
    /// <typeparam name="TFiltered">The concrete type of the filtered view.</typeparam>
    public abstract class MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered> : IEnumerable<ItemValue>, IEnumerable
        where FilterKey : notnull
        where ItemValue : notnull
        where TCollection : MultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered>
        where TFiltered : MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        /// <summary>
        /// An object for accessing base classes
        /// </summary>
        internal MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered> BaseThis => this;


        // The base collection from which items are drawn.
        private readonly TCollection _base;
        // The parent filtered view (if any) for cascading filters.
        private readonly TFiltered? _parent;

        /// <summary>
        /// Factory method for creating the filtered ID list.
        /// </summary>
        protected virtual DirtySortList<int> CreateFilteredIdList()
        {
            return [];
        }

        /// <summary>
        /// Factory method for creating the collection of child filtered views.
        /// </summary>
        protected virtual Dictionary<FilterKey, TFiltered> CreateChildViews()
        {
            return [];
        }

        /// <summary>
        /// Gets the filter function used to determine which items are included in this view.
        /// If null, no filtering is applied.
        /// </summary>
        public Func<ItemValue, bool>? FilterFunc { get; private set; } = null;

        // List of item IDs included in this filtered view.
        // Note: The initializer "[]" is a placeholder and should be replaced with an actual collection initializer (e.g., new DirtySortList<int>()).
        protected readonly DirtySortList<int> _idList;

        // Dictionary that holds child filtered views associated with specific filter keys.
        protected readonly Dictionary<FilterKey, TFiltered> _children;

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
        /// Returns an enumerator that iterates through the filtered items.
        /// </summary>
        /// <returns>An enumerator for the filtered items.</returns>
        public IEnumerator<ItemValue> GetEnumerator() => new FilterEnumerator(_base, _idList);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the number of items in the filtered view.
        /// </summary>
        public int Count => _idList.Count;

        // Indicates whether the view has been initialized.
        bool _initialize = false;

        /// <summary>
        /// Checks whether the view has been initialized; if not, throws an exception.
        /// </summary>
        private void InitializeCheck()
        {
            if (!_initialize)
                throw new Exception("Not initialized");
        }

        /// <summary>
        /// Initializes the filtered view by applying an optional filter function and sorting using the provided comparer.
        /// </summary>
        /// <param name="filter">An optional filter function.</param>
        /// <param name="comparer">An optional comparer for sorting.</pa ram>
        internal virtual void Initialize(Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            if (!_initialize)
            {
                _initialize = true;
                ChangeFilter(filter);
                Sort(comparer);
            }
        }

        /// <summary>
        /// Initializes a new instance of the filtered view with an optional parent view.
        /// </summary>
        /// <param name="base">The base collection instance.</param>
        /// <param name="parent">The parent filtered view (if any).</param>
        internal MultiLevelCascadeFilteredViewBase(TCollection @base, TFiltered? parent = null)
        {
            _base = @base;
            _parent = parent;

            _idList = CreateFilteredIdList();
            _children = CreateChildViews();
        }

        /// <summary>
        /// Gets or sets the item at the specified index in the filtered view.
        /// </summary>
        /// <param name="index">The zero-based index in the filtered view.</param>
        /// <returns>The item corresponding to the specified index.</returns>
        public virtual ItemValue this[int index]
        {
            get
            {
                InitializeCheck();
                if ((uint)index >= (uint)_idList.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _base[_idList[index]];
            }
            set
            {
                InitializeCheck();
                if ((uint)index >= (uint)_idList.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _base[_idList[index]] = value;
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
        /// If the item passes the filter, it is added and the change is propagated to child views.
        /// </summary>
        /// <param name="id">The unique identifier of the item to add.</param>
        /// <returns>True if the item is added; otherwise, false.</returns>
        internal virtual bool Add(int id)
        {
            InitializeCheck();
            if (FilterCheck(id))
            {
                _idList.Add(id);
                foreach (var child in _children.Values)
                {
                    child.Add(id);
                }
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
            InitializeCheck();
            bool result = false;
            if (FilterFunc == null)
            {
                _idList.AddRange(ids);
                foreach (var child in _children.Values)
                {
                    child.AddRange(ids);
                }
            }
            else
            {
                foreach (int id in ids)
                {
                    if (Add(id))
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Inserts the item identified by its unique ID into the filtered view at the correct sorted position
        /// using binary search based on the last used comparer. If the item does not pass the filter,
        /// it is not inserted.
        /// </summary>
        /// <param name="id">The unique identifier of the item to insert.</param>
        /// <returns>
        /// The index at which the item was inserted if it passes the filter; otherwise, -1.
        /// </returns>
        internal int InsertItemInOrder(int id)
        {
            InitializeCheck();
            if (FilterCheck(id))
            {
                // Find the correct sorted index for the new item.
                int index = _idList.BinarySearch(id, _idList.LastComparer);
                if (index < 0)
                    index = ~index;
                // Preserve the current 'dirty' state before insertion.
                bool dirtySave = _idList.IsDirty;
                _idList.Insert(index, id);
                _idList.IsDirty = dirtySave;
                // Propagate the insertion to all child filtered views.
                foreach (var child in _children)
                {
                    child.Value.InsertItemInOrder(id);
                }
                return index;
            }
            return -1;
        }

        /// <summary>
        /// Removes the item with the specified ID from the filtered view.
        /// </summary>
        /// <param name="id">The unique identifier of the item to remove.</param>
        /// <returns>True if the item is removed; otherwise, false.</returns>
        internal virtual bool Remove(int id)
        {
            InitializeCheck();
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
            InitializeCheck();
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
        /// <returns>The index of the item if found; otherwise, -1.</returns>
        public virtual int IndexOf(ItemValue item)
        {
            InitializeCheck();
            if (_base._baseList.TryGetKey(item, out int id))
            {
                return _idList.IndexOf(id);
            }
            return -1;
        }

        /// <summary>
        /// Moves an item from one index to another within the filtered view.
        /// </summary>
        /// <param name="fromIndex">The current index of the item to move.</param>
        /// <param name="toIndex">The target index where the item should be placed.</param>
        /// <returns>
        /// The new index of the moved item if the move is performed; returns -1 if no move is needed.
        /// </returns>
        public int Move(int fromIndex, int toIndex)
        {
            InitializeCheck();

            if (fromIndex < 0 || fromIndex >= _idList.Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex), "Source index is out of range.");

            if (toIndex < 0 || toIndex > _idList.Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex), "Destination index is out of range.");

            if (fromIndex == toIndex)
                return -1;

            int id = _idList[fromIndex];
            _idList.RemoveAt(fromIndex);

            // If the source index was before the destination, adjust the destination index 
            // because the list now contains one fewer element.
            if (fromIndex < toIndex)
                toIndex--;

            _idList.Insert(toIndex, id);
            return toIndex;
        }

        #region Sorting Methods

        /// <summary>
        /// Sorts the filtered view using the default comparer.
        /// </summary>
        /// <returns>True if the sort operation is successful; otherwise, false.</returns>
        public bool Sort() => Sort(0, _idList.Count, null);

        /// <summary>
        /// Sorts the filtered view using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer used to sort the items.</param>
        /// <returns>True if the sort operation is successful; otherwise, false.</returns>
        public bool Sort(IComparer<ItemValue>? comparer) => Sort(0, _idList.Count, comparer);

        /// <summary>
        /// Sorts a range of items in the filtered view using the specified comparer.
        /// </summary>
        /// <param name="index">The starting index of the range to sort.</param>
        /// <param name="count">The number of items to sort.</param>
        /// <param name="comparer">The comparer used to sort the items.</param>
        /// <returns>True if the sort operation is successful; otherwise, false.</returns>
        public virtual bool Sort(int index, int count, IComparer<ItemValue>? comparer)
        {
            InitializeCheck();
            return _idList.Sort(index, count, comparer != null ? new FilterComparer(_base, comparer) : null);
        }

        /// <summary>
        /// Sorts the filtered view using the specified comparison delegate.
        /// </summary>
        /// <param name="comparison">The comparison delegate used to sort the items.</param>
        /// <returns>True if the sort operation is successful; otherwise, false.</returns>
        public bool Sort(Comparison<ItemValue> comparison) => this.Sort(Comparer<ItemValue>.Create(comparison));


        /// <summary>
        /// Re-sorts the filtered view using the last used sorting logic.
        /// </summary>
        /// <returns>True if the re-sort operation is successful; otherwise, false.</returns>
        public bool RedoLastSort()
        {
            InitializeCheck();
            return _idList.RedoLastSort();
        }

        /// <summary>
        /// Recursively re-sorts the filtered view and all its child filtered views.
        /// </summary>
        /// <returns>True if the re-sort operation is successful for all views; otherwise, false.</returns>
        public bool RedoLastSortRecursively()
        {
            InitializeCheck();
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
            InitializeCheck();
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
                    // If no filter is specified, add all items from the base that are not already in the view.
                    AddRange(parentIdHashSet.Except(idHashSet));
                }
                else
                {
                    _currentFilterAll = false;
                    // Update the view by removing items that do not satisfy the filter and adding those that do.
                    foreach (int baseId in parentIdHashSet)
                    {
                        if (idHashSet.Contains(baseId))
                        {
                            Remove(baseId);
                        }
                        else
                        {
                            Add(baseId);
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
        /// <param name="filterKey">The key identifying the filtered view.</param>
        /// <returns>The child filtered view if it exists; otherwise, null.</returns>
        public TFiltered? GetFilterView(FilterKey filterKey)
        {
            InitializeCheck();
            if (_children.TryGetValue(filterKey, out TFiltered? value))
                return value;
            return null;
        }

        /// <summary>
        /// Adds a new child filtered view to this view using a filter function and an optional comparer.
        /// </summary>
        /// <param name="filterKey">The key that identifies the new filtered view.</param>
        /// <param name="filter">An optional filter function to select items.</param>
        /// <param name="comparer">An optional comparer for sorting the items in the child view.</param>
        /// <returns>The newly created child filtered view.</returns>
        public TFiltered AddFilterView(FilterKey filterKey, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            InitializeCheck();
            TFiltered newFilter = CreateChildCollection(_base, (TFiltered)this, filter, comparer);
            _children.Add(filterKey, newFilter);
            return newFilter;
        }

        /// <summary>
        /// Removes the child filtered view associated with the specified filter key.
        /// </summary>
        /// <param name="filterKey">The key identifying the filtered view to remove.</param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
        public bool RemoveFilterView(FilterKey filterKey)
        {
            InitializeCheck();
            return _children.Remove(filterKey);
        }

        #endregion Filter Methods

        #region FilterEnumerator Struct

        /// <summary>
        /// Enumerator for iterating through the filtered items.
        /// </summary>
        public struct FilterEnumerator : IEnumerator<ItemValue>, System.Collections.IEnumerator
        {
            // Reference to the base collection to access items by ID.
            private readonly TCollection _base;
            // The list of item IDs included in the filtered view.
            private readonly DirtySortList<int> _idList;
            // Current index in the enumeration.
            private int _index;
            // Current item being enumerated.
            private ItemValue? _current;

            /// <summary>
            /// Initializes a new instance of the <see cref="FilterEnumerator"/> struct.
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

            /// <summary>
            /// Releases any resources used by the enumerator.
            /// </summary>
            public readonly void Dispose() { }

            /// <summary>
            /// Advances the enumerator to the next element.
            /// </summary>
            /// <returns>True if the enumerator was successfully advanced; otherwise, false.</returns>
            public bool MoveNext()
            {
                if ((uint)_index < (uint)_idList.Count)
                {
                    _current = _base[_idList[_index]];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            /// <summary>
            /// Handles the termination of the enumeration.
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
                        throw new InvalidOperationException();
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

        #endregion FilterEnumerator Struct

        #region FilterComparer Class

        /// <summary>
        /// Comparer for item IDs that uses a provided comparer to compare the corresponding items in the base collection.
        /// </summary>
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
            /// A signed integer that indicates the relative values of the items;
            /// less than zero if x is less than y, zero if x equals y, and greater than zero if x is greater than y.
            /// </returns>
            public int Compare(int x, int y)
            {
                return _comparer.Compare(@base._baseList[x], @base._baseList[y]);
            }
        }

        #endregion FilterComparer Class
    }
}

