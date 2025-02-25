using MikouTools.Thread.Utils;
using System.Collections;

namespace MikouTools.Thread.ThreadSafe.Collections
{
    /// <summary>
    /// A thread-safe wrapper around a List. This class provides synchronized access to the inner list,
    /// ensuring that all operations are performed with proper locking.
    /// </summary>
    /// <typeparam name="TListType">A type that derives from List&lt;TValue&gt;.</typeparam>
    /// <typeparam name="TValue">The type of elements contained in the list.</typeparam>
    public class ThreadSafeList<TListType, TValue>(TListType _list) : List<TValue> where TListType : List<TValue>
    {
        // Private lock object used for synchronizing access to the list.
        private readonly object _lock = new();

        /// <summary>
        /// Gets the inner list in a thread-safe manner.
        /// </summary>
        public TListType InnerList
        {
            get
            {
                lock (_lock)
                {
                    return _list;
                }
            }
        }

        /// <summary>
        /// A helper class that represents a locked handle for accessing the inner list.
        /// When disposed, it releases the lock.
        /// </summary>
        public sealed class LockHandle : IDisposable
        {
            readonly object _lock;
            private bool _disposed;
            /// <summary>
            /// The inner list accessible while the lock is held.
            /// </summary>
            public TListType List { get; }

            /// <summary>
            /// Acquires the lock on the given object and provides access to the list.
            /// </summary>
            /// <param name="lock">The lock object.</param>
            /// <param name="list">The inner list.</param>
            public LockHandle(object @lock, TListType list)
            {
                _lock = @lock;
                // Acquire the lock.
                Monitor.Enter(_lock);
                List = list;
            }

