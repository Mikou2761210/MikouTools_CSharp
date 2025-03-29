using MikouTools.Collections.ListEx.Overrideable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MikouTools.Collections.ListEx
{
    public class ListSwitcher<T>
    {
        string? _currentListName = null;
        public string? CurrentListName 
        { 
            get => _currentListName;
            set
            {
                _currentListName = value;
                _currentList = TryGet(_currentListName);
            }
        }
        OverrideableList<T>? _currentList = null;
        public OverrideableList<T>? CurrentList => _currentList;

        protected virtual Dictionary<string, OverrideableList<T>> _lists { get; set; } = [];

        public IReadOnlyCollection<string> ListNames => _lists.Keys;
        public IReadOnlyCollection<OverrideableList<T>> Lists => _lists.Values;


        public bool AddList(string name, OverrideableList<T> list) => _lists.TryAdd(name, list);
        public bool RemoveList(string name)
        {
            if (_lists.Remove(name))
            {
                if (name == CurrentListName)
                    CurrentListName = null;

                return true;
            }
            return false;
        }

        public OverrideableList<T> this[string name]
        {
            get => _lists[name];
            set => _lists[name] = value;
        }

        public bool TryGet(string name, [MaybeNullWhen(false)] out OverrideableList<T> list) => _lists.TryGetValue(name, out list);

        public OverrideableList<T>? TryGet(string? name) => (name == null) ? null : (TryGet(name, out OverrideableList<T>? list) ? list : null);



        /// <summary>
        /// _currentListがnullでないかチェックし、nullの場合は例外を投げる
        /// </summary>
        private void CheckCurrentList()
        {
            if (_currentList == null)
            {
                throw new InvalidOperationException("Current list is null.");
            }
        }

        // ここから下

        // Add 
        public virtual void Add(T item)
        {
            CheckCurrentList();
            _currentList!.Add(item);
        }

        public virtual void AddRange(IEnumerable<T> collection)
        {
            CheckCurrentList();
            _currentList!.AddRange(collection);
        }

        // BinarySearch
        public virtual int BinarySearch(T item)
        {
            CheckCurrentList();
            return _currentList!.BinarySearch(item);
        }

        public virtual int BinarySearch(T item, IComparer<T> comparer)
        {
            CheckCurrentList();
            return _currentList!.BinarySearch(item, comparer);
        }

        public virtual int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            CheckCurrentList();
            return _currentList!.BinarySearch(index, count, item, comparer);
        }

        // Clear
        public virtual void Clear()
        {
            CheckCurrentList();
            _currentList!.Clear();
        }

        // Contains
        public virtual bool Contains(T item)
        {
            CheckCurrentList();
            return _currentList!.Contains(item);
        }

        // CopyTo
        public virtual void CopyTo(T[] array)
        {
            CheckCurrentList();
            _currentList!.CopyTo(array);
        }

        public virtual void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            CheckCurrentList();
            _currentList!.CopyTo(index, array, arrayIndex, count);
        }

        // Exists / Find / FindAll / FindIndex / FindLast / FindLastIndex
        public virtual bool Exists(Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.Exists(match);
        }

        public virtual T? Find(Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.Find(match);
        }

        public virtual List<T> FindAll(Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.FindAll(match);
        }

        public virtual int FindIndex(Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.FindIndex(match);
        }

        public virtual int FindIndex(int startIndex, Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.FindIndex(startIndex, match);
        }

        public virtual int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.FindIndex(startIndex, count, match);
        }

        public virtual T? FindLast(Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.FindLast(match);
        }

        public virtual int FindLastIndex(Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.FindLastIndex(match);
        }

        public virtual int FindLastIndex(int startIndex, Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.FindLastIndex(startIndex, match);
        }

        public virtual int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.FindLastIndex(startIndex, count, match);
        }

        // ForEach
        public virtual void ForEach(Action<T> action)
        {
            CheckCurrentList();
            _currentList!.ForEach(action);
        }

        // GetEnumerator
        public virtual IEnumerator<T> GetEnumerator()
        {
            CheckCurrentList();
            return _currentList!.GetEnumerator();
        }

        // GetRange
        public virtual List<T> GetRange(int index, int count)
        {
            CheckCurrentList();
            return _currentList!.GetRange(index, count);
        }

        // IndexOf
        public virtual int IndexOf(T item)
        {
            CheckCurrentList();
            return _currentList!.IndexOf(item);
        }

        public virtual int IndexOf(T item, int index)
        {
            CheckCurrentList();
            return _currentList!.IndexOf(item, index);
        }

        public virtual int IndexOf(T item, int index, int count)
        {
            CheckCurrentList();
            return _currentList!.IndexOf(item, index, count);
        }

        // Insert
        public virtual void Insert(int index, T item)
        {
            CheckCurrentList();
            _currentList!.Insert(index, item);
        }

        public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            CheckCurrentList();
            _currentList!.InsertRange(index, collection);
        }

        // LastIndexOf
        public virtual int LastIndexOf(T item)
        {
            CheckCurrentList();
            return _currentList!.LastIndexOf(item);
        }

        public virtual int LastIndexOf(T item, int index)
        {
            CheckCurrentList();
            return _currentList!.LastIndexOf(item, index);
        }

        public virtual int LastIndexOf(T item, int index, int count)
        {
            CheckCurrentList();
            return _currentList!.LastIndexOf(item, index, count);
        }

        // Remove
        public virtual bool Remove(T item)
        {
            CheckCurrentList();
            return _currentList!.Remove(item);
        }

        public virtual int RemoveAll(Predicate<T> match)
        {
            CheckCurrentList();
            return _currentList!.RemoveAll(match);
        }

        public virtual void RemoveAt(int index)
        {
            CheckCurrentList();
            _currentList!.RemoveAt(index);
        }

        public virtual void RemoveRange(int index, int count)
        {
            CheckCurrentList();
            _currentList!.RemoveRange(index, count);
        }

        // Reverse
        public virtual void Reverse()
        {
            CheckCurrentList();
            _currentList!.Reverse();
        }

        public virtual void Reverse(int index, int count)
        {
            CheckCurrentList();
            _currentList!.Reverse(index, count);
        }

        // Sort
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
            CheckCurrentList();
            _currentList!.Sort(index, count, comparer);
        }

        // ToArray
        public virtual T[] ToArray()
        {
            CheckCurrentList();
            return _currentList!.ToArray();
        }

        // Indexer
        public virtual T this[int index]
        {
            get
            {
                CheckCurrentList();
                return _currentList![index];
            }
            set
            {
                CheckCurrentList();
                _currentList![index] = value;
            }
        }

        // Count
        public virtual int Count
        {
            get
            {
                CheckCurrentList();
                return _currentList!.Count;
            }
        }
    }
}
