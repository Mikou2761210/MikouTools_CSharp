using MikouTools.Thread.Utils;
using System.Collections;

namespace MikouTools.Collections.MultiLock
{
    public class MultiLockQueue : Queue, IMultiLock
    {
        public virtual bool AddLock { get; set; }
        public virtual bool RemoveLock { get; set; }
        public override void Enqueue(object? obj)
        {
            if (!AddLock)
                base.Enqueue(obj);
        }
        public override object? Dequeue()
        {
            if (!AddLock)
                return base.Dequeue();
            return null;
        }
    }
    public class CustomQueue<T> : Queue<T>, IMultiLock
    {
        public virtual bool AddLock { get; set; }
        public virtual bool RemoveLock { get; set; }
        public new virtual void Enqueue(T obj)
        {
            if (!AddLock)
                base.Enqueue(obj);
        }
        public new virtual T? Dequeue()
        {
            if (!AddLock)
                return base.Dequeue();
            return default(T);
        }
    }
}
