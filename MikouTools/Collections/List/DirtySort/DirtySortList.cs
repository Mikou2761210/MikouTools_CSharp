using MikouTools.Collections.Interfaces;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MikouTools.Collections.List.DirtySort
{
    public interface IDirtySortList<T> : IList<T>, IDirtySort<T>;

    public class DirtySortList<T> : List<T>, IDirtySortList<T>
    {

        public virtual bool IsDirty { get; set; } = false;

        public virtual IComparer<T>? LastComparer { get; set; } = null;


        private void MarkDirty()
        {
            IsDirty = true;
        }


        public virtual new T this[int index]
        {
            get => base[index];
            set
            {
                DetachItemEventHandlers(base[index]);
                base[index] = value;
                AttachItemEventHandlers(value);
                MarkDirty();
            }
        }


        private void AttachItemEventHandlers(T item)
        {
            if (item is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += OnItemPropertyChanged;
            }
            if (item is INotifyCollectionChanged incc)
            {
                incc.CollectionChanged += OnItemCollectionChanged;
            }
        }
        private void DetachItemEventHandlers(T item)
        {
            if (item is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged -= OnItemPropertyChanged;
            }
            if (item is INotifyCollectionChanged incc)
            {
                incc.CollectionChanged -= OnItemCollectionChanged;
            }
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) => MarkDirty();

        private void OnItemCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => MarkDirty();



        public virtual new int Count => base.Count;

        public virtual bool IsReadOnly => false;

        public virtual new void Add(T item)
        {
            base.Add(item);
            AttachItemEventHandlers(item);
            MarkDirty();
        }

        public virtual new void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                AttachItemEventHandlers(item);
            }
            base.AddRange(items);
            MarkDirty();
        }


        public virtual new bool Remove(T item)
        {
            bool removed = base.Remove(item);
            if (removed)
            {
                DetachItemEventHandlers(item);
                MarkDirty();
            }
            return removed;
        }
        public virtual new void RemoveRange(int index, int count)
        {
            for (int i = index; i < index + count; i++)
            {
                DetachItemEventHandlers(base[i]);
            }
            base.RemoveRange(index, count);
            MarkDirty();
        }

        public virtual new void RemoveAt(int index)
        {
            DetachItemEventHandlers(base[index]);
            base.RemoveAt(index);
            MarkDirty();
        }
        public virtual T PopAt(int index)
        {
            T result = base[index];
            Remove(result);
            MarkDirty();
            return result;
        }

        public virtual new void Insert(int index, T item)
        {
            base.Insert(index, item);
            AttachItemEventHandlers(item);
            MarkDirty();
        }
        public virtual new void InsertRange(int index, IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                AttachItemEventHandlers(item);
            }
            base.InsertRange(index, collection);
            MarkDirty();
        }
        public virtual new void Reverse(int index, int count)
        {
            base.Reverse(index, count);
            MarkDirty();
        }

        public virtual new void Clear()
        {
            foreach (T item in this)
            {
                DetachItemEventHandlers(item);
            }
            base.Clear();
            MarkDirty();
        }

        public virtual new int IndexOf(T item) => base.IndexOf(item);

        public virtual new bool Contains(T item) => base.Contains(item);


        public virtual  new void CopyTo(T[] array, int arrayIndex) => base.CopyTo(array, arrayIndex);

        public virtual new IEnumerator<T> GetEnumerator() => base.GetEnumerator();






        public virtual new bool Sort() => Sort(0, base.Count, null);

        public virtual new bool Sort(IComparer<T>? comparer) => Sort(0, base.Count, comparer);

        public virtual new bool Sort(int index, int count, IComparer<T>? comparer)
        {
            if (IsDirty || LastComparer != comparer) 
            {
                base.Sort(index, count, comparer);
                LastComparer = comparer;
                IsDirty = false;
                return true;
            }
            return false;
        }
        public virtual new bool Sort(Comparison<T> comparison)
        {
            return Sort(Comparer<T>.Create(comparison));
        }

        public virtual bool RedoLastSort()
        {
            return Sort(LastComparer);
        }

    }
}
