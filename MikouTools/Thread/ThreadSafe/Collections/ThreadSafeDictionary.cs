using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace MikouTools.Thread.ThreadSafe.Collections
{

    public class ThreadSafeDictionary<TDictionaryType, TKey, TValue>(TDictionaryType _dictionary) : Dictionary<TKey, TValue> where TKey : notnull where TDictionaryType : Dictionary<TKey, TValue>
    {
        private readonly object _lock = new();

        public TDictionaryType InnerDictionary
        {
            get
            {
                lock (_lock)
                {
                    return _dictionary;
                }
            }
        }

        /// <summary>
        /// Handle for maintaining lock state.
        /// Disposing this handle releases the lock.
        /// </summary>
        public sealed class LockHandle : IDisposable
        {
            private readonly object _lockObj;
            private bool _disposed;

            /// <summary>
            /// Gets a reference to the dictionary while the lock is held.
            /// Note: Direct operations on this reference are not thread-safe.
            /// </summary>
            public TDictionaryType Dictionary { get; }

            internal LockHandle(object lockObj, TDictionaryType dictionary)
            {
                _lockObj = lockObj;
                Monitor.Enter(_lockObj);
                Dictionary = dictionary;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    Monitor.Exit(_lockObj);
                }
            }
        }

        /// <summary>
        /// Acquires the lock and returns a reference to the dictionary under lock.
        /// Be sure to dispose of the handle after use.
        /// </summary>
        public LockHandle LockAndGetDictionary() => new(_lock, _dictionary);

        /// <summary>
        /// Explicitly acquires the lock.
        /// Remember to release it with ExitLock() afterwards.
        /// </summary>
        public void EnterLock() => Monitor.Enter(_lock);

        /// <summary>
        /// Explicitly releases the lock.
        /// </summary>
        public void ExitLock() => Monitor.Exit(_lock);

        /// <summary>
        /// Indexer: Gets or sets the value by key.
        /// </summary>
        public new TValue this[TKey key]
        {
            get
            {
                lock (_lock)
                {
                    return _dictionary[key];
                }
            }
            set
            {
                lock (_lock)
                {
                    _dictionary[key] = value;
                }
            }
        }

        /// <summary>
        /// Adds a key-value pair.
        /// </summary>
        public new void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                _dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Returns whether the specified key exists.
        /// </summary>
        public new bool ContainsKey(TKey key)
        {
            lock (_lock)
            {
                return _dictionary.ContainsKey(key);
            }
        }

        /// <summary>
        /// Attempts to retrieve the value associated with the specified key.
        /// </summary>
        public new bool TryGetValue(TKey key, out TValue? value)
        {
            lock (_lock)
            {
                return _dictionary.TryGetValue(key, out value);
            }
        }

        /// <summary>
        /// Removes the element with the specified key and returns whether it was successful.
        /// </summary>
        public new bool Remove(TKey key)
        {
            lock (_lock)
            {
                return _dictionary.Remove(key);
            }
        }

        /// <summary>
        /// Removes all elements.
        /// </summary>
        public new void Clear()
        {
            lock (_lock)
            {
                _dictionary.Clear();
            }
        }

        /// <summary>
        /// Returns the number of elements.
        /// </summary>
        public new int Count
        {
            get
            {
                lock (_lock)
                {
                    return _dictionary.Count;
                }
            }
        }

        /// <summary>
        /// Gets a snapshot (copy) of the keys.
        /// </summary>
        public new IReadOnlyCollection<TKey> Keys
        {
            get
            {
                lock (_lock)
                {
                    return _dictionary.Keys.ToList().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// Gets a snapshot (copy) of the values.
        /// </summary>
        public new IReadOnlyCollection<TValue> Values
        {
            get
            {
                lock (_lock)
                {
                    return _dictionary.Values.ToList().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// Gets a snapshot (copy) of the current dictionary.
        /// </summary>
        public IReadOnlyDictionary<TKey, TValue> Snapshot()
        {
            lock (_lock)
            {
                return new Dictionary<TKey, TValue>(_dictionary);
            }
        }

        /// <summary>
        /// Gets an enumerator for iterating over the dictionary's elements (snapshot version).
        /// </summary>
        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            Dictionary<TKey, TValue> snapshot;
            lock (_lock)
            {
                snapshot = new Dictionary<TKey, TValue>(_dictionary);
            }
            return snapshot.GetEnumerator();
        }

        public override string ToString()
        {
            lock (_lock)
            {
                string content = string.Join(", ", _dictionary.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                return $"ThreadSafeDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>: {{{content}}}";
            }
        }
    }
}
