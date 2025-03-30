using MikouTools.Collections.ListEx.Overrideable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.ListEx.Filterable
{
    public class AdvancedFilterableList<T> : IEnumerable<T> 
    {
        #region Fields

        protected virtual Func<T, bool>? _filterPredicate { get; set; }

        protected virtual OverrideableList<T> _fullItems { get; set; }
        protected virtual OverrideableList<T> _filteredItems { get; set; } = [];

        public virtual IReadOnlyList<T> FilteredList => _filteredItems;

        public virtual IReadOnlyList<T> FullList => _fullItems;

        #endregion

        #region Properties

        public virtual Func<T, bool>? FilterPredicate
        {
            get => _filterPredicate;
            set
            {
                _filterPredicate = value;
                RebuildFilteredItems();
            }
        }

        public virtual List<T> FilteredItems => _filteredItems;

        public virtual bool IsFullMode { get; set; } = true;

        #endregion

        #region Constructors

        public AdvancedFilterableList() : base() { _fullItems = []; }

        public AdvancedFilterableList(int capacity) { _fullItems = new(capacity); }

        public AdvancedFilterableList(IEnumerable<T> collection)
        {
            _fullItems = [.. collection];
            RebuildFilteredItems();
        }

        #endregion

        #region Core Methods

        public virtual void RebuildFilteredItems()
        {
            _filteredItems.Clear();
            if (_filterPredicate != null)
            {
                // Use the base indexer to ensure the entire underlying list is considered.
                for (int i = 0; i < _fullItems.Count; i++)
                {
                    T item = _fullItems[i];
                    if (_filterPredicate(item))
                        _filteredItems.Add(item);
                }
            }
        }

        public virtual int Move(int fromIndex, int toIndex)
        {
            var list = IsFullMode ? (IList<T>)this : _filteredItems;

            if (fromIndex < 0 || fromIndex >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex), "Source index is out of range.");
            if (toIndex < 0 || toIndex > list.Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex), "Destination index is out of range.");
            if (fromIndex == toIndex)
                return -1;

            T item = list[fromIndex];
            list.RemoveAt(fromIndex);

            if (fromIndex < toIndex)
                toIndex--;

            list.Insert(toIndex, item);
            return toIndex;
        }

        #endregion




        #region Transferred Method

        public virtual void Add(T item)
        {
            _fullItems.Add(item);
            if (_filterPredicate != null && _filterPredicate(item))
                _filteredItems.Add(item);
        }

        public virtual void Insert(int index, T item)
        {
            _fullItems.Insert(index, item);
            if (_filterPredicate != null && _filterPredicate(item))
                _filteredItems.Add(item);
        }

        public virtual bool Remove(T item)
        {
            bool removed = _fullItems.Remove(item);
            if (removed && _filterPredicate != null && _filterPredicate(item))
                _filteredItems.Remove(item);
            return removed;
        }

        public virtual void RemoveAt(int index)
        {
            if (IsFullMode)
            {
                T item = _fullItems[index];
                _fullItems.RemoveAt(index);
                if (_filterPredicate != null && _filterPredicate(item))
                    _filteredItems.Remove(item);
            }
            else
            {
                T item = _filteredItems[index];
                _filteredItems.RemoveAt(index);
                int fullIndex = _fullItems.IndexOf(item);
                if (fullIndex >= 0)
                    _fullItems.RemoveAt(fullIndex);
            }
        }

        public virtual void Clear()
        {
            _fullItems.Clear();
            _filteredItems.Clear();
        }
        public virtual T this[int index]
        {
            get => IsFullMode ? _fullItems[index] : _filteredItems[index];
            set
            {
                if (IsFullMode)
                {
                    T oldItem = _fullItems[index];
                    bool wasFiltered = _filterPredicate != null && _filterPredicate(oldItem);
                    _fullItems[index] = value;
                    bool isFiltered = _filterPredicate != null && _filterPredicate(value);

                    if (wasFiltered)
                    {
                        int filteredIndex = _filteredItems.IndexOf(oldItem);
                        if (filteredIndex >= 0)
                        {
                            if (isFiltered)
                                _filteredItems[filteredIndex] = value;
                            else
                                _filteredItems.RemoveAt(filteredIndex);
                        }
                    }
                    else if (isFiltered)
                    {
                        _filteredItems.Add(value);
                    }
                }
                else
                {
                    T oldItem = _filteredItems[index];
                    _filteredItems[index] = value;
                    int fullIndex = _fullItems.IndexOf(oldItem);
                    if (fullIndex >= 0)
                        _fullItems[fullIndex] = value;
                }
            }
        }

        public virtual int Count => IsFullMode ? _fullItems.Count : _filteredItems.Count;

        public virtual void Sort()
        {
            Sort(0, Count, null);
        }

        public virtual void Sort(IComparer<T>? comparer)
        {
            Sort(0, Count, comparer);
        }

        public virtual void Sort(Comparison<T> comparison)
        {
            Sort(0, Count, Comparer<T>.Create(comparison));
        }
        public virtual void Sort(int index, int count, IComparer<T>? comparer)
        {
            if (IsFullMode)
            {
                _fullItems.Sort(index, count, comparer);
                RebuildFilteredItems();
            }
            else
            {
                _filteredItems.Sort(index, count, comparer);
            }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return IsFullMode ? _fullItems.GetEnumerator() : _filteredItems.GetEnumerator();
        }

        public virtual IEnumerator<T> BaseGetEnumerator() => _fullItems.GetEnumerator();

        public virtual IEnumerator<T> FilterGetEnumerator() => _filteredItems.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Forwarded List Methods

        public virtual bool Contains(T item) =>
            IsFullMode ? _fullItems.Contains(item) : _filteredItems.Contains(item);

        public virtual int IndexOf(T item) =>
            IsFullMode ? _fullItems.IndexOf(item) : _filteredItems.IndexOf(item);

        public virtual int LastIndexOf(T item) =>
            IsFullMode ? _fullItems.LastIndexOf(item) : _filteredItems.LastIndexOf(item);

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            if (IsFullMode)
                _fullItems.CopyTo(array, arrayIndex);
            else
                _filteredItems.CopyTo(array, arrayIndex);
        }

        public virtual void AddRange(IEnumerable<T> collection)
        {
            _fullItems.AddRange(collection);
            RebuildFilteredItems();
        }

        public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            _fullItems.InsertRange(index, collection);
            RebuildFilteredItems();
        }

        public virtual void RemoveRange(int index, int count)
        {
            if (IsFullMode)
            {
                _fullItems.RemoveRange(index, count);
                RebuildFilteredItems();
            }
            else
            {
                var itemsToRemove = _filteredItems.GetRange(index, count);
                _filteredItems.RemoveRange(index, count);
                foreach (var item in itemsToRemove)
                {
                    _fullItems.Remove(item);
                }
            }
        }

        public virtual int RemoveAll(Predicate<T> match)
        {
            if (IsFullMode)
            {
                int removed = _fullItems.RemoveAll(match);
                RebuildFilteredItems();
                return removed;
            }
            else
            {
                var itemsToRemove = _filteredItems.Where(item => match(item)).ToList();
                foreach (var item in itemsToRemove)
                {
                    _filteredItems.Remove(item);
                    _fullItems.Remove(item);
                }
                return itemsToRemove.Count;
            }
        }

        public virtual int BinarySearch(T item, IComparer<T> comparer)
        {
            return IsFullMode ? _fullItems.BinarySearch(item, comparer) : _filteredItems.BinarySearch(item, comparer);
        }

        public virtual void Reverse()
        {
            if (IsFullMode)
            {
                _fullItems.Reverse();
                RebuildFilteredItems();
            }
            else
            {
                _filteredItems.Reverse();
            }
        }

        public virtual T[] ToArray() =>
            IsFullMode ? _fullItems.ToArray() : _filteredItems.ToArray();

        public virtual bool TrueForAll(Predicate<T> match) =>
            IsFullMode ? _fullItems.TrueForAll(match) : _filteredItems.TrueForAll(match);

        public virtual List<U> ConvertAll<U>(Converter<T, U> converter) =>
            IsFullMode ? _fullItems.ConvertAll(converter) : _filteredItems.ConvertAll(converter);

        public virtual bool Exists(Predicate<T> match) =>
            IsFullMode ? _fullItems.Exists(match) : _filteredItems.Exists(match);

        public virtual T? Find(Predicate<T> match) =>
            IsFullMode ? _fullItems.Find(match) : _filteredItems.Find(match);

        public virtual List<T> FindAll(Predicate<T> match) =>
            IsFullMode ? _fullItems.FindAll(match) : _filteredItems.FindAll(match);

        public virtual int FindIndex(Predicate<T> match) =>
            IsFullMode ? _fullItems.FindIndex(match) : _filteredItems.FindIndex(match);

        public virtual T? FindLast(Predicate<T> match) =>
            IsFullMode ? _fullItems.FindLast(match) : _filteredItems.FindLast(match);

        public virtual int FindLastIndex(Predicate<T> match) =>
            IsFullMode ? _fullItems.FindLastIndex(match) : _filteredItems.FindLastIndex(match);

        public virtual void ForEach(Action<T> action)
        {
            if (IsFullMode)
                _fullItems.ForEach(action);
            else
                _filteredItems.ForEach(action);
        }

        #endregion
    }
}
