using MikouTools.Collections.List.Overrideable;
using System.Collections;

namespace MikouTools.Collections.List.Filterable
{
    /// <summary>
    /// A base list class that provides full (unfiltered) access to the underlying items.
    /// This class serves as a base for lists that may apply filters but need to expose the complete collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class FullAccessList<T> : OverrideableList<T>, IEnumerable<T>
    {
        // Provides access to the full underlying list.
        // Using a virtual property allows derived classes to override the retrieval mechanism if needed.
        protected virtual List<T> GetFullItems => this;

        // Default constructor.
        public FullAccessList() : base() { }

        // Constructor with initial capacity.
        public FullAccessList(int capacity) : base(capacity) { }

        // Constructor that initializes the list with an existing collection.
        public FullAccessList(IEnumerable<T> collection) : base(collection) { }
    }

    /// <summary>
    /// A list that supports filtering. It maintains both the full list (inherited from FullAccessList)
    /// and a filtered list containing only items that satisfy a given predicate.
    /// Operations such as Add, Remove, and Sort update both lists accordingly.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class FilterableList<T> : FullAccessList<T>, IEnumerable<T>
    {
        #region Fields

        // Predicate function to determine if an item should be included in the filtered list.
        private Func<T, bool>? filterPredicate;

        // Internal list holding only the items that satisfy the filter predicate.
        private readonly List<T> filteredItems = new List<T>();

        // Exposes the filtered items as a read-only list.
        public IReadOnlyList<T> FilteredList => filteredItems;

        // Exposes the full (unfiltered) items from the base class.
        public IReadOnlyList<T> FullList => base.GetFullItems;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the filter predicate.
        /// When the predicate is set, the filtered items list is rebuilt to reflect the new filter.
        /// </summary>
        public Func<T, bool>? FilterPredicate
        {
            get => filterPredicate;
            set
            {
                filterPredicate = value;
                RebuildFilteredItems();
            }
        }

        /// <summary>
        /// Gets the list of items that satisfy the filter predicate.
        /// This property returns the modifiable internal list, which might be used internally.
        /// For external consumption, consider using <see cref="FilteredList"/>.
        /// </summary>
        public List<T> FilteredItems => filteredItems;

        /// <summary>
        /// Determines whether operations such as Add, Remove, and indexing affect the full list (if true)
        /// or only the filtered list (if false).
        /// </summary>
        public bool IsFullMode { get; set; } = true;

        #endregion

        #region Constructors

        // Default constructor.
        public FilterableList() : base() { }

        // Constructor with an initial capacity.
        public FilterableList(int capacity) : base(capacity) { }

        // Constructor that initializes the list with an existing collection.
        // Also rebuilds the filtered list based on the current filter predicate (if any).
        public FilterableList(IEnumerable<T> collection) : base(collection)
        {
            RebuildFilteredItems();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Rebuilds the filteredItems list by iterating over the full list and selecting items
        /// that satisfy the filter predicate.
        /// </summary>
        public void RebuildFilteredItems()
        {
            filteredItems.Clear();
            if (filterPredicate != null)
            {
                // Use the base indexer to ensure the entire underlying list is considered.
                for (int i = 0; i < base.Count; i++)
                {
                    T item = base[i];
                    if (filterPredicate(item))
                        filteredItems.Add(item);
                }
            }
        }

        /// <summary>
        /// Moves an item from one index to another within the current mode (full or filtered).
        /// </summary>
        /// <param name="fromIndex">The source index.</param>
        /// <param name="toIndex">The destination index.</param>
        /// <returns>The new index of the moved item, or -1 if no move occurred.</returns>
        public int Move(int fromIndex, int toIndex)
        {
            // Select the list based on current mode: full list if IsFullMode is true, otherwise the filtered list.
            var list = IsFullMode ? (IList<T>)this : filteredItems;

            if (fromIndex < 0 || fromIndex >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex), "Source index is out of range.");
            if (toIndex < 0 || toIndex > list.Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex), "Destination index is out of range.");
            if (fromIndex == toIndex)
                return -1;

            T item = list[fromIndex];
            list.RemoveAt(fromIndex);

            // Adjust destination index if the item was removed from a position before the target.
            if (fromIndex < toIndex)
                toIndex--;

            list.Insert(toIndex, item);
            return toIndex;
        }

        #endregion

        #region Overridden Operations

