using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.DirtySort
{
    public class ConcurrentDirtySortList<T> : DirtySortList<T>
    {
        private readonly object _lockObj = new();

        public override bool IsDirty { get { lock (_lockObj) return base.IsDirty; } set { lock (_lockObj) base.IsDirty = value; } } 
        public override IComparer<T>? LastComparer { get { lock (_lockObj) return base.LastComparer; } set { lock (_lockObj) base.LastComparer = value; } }

        private void MarkDirty()
        {
            IsDirty = true;
        }

        public override T this[int index]
        {
            get
            {
                lock (_lockObj)
                {
                    return base[index];
                }
            }
            set
            {
                lock (_lockObj)
                {
                    base[index] = value;
                    MarkDirty();
                }
            }
        }

        public override int Count
        {
            get
            {
                lock (_lockObj)
                {
                    return base.Count;
                }
            }
        }

        public override bool IsReadOnly => false;

        public override void Add(T item)
        {
            lock (_lockObj)
            {
                base.Add(item);
                MarkDirty();
            }
        }

        public override void AddRange(IEnumerable<T> items)
        {
            lock (_lockObj)
            {
                base.AddRange(items);
                MarkDirty();
            }
        }

        public override bool Remove(T item)
        {
            lock (_lockObj)
            {
                bool removed = base.Remove(item);
                if (removed)
                {
                    MarkDirty();
                }
                return removed;
            }
        }

        public override void RemoveRange(int index, int count)
        {
            lock (_lockObj)
            {
                base.RemoveRange(index, count);
                MarkDirty();
            }
        }

        public override void RemoveAt(int index)
        {
            lock (_lockObj)
            {
                base.RemoveAt(index);
                MarkDirty();
            }
        }

        public override void Insert(int index, T item)
        {
            lock (_lockObj)
            {
                base.Insert(index, item);
                MarkDirty();
            }
        }

        public override void InsertRange(int index, IEnumerable<T> collection)
        {
            lock (_lockObj)
            {
                base.InsertRange(index, collection);
                MarkDirty();
            }
        }

        public override void Reverse(int index, int count)
        {
            lock (_lockObj)
            {
                base.Reverse(index, count);
                MarkDirty();
            }
        }

        public override void Clear()
        {
            lock (_lockObj)
            {
                base.Clear();
                MarkDirty();
            }
        }

        public override int IndexOf(T item)
        {
            lock (_lockObj)
            {
                return base.IndexOf(item);
            }
        }

        public override bool Contains(T item)
        {
            lock (_lockObj)
            {
                return base.Contains(item);
            }
        }

        public override void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lockObj)
            {
                base.CopyTo(array, arrayIndex);
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            // 列挙中に他のスレッドからの変更による問題を避けるため、スナップショットを返します
            lock (_lockObj)
            {
                return base.GetEnumerator();
            }
        }


        // List<T> のその他のメソッドも同様にロックを行う必要がある場合は、new キーワードでラップしてください
        // 例: BinarySearch, Find, ToArray, 等

        public override bool Sort() => Sort(0, Count, null);

        public override bool Sort(IComparer<T>? comparer) => Sort(0, Count, comparer);

        public override bool Sort(int index, int count, IComparer<T>? comparer)
        {
            lock (_lockObj)
            {
                return base.Sort(index, count, comparer);
            }
        }

        public override bool Sort(Comparison<T> comparison)
        {
            return Sort(Comparer<T>.Create(comparison));
        }

        public override bool RedoLastSort()
        {
            return Sort(LastComparer);
        }
    }

}
