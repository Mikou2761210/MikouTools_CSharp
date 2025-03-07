using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Overrideable
{
    public class OverrideableList<T> : List<T>
    {

        public OverrideableList() : base() { }
        public OverrideableList(IEnumerable<T> collection) : base(collection) { }
        public OverrideableList(int capacity) : base(capacity) { }

        // Add
        public new virtual void Add(T item)
        {
            base.Add(item);
        }

        public new virtual void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);
        }

        // BinarySearch
        public new virtual int BinarySearch(T item)
        {
            return base.BinarySearch(item);
        }

        public new virtual int BinarySearch(T item, IComparer<T> comparer)
        {
            return base.BinarySearch(item, comparer);
        }

        public new virtual int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return base.BinarySearch(index, count, item, comparer);
        }

        // Clear
        public new virtual void Clear()
        {
            base.Clear();
        }

        // Contains
        public new virtual bool Contains(T item)
        {
            return base.Contains(item);
        }

        // CopyTo
        public new virtual void CopyTo(T[] array)
        {
            base.CopyTo(array);
        }

        public new virtual void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            base.CopyTo(index, array, arrayIndex, count);
        }

        // Exists / Find / FindAll / FindIndex / FindLast / FindLastIndex
        public new virtual bool Exists(Predicate<T> match)
        {
            return base.Exists(match);
        }

        public new virtual T? Find(Predicate<T> match)
        {
            return base.Find(match);
        }

        public new virtual List<T> FindAll(Predicate<T> match)
        {
            return base.FindAll(match);
        }

        public new virtual int FindIndex(Predicate<T> match)
        {
            return base.FindIndex(match);
        }

        public new virtual int FindIndex(int startIndex, Predicate<T> match)
        {
            return base.FindIndex(startIndex, match);
        }

        public new virtual int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return base.FindIndex(startIndex, count, match);
        }

        public new virtual T? FindLast(Predicate<T> match)
        {
            return base.FindLast(match);
        }

        public new virtual int FindLastIndex(Predicate<T> match)
        {
            return base.FindLastIndex(match);
        }

        public new virtual int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return base.FindLastIndex(startIndex, match);
        }

        public new virtual int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return base.FindLastIndex(startIndex, count, match);
        }

        // ForEach
        public new virtual void ForEach(Action<T> action)
        {
            base.ForEach(action);
        }

        // GetEnumerator
        public new virtual IEnumerator<T> GetEnumerator()
        {
            return base.GetEnumerator();
        }

        // GetRange
        public new virtual List<T> GetRange(int index, int count)
        {
            return base.GetRange(index, count);
        }

        // IndexOf
        public new virtual int IndexOf(T item)
        {
            return base.IndexOf(item);
        }

        public new virtual int IndexOf(T item, int index)
        {
            return base.IndexOf(item, index);
        }

        public new virtual int IndexOf(T item, int index, int count)
        {
            return base.IndexOf(item, index, count);
        }

        // Insert
        public new virtual void Insert(int index, T item)
        {
            base.Insert(index, item);
        }

        public new virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            base.InsertRange(index, collection);
        }

        // LastIndexOf
        public new virtual int LastIndexOf(T item)
        {
            return base.LastIndexOf(item);
        }

        public new virtual int LastIndexOf(T item, int index)
        {
            return base.LastIndexOf(item, index);
        }

        public new virtual int LastIndexOf(T item, int index, int count)
        {
            return base.LastIndexOf(item, index, count);
        }

        // Remove
        public new virtual bool Remove(T item)
        {
            return base.Remove(item);
        }

        public new virtual int RemoveAll(Predicate<T> match)
        {
            return base.RemoveAll(match);
        }

        public new virtual void RemoveAt(int index)
        {
            base.RemoveAt(index);
        }

        public new virtual void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
        }

        // Reverse
        public new virtual void Reverse()
        {
            base.Reverse();
        }

        public new virtual void Reverse(int index, int count)
        {
            base.Reverse(index, count);
        }

        // Sort
        public new virtual void Sort()
        {
            base.Sort();
        }

        public new virtual void Sort(IComparer<T> comparer)
        {
            base.Sort(comparer);
        }

        public new virtual void Sort(Comparison<T> comparison)
        {
            base.Sort(comparison);
        }

        public new virtual void Sort(int index, int count, IComparer<T> comparer)
        {
            base.Sort(index, count, comparer);
        }

        // ToArray
        public new virtual T[] ToArray()
        {
            return base.ToArray();
        }

        // Indexer
        public new virtual T this[int index]
        {
            get { return base[index]; }
            set { base[index] = value; }
        }

        // Count
        public new virtual int Count
        {
            get { return base.Count; }
        }
    }
}