        /// <summary>
        /// Adds an item to the full list.
        /// If the item satisfies the filter predicate, it is also added to the filtered list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public override void Add(T item)
        {
            base.Add(item);
            if (filterPredicate != null && filterPredicate(item))
                filteredItems.Add(item);
        }

        /// <summary>
        /// Inserts an item at the specified index in the full list.
        /// If the item satisfies the filter predicate, it is also added to the filtered list.
        /// </summary>
        /// <param name="index">The index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        public override void Insert(int index, T item)
        {
            base.Insert(index, item);
            if (filterPredicate != null && filterPredicate(item))
                filteredItems.Add(item);
        }

        /// <summary>
        /// Removes the first occurrence of an item from the full list.
        /// If the item satisfies the filter predicate, it is also removed from the filtered list.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
        public override bool Remove(T item)
        {
            bool removed = base.Remove(item);
            if (removed && filterPredicate != null && filterPredicate(item))
                filteredItems.Remove(item);
            return removed;
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// The operation is performed on the full list if IsFullMode is true,
        /// otherwise it is performed on the filtered list and the corresponding item is removed from the full list.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public override void RemoveAt(int index)
        {
            if (IsFullMode)
            {
                T item = base[index];
                base.RemoveAt(index);
                if (filterPredicate != null && filterPredicate(item))
                    filteredItems.Remove(item);
            }
            else
            {
                T item = filteredItems[index];
                filteredItems.RemoveAt(index);
                // Remove the corresponding item from the full list.
                int fullIndex = base.IndexOf(item);
                if (fullIndex >= 0)
                    base.RemoveAt(fullIndex);
            }
        }

        /// <summary>
        /// Clears all items from both the full list and the filtered list.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            filteredItems.Clear();
        }

        /// <summary>
        /// Gets or sets the element at the specified index, depending on the current mode (full or filtered).
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>The element at the specified index.</returns>
        public override T this[int index]
        {
            get => IsFullMode ? base[index] : filteredItems[index];
            set
            {
                if (IsFullMode)
                {
                    T oldItem = base[index];
                    bool wasFiltered = filterPredicate != null && filterPredicate(oldItem);
                    base[index] = value;
                    bool isFiltered = filterPredicate != null && filterPredicate(value);

                    if (wasFiltered)
                    {
                        int filteredIndex = filteredItems.IndexOf(oldItem);
                        if (filteredIndex >= 0)
                        {
                            if (isFiltered)
                                filteredItems[filteredIndex] = value;
                            else
                                filteredItems.RemoveAt(filteredIndex);
                        }
                    }
                    else if (isFiltered)
                    {
                        filteredItems.Add(value);
                    }
                }
                else
                {
                    T oldItem = filteredItems[index];
                    filteredItems[index] = value;
                    int fullIndex = base.IndexOf(oldItem);
                    if (fullIndex >= 0)
                        base[fullIndex] = value;
                }
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the list, depending on the current mode.
        /// Returns the count of the full list when in full mode, or the filtered list count otherwise.
        /// </summary>
        public override int Count => IsFullMode ? base.Count : filteredItems.Count;

        /// <summary>
        /// Sorts a range of elements in the list.
        /// If operating on the full list, the filtered list is rebuilt afterwards.
        /// Otherwise, only the filtered list is sorted.
        /// </summary>
        /// <param name="index">The starting index of the range to sort.</param>
        /// <param name="count">The number of elements to sort.</param>
        /// <param name="comparer">The comparer to use, or null to use the default comparer.</param>
        public override void Sort(int index, int count, IComparer<T>? comparer)
        {
            if (IsFullMode)
            {
                base.Sort(index, count, comparer);
                RebuildFilteredItems();
            }
            else
            {
                filteredItems.Sort(index, count, comparer);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the list,
        /// returning elements from the full list if in full mode, or from the filtered list otherwise.
        /// </summary>
        /// <returns>An enumerator for the list.</returns>
        public override IEnumerator<T> GetEnumerator()
        {
            if (IsFullMode)
                return base.GetEnumerator();
            else
                return filteredItems.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator for the full (unfiltered) list.
        /// </summary>
        public virtual IEnumerator<T> BaseGetEnumerator() => base.GetEnumerator();

        /// <summary>
        /// Returns an enumerator for the filtered list.
        /// </summary>
        public virtual IEnumerator<T> FilterGetEnumerator() => filteredItems.GetEnumerator();

        /// <summary>
        /// Returns a non-generic enumerator that iterates through the list.
        /// This method supports older interfaces.
        /// </summary>
        /// <returns>A non-generic enumerator for the list.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

}
