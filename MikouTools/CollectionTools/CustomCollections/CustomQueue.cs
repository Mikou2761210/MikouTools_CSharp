using MikouTools.UtilityTools.Threading;
using System.Collections;

namespace MikouTools.CollectionTools.CustomCollections
{
    public class CustomQueue : Queue
    {
        LockableProperty<bool> _addAllow = new LockableProperty<bool>(true);
        public bool AddAllow { get => _addAllow.Value; set => _addAllow.Value = value; }
        public override void Enqueue(object? obj)
        {
            if (AddAllow)
                base.Enqueue(obj);
        }
    }
    public class CustomQueue<T> : Queue<T>
    {
        LockableProperty<bool> _addAllow = new LockableProperty<bool>(true);
        public bool AddAllow { get => _addAllow.Value; set => _addAllow.Value = value; }
        public new void Enqueue(T item)
        {
            if (AddAllow)
                base.Enqueue(item);
        }
    }
}
