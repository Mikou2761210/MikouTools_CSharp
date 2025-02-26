using System;

namespace MikouTools.Collections.DirtySort
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
                base[index] = value;
                MarkDirty();
            }
        }

        public virtual new int Count => base.Count;

        public virtual bool IsReadOnly => false;

        public virtual new void Add(T item)
        {
            base.Add(item);
            MarkDirty();
        }

        public virtual new void AddRange(IEnumerable<T> items)
        {
            base.AddRange(items);
            MarkDirty();
        }


        public virtual new bool Remove(T item)
        {
            bool removed = base.Remove(item);
            if (removed) MarkDirty();
            return removed;
        }
        public virtual new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            MarkDirty();
        }

        public virtual new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            MarkDirty();
        }

        public virtual new void Insert(int index, T item)
        {
            base.Insert(index, item);
            MarkDirty();
        }
        public virtual new void InsertRange(int index, IEnumerable<T> collection)
        {
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
