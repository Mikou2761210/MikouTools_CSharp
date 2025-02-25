using MikouTools.Thread.Utils;
using System.Collections;

namespace MikouTools.Thread.ThreadSafe.Collections
{
    public class ThreadSafeList<TListType, TValue>(TListType _list) : List<TValue> where TListType : List<TValue>
    {
        private readonly object _lock = new();

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

        public sealed class LockHandle : IDisposable
        {
            readonly object _lock;
            private bool _disposed;
            public TListType List { get; }


            public LockHandle(object @lock, TListType list)
            {
                _lock = @lock;

                Monitor.Enter(_lock);

                List = list;

            }


            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    Monitor.Exit(_lock);
                }
            }
        }

        public void EnterLock()
        {
            Monitor.Enter(_lock);
        }

        public void ExitLock()
        {
            Monitor.Exit(_lock);
        }

        public LockHandle LockAndGetList()
        {
            return new LockHandle(_lock, _list);
        }


        // Sets or Gets the element at the given index.
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


        // Is this List synchronized (thread-safe)?

        // Synchronization root for this object.

        public IReadOnlyList<TValue> AccessListWhileLocked => _list;

        public IReadOnlyList<TValue> Snapshot()
        {
            lock (_lock)
            {
                return _list.ToArray(); // 配列を返すことで外部から変更不可能にする
            }
        }

        //ToString
        public override string ToString()
        {
            lock (_lock)
            {
                return $"ThreadSafeList<{typeof(TValue).Name}>: [{string.Join(", ", _list)}]";
            }
        }

        // Add
        public new void Add(TValue item)
        {
            lock (_lock)
            {
                _list.Add(item);
            }
        }

        public new void AddRange(IEnumerable<TValue> items)
        {
            lock (_lock)
            {
                _list.AddRange(items);
            }
        }

        public new void Insert(int index, TValue item)
        {
            lock (_lock)
            {
                _list.Insert(index, item);
            }
        }

        // Get
        public TValue Get(int index)
        {
            lock (_lock)
            {
                return _list[index];
            }
        }

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

        // GetRange
        public new IReadOnlyList<TValue> GetRange(int index, int count)
        {
            lock (_lock)
            {
                return _list.GetRange(index, count);
            }
        }

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
                    result = [];
                    return false;
                }
            }
        }

        // Remove
        public new bool Remove(TValue item)
        {
            lock (_lock)
            {
                return _list.Remove(item);
            }
        }

        public new void RemoveAt(int index)
        {
            lock (_lock)
            {
                _list.RemoveAt(index);
            }
        }

        public new void RemoveRange(int index, int count)
        {
            lock (_lock)
            {
                _list.RemoveRange(index, count);
            }
        }

        public TValue RemoveAndGet(int index)
        {
            lock (_lock)
            {
                TValue result = _list[index];
                _list.RemoveAt(index);
                return result;
            }
        }

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

        //Find
        public new TValue? Find(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.Find(match);
            }
        }

        public new List<TValue> FindAll(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.FindAll(match);
            }
        }

        public new int FindIndex(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.FindIndex(match);
            }
        }

        public new int FindLastIndex(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.FindLastIndex(match);
            }
        }

        public new TValue? FindLast(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.FindLast(match);
            }
        }

        //IndexOf
        public new int IndexOf(TValue item)
        {
            lock (_lock)
            {
                return _list.IndexOf(item);
            }
        }

        public new int LastIndexOf(TValue item)
        {
            lock (_lock)
            {
                return _list.LastIndexOf(item);
            }
        }

        //Contains
        public new bool Contains(TValue item)
        {
            lock (_lock)
            {
                return _list.Contains(item);
            }
        }

        //Exists
        public new bool Exists(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.Exists(match);
            }
        }

        //TrueForAll
        public new bool TrueForAll(Predicate<TValue> match)
        {
            lock (_lock)
            {
                return _list.TrueForAll(match);
            }
        }

        //ForEach
        public new void ForEach(Action<TValue> action)
        {
            lock (_lock)
            {
                _list.ForEach(action);
            }
        }

        //BinarySearch
        public new int BinarySearch(TValue item)
        {
            lock (_lock)
            {
                return _list.BinarySearch(item);
            }
        }

        public new int BinarySearch(TValue item, IComparer<TValue> comparer)
        {
            lock (_lock)
            {
                return _list.BinarySearch(item, comparer);
            }
        }

        //IEnumerator
        public new IEnumerator<TValue> GetEnumerator()
        {
            lock (_lock)
            {
                _list.GetEnumerator();
                return _list.AsReadOnly().GetEnumerator();
            }
        }

        //CopyTo
        public new void CopyTo(int index, TValue[] array, int arrayIndex, int count)
        {
            lock (_lock)
            {
                _list.CopyTo(index, array, arrayIndex, count);
            }
        }

        public new void CopyTo(TValue[] array, int arrayIndex)
        {
            lock (_lock)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        // Clear
        public new void Clear()
        {
            lock (_lock)
            {
                _list.Clear();
            }
        }

        // Count
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

        // Capacity
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

        // Additional utility methods
        public new TValue[] ToArray()
        {
            lock (_lock)
            {
                return _list.ToArray();
            }
        }

        public new void Sort()
        {
            lock (_lock)
            {
                _list.Sort();
            }
        }

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
