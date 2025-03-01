using MikouTools.Collections.DirtySort;
using MikouTools.Collections.Optimized;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    /// <summary>
    /// Base class for a multi-level cascade collection that stores items with unique integer IDs
    /// and supports child filtered views.
    /// </summary>
    /// <typeparam name="FilterKey">Type used as a key for filtering views (must be non-null).</typeparam>
    /// <typeparam name="ItemValue">Type of the items stored in the collection (must be non-null).</typeparam>
    /// <typeparam name="TCollection">The concrete type of the multi-level cascade collection.</typeparam>
    /// <typeparam name="TFiltered">The concrete type of the filtered view.</typeparam>
    public abstract class MultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered>
        where FilterKey : notnull
        where ItemValue : notnull
        where TCollection : MultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered>
        where TFiltered : MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        // Dictionary that stores the base items using a unique integer key.
        internal DualKeyDictionary<int, ItemValue> _baseList;

        // Dictionary that holds the child filtered views associated with filter keys.
        protected readonly Dictionary<FilterKey, TFiltered> _children;

        /// <summary>
        /// Factory method for creating the base item dictionary.
        /// </summary>
        protected virtual DualKeyDictionary<int, ItemValue> CreateBaseItemDictionary()
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

        // Stack of available (reusable) IDs.
        private readonly Stack<int> _availableIds = [];

        // The next unique ID to assign when no reusable IDs are available.
        private int _nextId = 0;

        public MultiLevelCascadeCollectionBase()
        {
            _baseList = CreateBaseItemDictionary();
            _children = CreateChildViews();
        }

        /// <summary>
        /// Creates a new child filtered view with an optional filter function and comparer.
        /// </summary>
        /// <param name="base">The base collection instance.</param>
        /// <param name="filter">An optional filter function to determine which items are included in the view.</param>
        /// <param name="comparer">An optional comparer for sorting items within the view.</param>
        /// <returns>A new filtered view instance.</returns>
        protected abstract TFiltered CreateChildCollection(TCollection @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null);

        /// <summary>
        /// Gets all the unique integer IDs of the items in the base collection.
        /// </summary>
        public IEnumerable<int> GetIDs() => _baseList.Keys;

        /// <summary>
        /// Gets all the item values stored in the base collection.
        /// </summary>
        public IEnumerable<ItemValue> GetValues() => _baseList.Values;

        /// <summary>
        /// Gets or sets the item at the specified unique ID in the collection.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <returns>The item corresponding to the specified ID.</returns>
        public ItemValue this[int id]
        {
            get => _baseList[id];
            set => _baseList[id] = value;
        }

        /// <summary>
        /// Generates a new unique ID for an item. Reuses an available ID if one exists;
        /// otherwise, returns a new unique ID.
        /// </summary>
        /// <returns>A unique integer ID.</returns>
        private int NewId()
        {
            if (_availableIds.Count > 0)
            {
                return _availableIds.Pop();
            }
            return _nextId++;
        }

        /// <summary>
        /// Retrieves the unique ID assigned to the specified item.
        /// </summary>
        /// <param name="item">The item whose ID is requested.</param>
        /// <returns>The unique ID if found; otherwise, -1.</returns>
        public int GetId(ItemValue item)
        {
            if (_baseList.TryGetKey(item, out int id))
            {
                return id;
            }
            return -1;
        }

        /// <summary>
        /// Adds a new item to the base collection, assigns it a unique ID, and updates all child filtered views.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The unique ID assigned to the new item.</returns>
        public int Add(ItemValue item)
        {
            int id = NewId();
            if(!_baseList.TryAdd(id, item))
            {
                _availableIds.Push(id);
                return -1;
            }
            // Propagate the addition to each child filtered view.
            foreach (var child in _children)
            {
                child.Value.Add(id);
            }
            return id;
        }

        /// <summary>
        /// Adds a range of items to the base collection.
        /// </summary>
        /// <param name="items">An enumerable collection of items to add.</param>
        public void AddRange(IEnumerable<ItemValue> items)
        {
            List<int> ids = new(items.Count());
            foreach (ItemValue item in items)
            {
                int id = NewId();
                if(_baseList.TryAdd(id, item))
                {
                    ids.Add(id);                
                }
                else
                {
                    _availableIds.Push(id);
                }
            }
            foreach (var child in _children.Values)
            {
                child.AddRange(ids);
            }
        }

        /// <summary>
        /// Removes an item from the base collection by its value.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
        public bool Remove(ItemValue item)
        {
            if (_baseList.TryGetKey(item, out int id))
            {
                return RemoveId(id);
            }
            return false;
        }

        /// <summary>
        /// Removes an item from the base collection by its unique ID.
        /// The removed ID is pushed back into the available pool for reuse,
        /// and the removal is propagated to all child filtered views.
        /// </summary>
        /// <param name="id">The unique ID of the item to remove.</param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
        public bool RemoveId(int id)
        {
            if (_baseList.Remove(id))
            {
                _availableIds.Push(id);
                // Propagate the removal to each child filtered view.
                foreach (var child in _children)
                {
                    child.Value.Remove(id);
                }
                return true;
            }
            return false;
        }

        #region Filter Methods

        /// <summary>
        /// Retrieves the filtered view associated with the specified filter key.
        /// </summary>
        /// <param name="filterKey">The key identifying the filter view.</param>
        /// <returns>The filtered view if it exists; otherwise, null.</returns>
        public TFiltered? GetFilterView(FilterKey filterKey)
        {
            if (_children.TryGetValue(filterKey, out TFiltered? value))
                return value;
            return null;
        }

        /// <summary>
        /// Adds a new filter view to the collection using a filter function and an optional comparer.
        /// The new view is created by calling the abstract CreateChildCollection method.
        /// </summary>
        /// <param name="filterKey">The key identifying the new filter view.</param>
        /// <param name="filter">An optional filter function to determine which items are included in the view.</param>
        /// <param name="comparer">An optional comparer for sorting items within the view.</param>
        /// <returns>The newly created child filtered view.</returns>
        public TFiltered AddFilterView(FilterKey filterKey, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer)
        {
            TFiltered newFilter = CreateChildCollection((TCollection)this, filter, comparer);
            _children.Add(filterKey, newFilter);
            return newFilter;
        }

        /// <summary>
        /// Removes the filter view associated with the specified filter key.
        /// </summary>
        /// <param name="filterKey">The key identifying the filter view to remove.</param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
        public bool RemoveFilterView(FilterKey filterKey)
        {
            return _children.Remove(filterKey);
        } 

        #endregion Filter Methods
    }

}

