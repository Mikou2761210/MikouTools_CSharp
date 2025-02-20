namespace MikouTools.CollectionTools.DirtySortCollections
{

    public class DirtySortList<T> : List<T>, IList<T>
    {

        public bool IsDirty = false;

        public IComparer<T>? LastComparer { get; private set; } = null;
        public Comparison<T>? LastComparison { get; private set; } = null;


        private void MarkDirty()
        {
            IsDirty = true;
        }


        public new T this[int index]
        {
            get => base[index];
            set
            {
                base[index] = value;
                MarkDirty();
            }
        }

        public new int Count => base.Count;

        public bool IsReadOnly => false;

        public new void Add(T item)
        {
            base.Add(item);
            MarkDirty();
        }

        public new void AddRange(IEnumerable<T> items)
        {
            base.AddRange(items);
            MarkDirty();
        }


        public new bool Remove(T item)
        {
            bool removed = base.Remove(item);
            if (removed) MarkDirty();
            return removed;
        }
        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            MarkDirty();
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            MarkDirty();
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);
            MarkDirty();
        }
        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            base.InsertRange(index, collection);
            MarkDirty();
        }
        public new void Reverse(int index, int count)
        {
            base.Reverse(index, count);
            MarkDirty();
        }

        public new void Clear()
        {
            base.Clear();
            MarkDirty();
        }

        public new int IndexOf(T item) => base.IndexOf(item);

        public new bool Contains(T item) => base.Contains(item);


        public new void CopyTo(T[] array, int arrayIndex) => base.CopyTo(array, arrayIndex);

        public new IEnumerator<T> GetEnumerator() => base.GetEnumerator();






        public new bool Sort() => Sort(0, base.Count, null);

        public new bool Sort(IComparer<T>? comparer) => Sort(0, base.Count, comparer);

        public new bool Sort(int index, int count, IComparer<T>? comparer)
        {
            if (IsDirty || (LastComparison != null && comparer == null) || comparer?.Equals(LastComparer) == false) 
            {
                base.Sort(index, count, comparer);
                LastComparer = comparer;
                LastComparison = null;
                IsDirty = false;
                return true;
            }
            return false;
        }
        public new bool Sort(Comparison<T> comparison)
        {
            if (IsDirty || comparison?.Equals(LastComparer) == false)
            {
                base.Sort(comparison);
                LastComparer = null;
                LastComparison = comparison;
                IsDirty = false;
                return true;
            }
            return false;
        }

        public bool RedoLastSort()
        {
            if (LastComparison == null)
                return Sort(0, base.Count, LastComparer);
            else
                return Sort(LastComparison);
        }

    }
}
