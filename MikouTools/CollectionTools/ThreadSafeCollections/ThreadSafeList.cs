using MikouTools.UtilityTools.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikouTools.CollectionTools.ThreadSafeCollections
{

    public class ThreadSafeList<T> : IList<T>, IReadOnlyList<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly object _lock = new object();
        private readonly LockableProperty<bool> _allowAdd = new LockableProperty<bool>(true);

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


            LockableProperty<bool> _dispose = new LockableProperty<bool>(false);

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
                return _list.ToArray(); // 配列を返すことで外部から変更不可能にする
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
                    result = Array.Empty<T>();
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
                return _list.ToArray();
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

    [Obsolete]
    public class AdvancedThreadSafeList<T> : IList<T>, IReadOnlyList<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly LockableProperty<bool> _allowAdd = new LockableProperty<bool>(true);

        public sealed class LockHandle : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public List<T> List { get; }


            public LockHandle(SemaphoreSlim semaphore, List<T> list)
            {
                _semaphore = semaphore;
                List = list;

                semaphore.Wait();
            }

            LockableProperty<bool> _dispose = new LockableProperty<bool>(false);

            public void Dispose()
            {
                if (!_dispose.SetAndReturnOld(true))
                {
                    _semaphore.Release();
                }
            }
        }

        public LockHandle LockAndGetList()
        {
            return new LockHandle(_semaphore, _list);
        }

        public void Lock()
        {
            _semaphore.Wait();
        }
        public void UnLock()
        {
            _semaphore.Release();
        }

        // Properties
        public T this[int index]
        {
            get
            {
                _semaphore.Wait();
                try { return _list[index]; }
                finally { _semaphore.Release(); }
            }
            set
            {
                _semaphore.Wait();
                try { _list[index] = value; }
                finally { _semaphore.Release(); }
            }
        }

        public bool AllowAdd
        {
            get { return _allowAdd.Value; }
            set { _allowAdd.Value = value; }
        }

        bool ICollection<T>.IsReadOnly => false;

        public IReadOnlyList<T> AccessListWhileLocked => _list;

        public int Count
        {
            get
            {
                _semaphore.Wait();
                try { return _list.Count; }
                finally { _semaphore.Release(); }
            }
        }

        public int Capacity
        {
            get
            {
                _semaphore.Wait();
                try { return _list.Capacity; }
                finally { _semaphore.Release(); }
            }
            set
            {
                _semaphore.Wait();
                try { _list.Capacity = value; }
                finally { _semaphore.Release(); }
            }
        }

        // Methods

        // Add methods
        public void Add(T item)
        {
            _semaphore.Wait();
            try
            {
                if (AllowAdd) _list.Add(item);
            }
            finally { _semaphore.Release(); }
        }

        public void AddRange(IEnumerable<T> items)
        {
            _semaphore.Wait();
            try
            {
                if (AllowAdd) _list.AddRange(items);
            }
            finally { _semaphore.Release(); }
        }

        public void Insert(int index, T item)
        {
            _semaphore.Wait();
            try { if (AllowAdd) _list.Insert(index, item); }
            finally { _semaphore.Release(); }
        }

        // Remove methods
        public bool Remove(T item)
        {
            _semaphore.Wait();
            try { return _list.Remove(item); }
            finally { _semaphore.Release(); }
        }

        public void RemoveAt(int index)
        {
            _semaphore.Wait();
            try { _list.RemoveAt(index); }
            finally { _semaphore.Release(); }
        }

        public void RemoveRange(int index, int count)
        {
            _semaphore.Wait();
            try { _list.RemoveRange(index, count); }
            finally { _semaphore.Release(); }
        }

        public T RemoveAndGet(int index)
        {
            _semaphore.Wait();
            try
            {
                T result = _list[index];
                _list.RemoveAt(index);
                return result;
            }
            finally { _semaphore.Release(); }
        }

        public bool TryRemoveAndGet(int index, out T result)
        {
            _semaphore.Wait();
            try
            {
                if (index >= 0 && index < _list.Count)
                {
                    result = _list[index];
                    _list.RemoveAt(index);
                    return true;
                }
                result = default!;
                return false;
            }
            finally { _semaphore.Release(); }
        }

        // Access methods

        public IReadOnlyList<T> GetRange(int index, int count)
        {
            _semaphore.Wait();
            try { return _list.GetRange(index, count); }
            finally { _semaphore.Release(); }
        }

        public bool TryGetRange(int index, int count, out IReadOnlyList<T> result)
        {
            _semaphore.Wait();
            try
            {
                if (index >= 0 && count >= 0 && index <= _list.Count - count)
                {
                    result = _list.GetRange(index, count);
                    return true;
                }
                result = Array.Empty<T>();
                return false;
            }
            finally { _semaphore.Release(); }
        }

        public IReadOnlyList<T> Snapshot()
        {
            _semaphore.Wait();
            try { return _list.ToArray(); }
            finally { _semaphore.Release(); }
        }

        // Search methods
        public T? Find(Predicate<T> match)
        {
            _semaphore.Wait();
            try { return _list.Find(match); }
            finally { _semaphore.Release(); }
        }

        public List<T> FindAll(Predicate<T> match)
        {
            _semaphore.Wait();
            try { return _list.FindAll(match); }
            finally { _semaphore.Release(); }
        }

        public int FindIndex(Predicate<T> match)
        {
            _semaphore.Wait();
            try { return _list.FindIndex(match); }
            finally { _semaphore.Release(); }
        }

        public int FindLastIndex(Predicate<T> match)
        {
            _semaphore.Wait();
            try { return _list.FindLastIndex(match); }
            finally { _semaphore.Release(); }
        }

        public T? FindLast(Predicate<T> match)
        {
            _semaphore.Wait();
            try { return _list.FindLast(match); }
            finally { _semaphore.Release(); }
        }

        // Other utility methods
        public bool Contains(T item)
        {
            _semaphore.Wait();
            try { return _list.Contains(item); }
            finally { _semaphore.Release(); }
        }

        public bool Exists(Predicate<T> match)
        {
            _semaphore.Wait();
            try { return _list.Exists(match); }
            finally { _semaphore.Release(); }
        }

        public bool TrueForAll(Predicate<T> match)
        {
            _semaphore.Wait();
            try { return _list.TrueForAll(match); }
            finally { _semaphore.Release(); }
        }

        public void ForEach(Action<T> action)
        {
            _semaphore.Wait();
            try { _list.ForEach(action); }
            finally { _semaphore.Release(); }
        }

        public int IndexOf(T item)
        {
            _semaphore.Wait();
            try { return _list.IndexOf(item); }
            finally { _semaphore.Release(); }
        }

        public int LastIndexOf(T item)
        {
            _semaphore.Wait();
            try { return _list.LastIndexOf(item); }
            finally { _semaphore.Release(); }
        }

        public int BinarySearch(T item)
        {
            _semaphore.Wait();
            try { return _list.BinarySearch(item); }
            finally { _semaphore.Release(); }
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            _semaphore.Wait();
            try { return _list.BinarySearch(item, comparer); }
            finally { _semaphore.Release(); }
        }

        public void Sort()
        {
            _semaphore.Wait();
            try { _list.Sort(); }
            finally { _semaphore.Release(); }
        }

        public void Reverse()
        {
            _semaphore.Wait();
            try { _list.Reverse(); }
            finally { _semaphore.Release(); }
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            _semaphore.Wait();
            try { _list.CopyTo(index, array, arrayIndex, count); }
            finally { _semaphore.Release(); }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _semaphore.Wait();
            try { _list.CopyTo(array, arrayIndex); }
            finally { _semaphore.Release(); }
        }

        public T[] ToArray()
        {
            _semaphore.Wait();
            try { return _list.ToArray(); }
            finally { _semaphore.Release(); }
        }

        public void Clear()
        {
            _semaphore.Wait();
            try { _list.Clear(); }
            finally { _semaphore.Release(); }
        }

        // Enumerator
        public IEnumerator<T> GetEnumerator()
        {
            _semaphore.Wait();
            try { return _list.ToList().GetEnumerator(); }
            finally { _semaphore.Release(); }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // ToString
        public override string ToString()
        {
            _semaphore.Wait();
            try { return $"ThreadSafeList<{typeof(T).Name}>: [{string.Join(", ", _list)}]"; }
            finally { _semaphore.Release(); }
        }
    }
}
