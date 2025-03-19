using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Dictionary.DualKey
{
    public class ConcurrentDualKeyDictionary<TKey, TValue> : DualKeyDictionary<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        private readonly object _lock = new();
        private readonly Dictionary<TValue, TKey> _reverseDictionary = [];



        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// Ensures that neither the key nor the value already exists in the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentException">Thrown when the key or value already exists.</exception>
        public override void Add(TKey key, TValue value)
        {
            lock (_lock)
            {

                if (base.ContainsKey(key) || _reverseDictionary.ContainsKey(value))
                    throw new ArgumentException("Item already exists in the dictionary.");

                base.Add(key, value);
                _reverseDictionary.Add(value, key);
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value is to be retrieved or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        /// <exception cref="ArgumentException">Thrown when setting a value that already exists in the dictionary.</exception>
        public override TValue this[TKey key]
        {
            get {  lock (_lock) { return base[key]; }; }
            set
            {
                lock (_lock)
                {
                    if (base.ContainsKey(key) || _reverseDictionary.ContainsKey(value))
                        throw new ArgumentException("Item already exists in the dictionary.");

                    base[key] = value;
                    _reverseDictionary[value] = key;
                }
            }
        }

        /// <summary>
        /// Removes the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false.</returns>
        public override bool Remove(TKey key)
        {
            lock (_lock)
            {
                if (base.TryGetValue(key, out TValue? value))
                {
                    return base.Remove(key) && _reverseDictionary.Remove(value);
                }
                return false;
            }
        }

        /// <summary>
        /// Removes all elements from the dictionary.
        /// </summary>
        public override void Clear()
        {
            lock (_lock)
            {
                base.Clear();
                _reverseDictionary.Clear();
            }
        }

        /// <summary>
        /// Tries to add the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>true if the element was added; otherwise, false.</returns>
        public override bool TryAdd(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (base.ContainsKey(key) || _reverseDictionary.ContainsKey(value)) return false;

                base.Add(key, value);
                _reverseDictionary.Add(value, key);

                return true;
            }
        }

        /// <summary>
        /// Removes and returns the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose associated value is to be removed and returned.</param>
        /// <returns>The value that was removed.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public override TValue Pop(TKey key)
        {
            lock (_lock)
            {

                if (base.TryGetValue(key, out TValue? value))
                {
                    base.Remove(key);
                    _reverseDictionary.Remove(value);
                    return value;
                }
                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Tries to get the key associated with the specified value.
        /// </summary>
        /// <param name="value">The value whose associated key is to be retrieved.</param>
        /// <param name="key">When this method returns, contains the key associated with the specified value, if found; otherwise, the default value.</param>
        /// <returns>true if the key was found; otherwise, false.</returns>
        public override bool TryGetKey(TValue value, out TKey? key)
        {
            lock (_lock)
            {
                return _reverseDictionary.TryGetValue(value, out key);
            }
        }

        public override bool ContainsValue(TValue value)
        {
            lock (_lock)
            {
                return _reverseDictionary.ContainsKey(value);
            }
        }



        /// <summary>
        /// Returns whether the specified key exists.
        /// </summary>
        public new bool ContainsKey(TKey key)
        {
            lock (_lock)
            {
                return base.ContainsKey(key);
            }
        }

        /// <summary>
        /// Attempts to retrieve the value associated with the specified key.
        /// </summary>
        public new bool TryGetValue(TKey key, out TValue? value)
        {
            lock (_lock)
            {
                return base.TryGetValue(key, out value);
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
                    return base.Count;
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
                    return base.Keys.ToList().AsReadOnly();
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
                    return base.Values.ToList().AsReadOnly();
                }
            }
        }


        /// <summary>
        /// Gets an enumerator for iterating over the dictionary's elements (snapshot version).
        /// </summary>
        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_lock)
            {
                return base.GetEnumerator();
            }
        }

        public override string? ToString()
        {
            lock (_lock)
            {
                return base.ToString();
            }
        }
    }
}
