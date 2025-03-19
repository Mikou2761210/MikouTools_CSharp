using MikouTools.Collections.Interfaces;
using System.Collections;

namespace MikouTools.Collections.Queue.MultiLock
{
    public class MultiLockQueue : System.Collections.Queue, IMultiLock
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
            if (!RemoveLock)
                return base.Dequeue();
            return base.Peek();
        }

    }
    public class MultiLockQueue<T> : Queue<T>, IMultiLock
    {
        public virtual bool AddLock { get; set; }
        public virtual bool RemoveLock { get; set; }
        public new virtual bool Enqueue(T obj)
        {
            if (!AddLock)
            {
                base.Enqueue(obj);
                return true;
            }
            return false;
        }
        public new virtual T Dequeue()
        {
            if (!RemoveLock)
                return base.Dequeue();
            return Peek();
        }
        public new virtual bool TryDequeue(out T? result)
        {
            result = default;
            if (!RemoveLock)
                return base.TryDequeue(out result);
            return false;
        }
    }
}
