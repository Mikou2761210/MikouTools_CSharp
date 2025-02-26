using MikouTools.Collections.Optimized;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    /// <summary>
    /// Represents a base collection that supports multi-level cascade filtering.
    /// This abstract class provides the foundation for collections that maintain a base list of items 
    /// and allow multiple filtered views to be created and maintained. Filtered views automatically 
    /// update when items are added or removed from the base collection.
    /// </summary>
    /// <typeparam name="FilterKey">
    /// The type used for the key that identifies each filter view.
    /// </typeparam>
    /// <typeparam name="ItemValue">
    /// The type of items stored in the collection.
    /// </typeparam>
    /// <typeparam name="TCollection">
    /// The type of the collection itself. This must derive from MultiLevelCascadeCollectionBase.
    /// </typeparam>
    /// <typeparam name="TFiltered">
    /// The type of the filtered view. This must derive from MultiLevelCascadeFilteredViewBase.
    /// </typeparam>
    public abstract class MultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered>: IMultiLevelCascadeCollection<FilterKey, ItemValue, TFiltered>
        where FilterKey : notnull
        where ItemValue : notnull
        where TCollection : MultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered>
        where TFiltered : MultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        // Dictionary storing the base items, using an integer key.
        internal DualKeyDictionary<int, ItemValue> _baseList = [];

        // Dictionary that holds the child filtered views associated with filter keys.
        protected readonly Dictionary<FilterKey, TFiltered> _children = [];

        // Stack of available (reusable) IDs.
        private readonly Stack<int> _availableIds = [];

        // The next unique ID to assign when no reusable IDs are available.
        private int _nextId = 0;

        /// <summary>
        /// Creates a new child filtered view with an optional filter function and comparer.
        /// </summary>
        /// <param name="base">The base collection instance.</param>
        /// <param name="filter">An optional filter function to determine which items are included in the view.</param>
        /// <param name="comparer">An optional comparer for sorting items within the view.</param>
        /// <returns>A new filtered view instance.</returns>
        protected abstract TFiltered CreateChildCollection(TCollection @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null);

        /// <summary>
        /// Creates a new child filtered view with an optional filter function and a comparison delegate.
        /// </summary>
        /// <param name="base">The base collection instance.</param>
        /// <param name="filter">An optional filter function to determine which items are included in the view.</param>
        /// <param name="comparison">An optional comparison delegate for sorting items within the view.</param>
        /// <returns>A new filtered view instance.</returns>
        protected abstract TFiltered CreateChildCollection(TCollection @base, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null);

        /// <summary>
        /// Gets all the unique integer IDs of the items in the base collection.
        /// </summary>
        public virtual IEnumerable<int> GetIDs() => _baseList.Keys;

        /// <summary>
        /// Gets all the item values stored in the base collection.
        /// </summary>
        public virtual IEnumerable<ItemValue> GetValues() => _baseList.Values;

        /// <summary>
        /// Gets or sets the item at the specified ID in the collection.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <returns>The item corresponding to the specified ID.</returns>
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

        /// <summary>
        /// Generates a new unique ID for an item. If there are any available reusable IDs, it reuses one.
        /// Otherwise, it returns a new ID.
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


        public virtual int GetId(ItemValue item)
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
        public virtual int Add(ItemValue item)
        {
            int id = NewId();
            _baseList.Add(id, item);
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
        public virtual void AddRange(IEnumerable<ItemValue> items)
        {
            foreach (ItemValue item in items)
                Add(item);
        }

        /// <summary>
        /// Removes an item from the base collection by its value.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
        public virtual bool Remove(ItemValue item)
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
        public virtual bool RemoveId(int id)
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
        /// <param name="Key">The key identifying the filter view.</param>
        /// <returns>
        /// The filtered view if it exists; otherwise, null.
        /// </returns>
        public virtual TFiltered? GetFilterView(FilterKey Key)
        {
            if (_children.TryGetValue(Key, out TFiltered? value))
                return value;
            return null;
        }

        /// <summary>
        /// Adds a new filter view to the collection using a filter function and an optional comparer.
        /// The new view is created by calling the abstract CreateChildCollection method.
        /// </summary>
        /// <param name="filterKey">The key identifying the new filter view.</param>
        /// <param name="filter">
        /// An optional filter function to determine which items are included in the view.
        /// </param>
        /// <param name="comparer">
        /// An optional comparer for sorting items within the view.
        /// </param>
        public virtual void AddFilterView(FilterKey filterKey, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null)
        {
            _children.Add(filterKey, CreateChildCollection((TCollection)this, filter, comparer));
        }

        /// <summary>
        /// Adds a new filter view to the collection using a filter function and an optional comparison delegate.
        /// The new view is created by calling the abstract CreateChildCollection method.
        /// </summary>
        /// <param name="filterName">The key identifying the new filter view.</param>
        /// <param name="filter">
        /// An optional filter function to determine which items are included in the view.
        /// </param>
        /// <param name="comparison">
        /// An optional comparison delegate for sorting items within the view.
        /// </param>
        public virtual void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null)
        {
            _children.Add(filterName, CreateChildCollection((TCollection)this, filter, comparison));
        }

        /// <summary>
        /// Removes the filter view associated with the specified filter key.
        /// </summary>
        /// <param name="Key">The key identifying the filter view to remove.</param>
        public virtual void RemoveFilterView(FilterKey Key)
        {
            _children.Remove(Key);
        }

        #endregion Filter Methods
    }
}

