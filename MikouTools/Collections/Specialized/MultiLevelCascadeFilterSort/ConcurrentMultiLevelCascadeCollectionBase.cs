using MikouTools.Collections.Optimized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// The type of the collection itself. This must derive from ConcurrentMultiLevelCascadeCollectionBase.
    /// </typeparam>
    /// <typeparam name="TFiltered">
    /// The type of the filtered view. This must derive from ConcurrentMultiLevelCascadeFilteredViewBase.
    /// </typeparam>
    public abstract class ConcurrentMultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered> : IMultiLevelCascadeCollection<FilterKey, ItemValue, TFiltered>
     where FilterKey : notnull
     where ItemValue : notnull
     where TCollection : ConcurrentMultiLevelCascadeCollectionBase<FilterKey, ItemValue, TCollection, TFiltered>
     where TFiltered : ConcurrentMultiLevelCascadeFilteredViewBase<FilterKey, ItemValue, TCollection, TFiltered>
    {
        // Dictionary storing base items using an integer key.
        internal ConcurrentDualKeyDictionary<int, ItemValue> _baseList = [];

        // Dictionary holding the child filtered views associated with filter keys.
        protected readonly Dictionary<FilterKey, TFiltered> _children = [];

        // Stack of available (reusable) IDs.
        private readonly Stack<int> _availableIds = [];

        // The next unique ID to assign when no reusable IDs are available.
        private int _nextId = 0;

        // Lock object for synchronizing access.
        protected readonly object _lock = new();

        /// <summary>
        /// Creates a new child filtered view using an optional filter function and comparer.
        /// </summary>
        protected abstract TFiltered CreateChildCollection(TCollection @base, Func<ItemValue, bool>? filter = null, IComparer<ItemValue>? comparer = null);

        /// <summary>
        /// Creates a new child filtered view using an optional filter function and comparison delegate.
        /// </summary>
        protected abstract TFiltered CreateChildCollection(TCollection @base, Func<ItemValue, bool>? filter = null, Comparison<ItemValue>? comparison = null);

        /// <summary>
        /// Returns all unique IDs from the base collection.
        /// </summary>
        public virtual IEnumerable<int> GetIDs()
        {
            lock (_lock)
            {
                // Return a copy of the keys.
                return [.. _baseList.Keys];
            }
        }

        /// <summary>
        /// Returns all item values from the base collection.
        /// </summary>
        public virtual IEnumerable<ItemValue> GetValues()
        {
            lock (_lock)
            {
                // Return a copy of the values.
                return [.. _baseList.Values];
            }
        }

        /// <summary>
        /// Gets or sets the item associated with the specified ID.
        /// </summary>
        public virtual ItemValue this[int id]
        {
            get
            {
                lock (_lock)
                {
                    return _baseList[id];
                }
            }
            set
            {
                lock (_lock)
                {
                    _baseList[id] = value;
                }
            }
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
        /// Generates a new unique ID, reusing an available ID if possible.
        /// </summary>
        private int NewId()
        {
            lock (_lock)
            {
                if (_availableIds.Count > 0)
                {
                    return _availableIds.Pop();
                }
                return _nextId++;
            }
        }

        /// <summary>
        /// Adds an item to the base collection, assigns it a unique ID,
        /// and propagates the addition to each child filtered view.
        /// </summary>
        public virtual int Add(ItemValue item)
        {
            lock (_lock)
            {
                int id = NewId();
                _baseList.Add(id, item);
                foreach (var child in _children.Values)
                {
                    child.Add(id);
                }
                return id;
            }
        }

        /// <summary>
        /// Adds a range of items to the base collection.
        /// </summary>
        public virtual void AddRange(IEnumerable<ItemValue> items)
        {
            lock (_lock)
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// Attempts to remove an item from the base collection by value.
        /// </summary>
        public virtual bool Remove(ItemValue item)
        {
            lock (_lock)
            {
                if (_baseList.TryGetKey(item, out int id))
                {
                    return RemoveId(id);
                }
                return false;
            }
        }

        /// <summary>
        /// Removes an item by its ID, recycles the ID,
        /// and propagates the removal to each child filtered view.
        /// </summary>
        public virtual bool RemoveId(int id)
        {
            lock (_lock)
            {
                if (_baseList.Remove(id))
                {
                    _availableIds.Push(id);
                    foreach (var child in _children.Values)
                    {
                        child.Remove(id);
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Retrieves the child filtered view for the specified filter key.
        /// </summary>
        public virtual TFiltered? GetFilterView(FilterKey key)
        {
            lock (_lock)
            {
                _children.TryGetValue(key, out TFiltered? view);
                return view;
            }
        }

        /// <summary>
        /// Adds a new filtered view using a filter function and an optional comparer.
        /// </summary>
        public virtual void AddFilterView(FilterKey filterKey, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer )
        {
            lock (_lock)
            {
                if (_children.ContainsKey(filterKey))
                {
                    throw new ArgumentException("A filtered view with the same key already exists.", nameof(filterKey));
                }
                var view = CreateChildCollection((TCollection)this, filter, comparer);
                _children.Add(filterKey, view);
            }
        }

        /// <summary>
        /// Adds a new filtered view using a filter function and an optional comparison delegate.
        /// </summary>
        public virtual void AddFilterView(FilterKey filterKey, Func<ItemValue, bool>? filter, Comparison<ItemValue> comparison)
        {
            lock (_lock)
            {
                if (_children.ContainsKey(filterKey))
                {
                    throw new ArgumentException("A filtered view with the same key already exists.", nameof(filterKey));
                }
                var view = CreateChildCollection((TCollection)this, filter, comparison);
                _children.Add(filterKey, view);
            }
        }

        /// <summary>
        /// Removes the filtered view associated with the specified filter key.
        /// </summary>
        public virtual void RemoveFilterView(FilterKey key)
        {
            lock (_lock)
            {
                _children.Remove(key);
            }
        }

    }

}
