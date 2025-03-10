using System.Diagnostics.CodeAnalysis;

namespace MikouTools.Collections.Optimized
{
    /// <summary>
    /// A dictionary that provides fast bidirectional lookup while ensuring both keys and values are unique.
    /// This dictionary maintains a strict one-to-one mapping, preventing duplicate keys or values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary. Must be non-nullable.</typeparam>
    public class DualKeyDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        private readonly Dictionary<TValue, TKey> _reverseDictionary = [];

        /// <summary>
        /// Initializes a new empty instance of the <see cref="DualKeyDictionary{TKey, TValue}"/> class.
        /// </summary>
        public DualKeyDictionary() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DualKeyDictionary{TKey, TValue}"/> class 
        /// with the specified key-value pair collection.
        /// </summary>
        /// <param name="collection">The collection of key-value pairs to initialize the dictionary with.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided collection is null.</exception>
        public DualKeyDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DualKeyDictionary{TKey, TValue}"/> class 
        /// with the specified dictionary and optional key comparer.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied to the new dictionary.</param>
        /// <param name="comparer">The comparer to use for the dictionary keys.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided dictionary is null.</exception>
        public DualKeyDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer = null) : base(dictionary?.Count ?? 0, comparer)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            foreach (var kvp in dictionary)
            {
                _reverseDictionary.Add(kvp.Value, kvp.Key);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DualKeyDictionary{TKey, TValue}"/> class 
        /// with the specified key-value pair collection.
        /// </summary>
        /// <param name="collection">The collection of key-value pairs to initialize the dictionary with.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided collection is null.</exception>
        public DualKeyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(collection, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DualKeyDictionary{TKey, TValue}"/> class 
        /// with the specified key-value pair collection and an optional key comparer.
        /// </summary>
        /// <param name="collection">The collection of key-value pairs to initialize the dictionary with.</param>
        /// <param name="comparer">The comparer to use for the dictionary keys.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided collection is null.</exception>
        public DualKeyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer) : base((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
        {
            ArgumentNullException.ThrowIfNull(collection);
            foreach (var kvp in collection)
            {
                _reverseDictionary.Add(kvp.Value, kvp.Key);
            }
        }


        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// Ensures that neither the key nor the value already exists in the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentException">Thrown when the key or value already exists.</exception>
        public virtual new void Add(TKey key, TValue value)
        {
            if (base.ContainsKey(key) || _reverseDictionary.ContainsKey(value))
                throw new ArgumentException("Item already exists in the dictionary.");

            base.Add(key, value);
            _reverseDictionary.Add(value, key);
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value is to be retrieved or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        /// <exception cref="ArgumentException">Thrown when setting a value that already exists in the dictionary.</exception>
        public virtual new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                if (_reverseDictionary.TryGetValue(value, out TKey? existingKey) && !existingKey.Equals(key))
                    throw new ArgumentException("Item already exists in the dictionary.");

                if (base.ContainsKey(key))
                {
                    TValue oldValue = base[key];
                    _reverseDictionary.Remove(oldValue);
                }


                base[key] = value;
                _reverseDictionary[value] = key;
            }
        }

        /// <summary>
        /// Removes the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false.</returns>
        public virtual new bool Remove(TKey key)
        {
            if (base.TryGetValue(key, out TValue? value))
            {
                return base.Remove(key) && _reverseDictionary.Remove(value);
            }
            return false;
        }

        /// <summary>
        /// Removes all elements from the dictionary.
        /// </summary>
        public virtual new void Clear()
        {
            base.Clear();
            _reverseDictionary.Clear();
        }

        /// <summary>
        /// Tries to add the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>true if the element was added; otherwise, false.</returns>
        public virtual new bool TryAdd(TKey key, TValue value)
        {
            if (base.ContainsKey(key) || _reverseDictionary.ContainsKey(value)) return false;

            base.Add(key, value);
            _reverseDictionary.Add(value, key);

            return true;
        }

        /// <summary>
        /// Removes and returns the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose associated value is to be removed and returned.</param>
        /// <returns>The value that was removed.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public virtual TValue Pop(TKey key)
        {
            if (base.TryGetValue(key, out TValue? value))
            {
                base.Remove(key);
                _reverseDictionary.Remove(value);
                return value;
            }
            throw new KeyNotFoundException();
        }

        public virtual bool TryPop(TKey key, [MaybeNullWhen(false)] out TValue? result)
        {
            if (base.TryGetValue(key, out result))
            {
                base.Remove(key);
                _reverseDictionary.Remove(result);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Tries to get the key associated with the specified value.
        /// </summary>
        /// <param name="value">The value whose associated key is to be retrieved.</param>
        /// <param name="key">When this method returns, contains the key associated with the specified value, if found; otherwise, the default value.</param>
        /// <returns>true if the key was found; otherwise, false.</returns>
        public virtual bool TryGetKey(TValue value, [MaybeNullWhen(false)] out TKey? key)
        {
            return _reverseDictionary.TryGetValue(value, out key);
        }

        public virtual new bool ContainsValue(TValue value) => _reverseDictionary.ContainsKey(value);
    }
}