            /// <summary>
            /// Releases the lock when disposing of the handle.
            /// </summary>
            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    Monitor.Exit(_lock);
                }
            }
        }

        /// <summary>
        /// Manually enters the lock. Use this method if you need to perform multiple operations under a single lock.
        /// </summary>
        public void EnterLock()
        {
            Monitor.Enter(_lock);
        }

        /// <summary>
        /// Exits the lock. Ensure that you call this after a corresponding EnterLock call.
        /// </summary>
        public void ExitLock()
        {
            Monitor.Exit(_lock);
        }

        /// <summary>
        /// Enters the lock and returns a LockHandle that provides access to the inner list.
        /// Dispose the handle to release the lock.
        /// </summary>
        /// <returns>A LockHandle instance containing the inner list.</returns>
        public LockHandle LockAndGetList()
        {
            return new LockHandle(_lock, _list);
        }

        // --- Indexer with Thread-Safety ---

        /// <summary>
        /// Gets or sets the element at the specified index in a thread-safe manner.
        /// </summary>
        public new TValue this[int index]
        {
            get
            {
                lock (_lock)
                {
                    return _list[index];
                }
            }
            set
            {
                lock (_lock)
                {
                    _list[index] = value;
                }
            }
        }

        // --- Read-Only Access and Snapshot Methods ---

        /// <summary>
        /// Provides read-only access to the inner list while under lock.
        /// </summary>
        public IReadOnlyList<TValue> AccessListWhileLocked => _list;

        /// <summary>
        /// Returns a snapshot of the current list as an array.
        /// The returned array is a copy to prevent external modification.
        /// </summary>
        public IReadOnlyList<TValue> Snapshot()
        {
            lock (_lock)
            {
                return _list.ToArray();
            }
        }

        /// <summary>
        /// Returns a string representation of the ThreadSafeList.
        /// </summary>
        public override string ToString()
        {
            lock (_lock)
            {
                return $"ThreadSafeList<{typeof(TValue).Name}>: [{string.Join(", ", _list)}]";
            }
        }

        // --- Add and Insert Operations ---

        /// <summary>
        /// Adds an item to the list in a thread-safe manner.
        /// </summary>
        public new void Add(TValue item)
        {
            lock (_lock)
            {
                _list.Add(item);
            }
        }

        /// <summary>
        /// Adds a range of items to the list in a thread-safe manner.
        /// </summary>
        public new void AddRange(IEnumerable<TValue> items)
        {
            lock (_lock)
            {
                _list.AddRange(items);
            }
        }

        /// <summary>
        /// Inserts an item at the specified index in a thread-safe manner.
        /// </summary>
        public new void Insert(int index, TValue item)
        {
            lock (_lock)
            {
                _list.Insert(index, item);
            }
        }

        // --- Get Operations ---

        /// <summary>
        /// Gets the item at the specified index in a thread-safe manner.
        /// </summary>
        public TValue Get(int index)
        {
            lock (_lock)
            {
                return _list[index];
            }
        }

        /// <summary>
        /// Attempts to get the item at the specified index in a thread-safe manner.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        /// <param name="result">The retrieved item if the index is valid; otherwise, default value.</param>
        /// <returns>True if the item was retrieved; otherwise, false.</returns>
        public bool TryGet(int index, out TValue result)
        {
            lock (_lock)
            {
                if (index >= 0 && index < _list.Count)
                {
                    result = _list[index];
                    return true;
                }
                else
                {
                    result = default!;
                    return false;
                }
            }
        }

        // --- Range Operations ---

        /// <summary>
        /// Gets a range of elements from the list in a thread-safe manner.
        /// </summary>
        public new IReadOnlyList<TValue> GetRange(int index, int count)
        {
            lock (_lock)
            {
                return _list.GetRange(index, count);
            }
        }

        /// <summary>
        /// Attempts to get a range of elements from the list.
        /// </summary>
        /// <param name="index">Starting index.</param>
        /// <param name="count">Number of elements to retrieve.</param>
        /// <param name="result">
        /// The resulting range as a read-only list if successful; otherwise, an empty list.
        /// </param>
        /// <returns>True if the range is valid and was retrieved; otherwise, false.</returns>
        public bool TryGetRange(int index, int count, out IReadOnlyList<TValue> result)
        {
            lock (_lock)
            {
                if (index >= 0 && count >= 0 && index <= _list.Count - count)
                {
                    result = _list.GetRange(index, count);
                    return true;
                }
                else
                {
                    result = new TValue[0];
                    return false;
                }
            }
        }

        // --- Remove Operations ---

        /// <summary>
        /// Removes the first occurrence of a specific item from the list in a thread-safe manner.
        /// </summary>
        public new bool Remove(TValue item)
        {
            lock (_lock)
            {
                return _list.Remove(item);
            }
        }

        /// <summary>
        /// Removes the element at the specified index in a thread-safe manner.
        /// </summary>
        public new void RemoveAt(int index)
        {
            lock (_lock)
            {
                _list.RemoveAt(index);
            }
        }

        /// <summary>
        /// Removes a range of elements from the list in a thread-safe manner.
        /// </summary>
        public new void RemoveRange(int index, int count)
        {
            lock (_lock)
            {
                _list.RemoveRange(index, count);
            }
        }

        /// <summary>
        /// Removes the element at the specified index and returns it in a thread-safe manner.
        /// </summary>
        public TValue RemoveAndGet(int index)
        {
            lock (_lock)
            {
                TValue result = _list[index];
                _list.RemoveAt(index);
                return result;
            }
        }

        /// <summary>
        /// Attempts to remove the element at the specified index and return it.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        /// <param name="result">The removed element if successful; otherwise, default value.</param>
        /// <returns>True if the removal was successful; otherwise, false.</returns>
        public bool TryRemoveAndGet(int index, out TValue result)
        {
            lock (_lock)
            {
                if (index >= 0 && index < _list.Count)
                {
                    result = _list[index];
                    _list.RemoveAt(index);
                    return true;
                }
                else
                {
                    result = default!;
                    return false;
                }
            }
        }

        // --- Search Operations ---

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate in a thread-safe manner.
        /// </summary>
        public new TValue? Find(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.Find(match);
            }
        }

        /// <summary>
        /// Retrieves all elements that match the conditions defined by the specified predicate in a thread-safe manner.
        /// </summary>
        public new List<TValue> FindAll(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.FindAll(match);
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions and returns the zero-based index of the first occurrence in a thread-safe manner.
        /// </summary>
        public new int FindIndex(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.FindIndex(match);
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions and returns the zero-based index of the last occurrence in a thread-safe manner.
        /// </summary>
        public new int FindLastIndex(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.FindLastIndex(match);
            }
        }

        /// <summary>
        /// Searches for the last element that matches the conditions in a thread-safe manner.
        /// </summary>
        public new TValue? FindLast(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.FindLast(match);
            }
        }

        // --- Index Lookup Operations ---

        /// <summary>
        /// Returns the zero-based index of the first occurrence of a value in a thread-safe manner.
        /// </summary>
        public new int IndexOf(TValue item)
        {
            lock (_lock)
            {
                return _list.IndexOf(item);
            }
        }

        /// <summary>
        /// Returns the zero-based index of the last occurrence of a value in a thread-safe manner.
        /// </summary>
        public new int LastIndexOf(TValue item)
        {
            lock (_lock)
            {
                return _list.LastIndexOf(item);
            }
        }

        /// <summary>
        /// Determines whether the list contains a specific value in a thread-safe manner.
        /// </summary>
        public new bool Contains(TValue item)
        {
            lock (_lock)
            {
                return _list.Contains(item);
            }
        }

        /// <summary>
        /// Determines whether the list contains elements that match the conditions defined by the specified predicate in a thread-safe manner.
        /// </summary>
        public new bool Exists(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.Exists(match);
            }
        }

        /// <summary>
        /// Determines whether every element in the list satisfies the conditions defined by the specified predicate in a thread-safe manner.
        /// </summary>
        public new bool TrueForAll(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.TrueForAll(match);
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the list in a thread-safe manner.
        /// </summary>
        public new void ForEach(Action<TValue> action)
        {
            lock (_lock)
            {
                _list.ForEach(action);
            }
        }

        // --- Binary Search Operations ---

        /// <summary>
        /// Searches the list for a value using a binary search algorithm in a thread-safe manner.
        /// </summary>
        public new int BinarySearch(TValue item)
        {
            lock (_lock)
            {
                return _list.BinarySearch(item);
            }
        }

        /// <summary>
        /// Searches the list for a value using a binary search algorithm and a custom comparer in a thread-safe manner.
        /// </summary>
        public new int BinarySearch(TValue item, IComparer<TValue> comparer)
        {
            lock (_lock)
            {
                return _list.BinarySearch(item, comparer);
            }
        }

        // --- Enumerator and Copy Operations ---

        /// <summary>
        /// Returns an enumerator that iterates through the list in a thread-safe manner.
        /// Note: Returns a read-only enumerator to prevent external modification.
        /// </summary>
        public new IEnumerator<TValue> GetEnumerator()
        {
            lock (_lock)
            {
                // Return a read-only enumerator of the list.
                _list.GetEnumerator();
                return _list.AsReadOnly().GetEnumerator();
            }
        }

        /// <summary>
        /// Copies a range of elements from the list to a compatible one-dimensional array in a thread-safe manner.
        /// </summary>
        public new void CopyTo(int index, TValue[] array, int arrayIndex, int count)
        {
            lock (_lock)
            {
                _list.CopyTo(index, array, arrayIndex, count);
            }
        }

        /// <summary>
        /// Copies the entire list to a compatible one-dimensional array, starting at the specified index, in a thread-safe manner.
        /// </summary>
        public new void CopyTo(TValue[] array, int arrayIndex)
        {
            lock (_lock)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        // --- Clear, Count, and Capacity ---

        /// <summary>
        /// Removes all elements from the list in a thread-safe manner.
        /// </summary>
        public new void Clear()
        {
            lock (_lock)
            {
                _list.Clear();
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the list in a thread-safe manner.
        /// </summary>
        public new int Count
        {
            get
            {
                lock (_lock)
                {
                    return _list.Count;
                }
            }
        }

        /// <summary>
        /// Gets or sets the total number of elements the list can hold without resizing, in a thread-safe manner.
        /// </summary>
        public new int Capacity
        {
            get
            {
                lock (_lock)
                {
                    return _list.Capacity;
                }
            }
            set
            {
                lock (_lock)
                {
                    _list.Capacity = value;
                }
            }
        }

        // --- Additional Utility Methods ---

        /// <summary>
        /// Returns an array containing all the elements in the list in a thread-safe manner.
        /// </summary>
        public new TValue[] ToArray()
        {
            lock (_lock)
            {
                return _list.ToArray();
            }
        }

        /// <summary>
        /// Sorts the elements in the entire list in a thread-safe manner.
        /// </summary>
        public new void Sort()
        {
            lock (_lock)
            {
                _list.Sort();
            }
        }

        /// <summary>
        /// Reverses the order of the elements in the entire list in a thread-safe manner.
        /// </summary>
        public new void Reverse()
        {
            lock (_lock)
            {
                _list.Reverse();
            }
        }
    }


    [Obsolete]
    public class OldThreadSafeList<T> : IList<T>, IReadOnlyList<T>
    {
        private readonly List<T> _list = [];
        private readonly object _lock = new();
        private readonly LockableProperty<bool> _allowAdd = new(true);

        public sealed class LockHandle : IDisposable
        {
            readonly object _lock;
            public List<T> List { get; }


            public LockHandle(object @lock, List<T> list)
            {
                _lock = @lock;

                Monitor.Enter(_lock);

                List = list;

            }


            readonly LockableProperty<bool> _dispose = new(false);

            public void Dispose()
            {
                if (!_dispose.SetAndReturnOld(true))
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        public void Lock()
        {
            Monitor.Enter(_lock);
        }

        public void UnLock()
        {
            Monitor.Exit(_lock);
        }

        public LockHandle LockAndGetList()
        {
            return new LockHandle(_lock, _list);
        }


        // Sets or Gets the element at the given index.
        public T this[int index]
        {
            get
            {
                lock (_lock)
                {
                    return _list[index];
                }
            }

            set
            {
                lock (_lock)
                {
                    _list[index] = value;
                }
            }
        }

        public bool AllowAdd
        {
            get { return _allowAdd.Value; }
            set { _allowAdd.Value = value; }
        }

        // Is this List read-only?
        bool ICollection<T>.IsReadOnly => false;

        // Is this List synchronized (thread-safe)?

        // Synchronization root for this object.

        public IReadOnlyList<T> AccessListWhileLocked => _list;

        public IReadOnlyList<T> Snapshot()
        {
            lock (_lock)
            {
                return [.. _list]; // 配列を返すことで外部から変更不可能にする
            }
        }

        //ToString
        public override string ToString()
        {
            lock (_lock)
            {
                return $"ThreadSafeList<{typeof(T).Name}>: [{string.Join(", ", _list)}]";
            }
        }

        // Add
        public void Add(T item)
        {
            lock (_lock)
            {
                if (AllowAdd)
                    _list.Add(item);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (_lock)
            {
                if (AllowAdd)
                    _list.AddRange(items);
            }
        }

        public void Insert(int index, T item)
        {
            lock (_lock)
            {
                if (AllowAdd)
                    _list.Insert(index, item);
            }
        }

        // Get
        public T Get(int index)
        {
            lock (_lock)
            {
                return _list[index];
            }
        }

        public bool TryGet(int index, out T result)
        {
            lock (_lock)
            {
                if (index >= 0 && index < _list.Count)
                {
                    result = _list[index];
                    return true;
                }
                else
                {
                    result = default!;
                    return false;
                }
            }
        }

        // GetRange
        public IReadOnlyList<T> GetRange(int index, int count)
        {
            lock (_lock)
            {
                return _list.GetRange(index, count);
            }
        }

        public bool TryGetRange(int index, int count, out IReadOnlyList<T> result)
        {
            lock (_lock)
            {
                if (index >= 0 && count >= 0 && index <= _list.Count - count)
                {
                    result = _list.GetRange(index, count);
                    return true;
                }
                else
                {
                    result = [];
                    return false;
                }
            }
        }

        // Remove
        public bool Remove(T item)
        {
            lock (_lock)
            {
                return _list.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
            {
                _list.RemoveAt(index);
            }
        }

        public void RemoveRange(int index, int count)
        {
            lock (_lock)
            {
                _list.RemoveRange(index, count);
            }
        }

        public T RemoveAndGet(int index)
        {
            lock (_lock)
            {
                T result = _list[index];
                _list.RemoveAt(index);
                return result;
            }
        }

        public bool TryRemoveAndGet(int index, out T result)
        {
            lock (_lock)
            {
                if (index >= 0 && index < _list.Count)
                {
                    result = _list[index];
                    _list.RemoveAt(index);
                    return true;
                }
                else
                {
                    result = default!;
                    return false;
                }
            }
        }

        //Find
        public T? Find(Predicate<T> match)
        {
            lock (_lock)
            {
                return _list.Find(match);
            }
        }

        public List<T> FindAll(Predicate<T> match)
        {
            lock (_lock)
            {
                return _list.FindAll(match);
            }
        }

        public int FindIndex(Predicate<T> match)
        {
            lock (_lock)
            {
                return _list.FindIndex(match);
            }
        }

        public int FindLastIndex(Predicate<T> match)
        {
            lock (_lock)
            {
                return _list.FindLastIndex(match);
            }
        }

        public T? FindLast(Predicate<T> match)
        {
            lock (_lock)
            {
                return _list.FindLast(match);
            }
        }

        //IndexOf
        public int IndexOf(T item)
        {
            lock (_lock)
            {
                return _list.IndexOf(item);
            }
        }

        public int LastIndexOf(T item)
        {
            lock (_lock)
            {
                return _list.LastIndexOf(item);
            }
        }

        //Contains
        public bool Contains(T item)
        {
            lock (_lock)
            {
                return _list.Contains(item);
            }
        }

        //Exists
        public bool Exists(Predicate<T> match)
        {
            lock (_lock)
            {
                return _list.Exists(match);
            }
        }

        //TrueForAll
        public bool TrueForAll(Predicate<T> match)
        {
            lock (_lock)
            {
                return _list.TrueForAll(match);
            }
        }

        //ForEach
        public void ForEach(Action<T> action)
        {
            lock (_lock)
            {
                _list.ForEach(action);
            }
        }

        //BinarySearch
        public int BinarySearch(T item)
        {
            lock (_lock)
            {
                return _list.BinarySearch(item);
            }
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            lock (_lock)
            {
                return _list.BinarySearch(item, comparer);
            }
        }

        //IEnumerator
        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
            {
                return _list.ToList().GetEnumerator(); // スナップショットを返す
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        //CopyTo
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            lock (_lock)
            {
                _list.CopyTo(index, array, arrayIndex, count);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lock)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        // Clear
        public void Clear()
        {
            lock (_lock)
            {
                _list.Clear();
            }
        }

        // Count
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _list.Count;
                }
            }
        }

        // Capacity
        public int Capacity
        {
            get
            {
                lock (_lock)
                {
                    return _list.Capacity;
                }
            }
            set
            {
                lock (_lock)
                {
                    _list.Capacity = value;
                }
            }
        }

        // Additional utility methods
        public T[] ToArray()
        {
            lock (_lock)
            {
                return [.. _list];
            }
        }

        public void Sort()
        {
            lock (_lock)
            {
                _list.Sort();
            }
        }

        public void Reverse()
        {
            lock (_lock)
            {
                _list.Reverse();
            }
        }
    }
    
}
