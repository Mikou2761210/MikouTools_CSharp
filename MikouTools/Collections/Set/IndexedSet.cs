using MikouTools.Collections.Dictionary.DualKey;
using System.Collections.Generic;

namespace MikouTools.Collections.Set
{
    public class IndexedSet<TValue> where TValue : notnull
    {
        readonly  DualKeyDictionary<int, TValue> _value = [];
        public int Count { get; private set; } = 0;
        public TValue this[int index]
        {
            get => _value[index];
            set
            {
                if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
                _value[index] = value;
            }
        }

        public IndexedSet()
        {

        }

        public void Add(TValue value)
        {
            _value[Count] = value;
            Count++;
        }
        public void AddRange(TValue[] values)
        {
            foreach (TValue value in values)
            {
                _value[Count] = value;
                Count++;
            }
        }
        public void Insert(int index, TValue value)
        {
            if ((uint)index > (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));

            for (int i = Count - 1; i >= index; i--)
            {
                UpdateIndex(i, i + 1);
            }

            _value[index] = value;
            Count++;
        }
        public void InsertRange(int index, TValue[] values)
        {
            if ((uint)index > (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
            ArgumentNullException.ThrowIfNull(values);
            if (values.Length == 0) return;

            for (int i = Count - 1; i >= index; i--)
            {
                UpdateIndex(i, i + values.Length);
            }

            for (int i = 0; i < values.Length; i++)
            {
                _value[index + i] = values[i];
            }
            Count += values.Length;
        }
        public bool Remove(TValue value)
        {
            int index = IndexOf(value);
            if (index != -1)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }
        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));


            for (int i = index; i < Count - 1; i++)
            {
                UpdateIndex(i + 1, i);
            }

            Count--;
            _value.Remove(Count);
        }
        public void RemoveRange(int index,int count)
        {
            if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            if (index + count >= Count) throw new ArgumentOutOfRangeException(nameof(index));

            for (int i = index; i < Count - count; i++)
            {
                UpdateIndex(i + count, i);
            }

            for (int i = 0; i < count; i++)
            {
                Count--;
                _value.Remove(Count);
            }
        }
        public bool Contains(TValue value) => _value.ContainsValue(value);
        public int IndexOf(TValue value)
        {
            if(_value.TryGetKey(value, out int result))
            {
                return result;
            }
            return -1;
        }

        void UpdateIndex(int oldIndex, int newIndex)
        {
            if (!_value.TryGetValue(oldIndex, out TValue? value))
            {
                throw new KeyNotFoundException("The given key was not found.");
            }

            if (_value.ContainsKey(newIndex))
            {
                throw new ArgumentException("The new index already exists.");
            }

            _value.Remove(oldIndex);
            _value[newIndex] = value;
        }


        /// <summary>
        /// Sorts the FastList in place using the specified comparer.
        /// </summary>
        /// <param name="comparer">
        /// The comparer to use when comparing elements.
        /// If null, the default comparer for <typeparamref name="TValue"/> is used.
        /// </param>
        public void Sort(IComparer<TValue>? comparer = null)
        {
            // Copy all items into an array
            TValue[] items = new TValue[Count];
            for (int i = 0; i < Count; i++)
            {
                items[i] = _value[i];
            }

            // Sort the array using Array.Sort
            Array.Sort(items, comparer);

            // Clear the underlying dictionary and re-add the sorted items with new sequential keys
            _value.Clear();
            for (int i = 0; i < items.Length; i++)
            {
                _value.Add(i, items[i]);
            }
        }
    }
}
