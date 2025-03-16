using MikouTools.Collections.Overrideable;
using System.Collections;

namespace MikouTools.Collections.Filterable
{
    /// <summary>
    /// A list that supports filtering. It maintains both the full list (inherited from the base)
    /// and a filtered list containing only items that satisfy a given predicate.
    /// Operations such as Add, Remove, and Sort update both lists accordingly.
    /// </summary>
    public class FilterableList<T> : OverrideableList<T>, IEnumerable<T>
    {
        #region Fields

        // Predicate function to determine if an item should be included in the filtered list.
        private Func<T, bool>? filterPredicate;

        // Internal list holding only the items that satisfy the filter predicate.
        private readonly List<T> filteredItems = new List<T>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the filter predicate. When the predicate is set, the filtered items list is rebuilt.
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
        /// </summary>
        public List<T> FilteredItems => filteredItems;

        /// <summary>
        /// If true, operations affect the full list (base list). If false, operations affect only the filtered list.
        /// </summary>
        public bool IsFullMode { get; set; } = true;

        #endregion

        #region Constructors

        public FilterableList() : base() { }

        public FilterableList(int capacity) : base(capacity) { }

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
                // Iterate using the base indexer to ensure we always use the full list.
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
            var list = IsFullMode ? (IList<T>)this : filteredItems;

            if (fromIndex < 0 || fromIndex >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex), "Source index is out of range.");
            if (toIndex < 0 || toIndex > list.Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex), "Destination index is out of range.");
            if (fromIndex == toIndex)
                return -1;

            T item = list[fromIndex];
            list.RemoveAt(fromIndex);

            // Adjust destination index if item was removed from before the target position.
            if (fromIndex < toIndex)
                toIndex--;

            list.Insert(toIndex, item);
            return toIndex;
        }

        #endregion

        #region Overridden Operations

        /// <summary>
        /// Adds an item to the full list. If the item satisfies the filter predicate,
        /// it is also added to the filtered list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public override void Add(T item)
        {
            base.Add(item);
            if (filterPredicate != null && filterPredicate(item))
                filteredItems.Add(item);
        }

        /// <summary>
        /// Inserts an item at the specified index in the full list. If the item satisfies the filter predicate,
        /// it is also added to the filtered list.
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
        /// Removes the first occurrence of an item from the full list. If the item satisfies the filter predicate,
        /// it is also removed from the filtered list.
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
        /// Removes the item at the specified index. The operation is performed on the full list if IsFullMode is true,
        /// otherwise it is performed on the filtered list.
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
                // Also remove the corresponding item from the full list.
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
                    bool wasFiltered = (filterPredicate != null && filterPredicate(oldItem));
                    base[index] = value;
                    bool isFiltered = (filterPredicate != null && filterPredicate(value));

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
        /// </summary>
        public override int Count => IsFullMode ? base.Count : filteredItems.Count;

        /// <summary>
        /// Sorts the elements in a range of the list. If operating on the full list,
        /// the filtered list is rebuilt afterwards.
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
        /// Returns an enumerator that iterates through the list, depending on the current mode.
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
        /// Returns a non-generic enumerator that iterates through the list.
        /// </summary>
        /// <returns>A non-generic enumerator for the list.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
