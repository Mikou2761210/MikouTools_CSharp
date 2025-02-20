using MikouTools.CollectionTools.CustomCollections;
using MikouTools.UtilityTools.Threading;

namespace MikouTools.CollectionTools.SignalingCollections
{
    public class CountSignalingQueue : CustomQueue
    {
        private readonly object _lock = new object();
        private readonly ManualResetEvent _signal = new ManualResetEvent(false);
        private readonly LockableProperty<bool> _wait = new LockableProperty<bool>(true);
        public Func<int, bool> WaitCondition;


        /// <summary>
        /// (_WaitCondition == true)時にWaitをしない
        /// </summary>
        /// <param name="_WaitCondition"></param>
        public CountSignalingQueue(Func<int, bool>? _WaitCondition = null)
        {
            WaitCondition = _WaitCondition ?? (count => count > 0);
        }
        public override void Enqueue(object? item)
        {
            lock (_lock)
            {
                base.Enqueue(item);

                CountCheck(Count);
            }
        }

        public new object? Dequeue()
        {
            lock (_lock)
            {
                CountCheck(Count - 1);
                return base.Dequeue();
            }

        }

        private void CountCheck(int count)
        {
            if (WaitCondition(count) || !_wait.Value)
                _signal.Set();
            else
                _signal.Reset();
        }

        public virtual void CountCheckAndWait() => CountCheckAndWait(-1);
        public virtual bool CountCheckAndWait(int millisecondsTimeout)
        {
            if (_wait.Value)
                return _signal.WaitOne(millisecondsTimeout);
            return true;
        }

        public virtual void DisableWait()
        {
            lock (_lock)
            {
                _wait.Value = false;
                _signal.Set();
            }
        }

        public virtual void EnableWait()
        {
            lock (_lock)
            {
                _wait.Value = true;
                CountCheck(Count);
            }
        }
    }
    public class CountSignalingQueue<T> : CustomQueue<T>
    {
        private readonly object _lock = new object();
        private readonly ManualResetEvent _signal = new ManualResetEvent(false);
        private readonly LockableProperty<bool> _wait = new LockableProperty<bool>(true);
        public Func<int, bool> WaitCondition;


        /// <summary>
        /// (_WaitCondition == true)時にWaitをしない
        /// </summary>
        /// <param name="_WaitCondition"></param>
        public CountSignalingQueue(Func<int, bool>? _WaitCondition = null)
        {
            WaitCondition = _WaitCondition ?? (count => count > 0);
        }

        public new void Enqueue(T item)
        {
            lock (_lock)
            {
                base.Enqueue(item);

                CountCheck(Count);
            }
        }
        public new T Dequeue()
        {
            lock (_lock)
            {
                CountCheck(Count - 1);
                return base.Dequeue();
            }

        }
        private void CountCheck(int count)
        {
            if (WaitCondition(count) || !_wait.Value)
                _signal.Set();
            else
                _signal.Reset();
        }

        public virtual void CountCheckAndWait() => CountCheckAndWait(-1);
        public virtual bool CountCheckAndWait(int millisecondsTimeout)
        {
            if (_wait.Value)
                return _signal.WaitOne(millisecondsTimeout);
            return true;
        }

        public virtual void DisableWait()
        {
            lock (_lock)
            {
                _wait.Value = false;
                _signal.Set();
            }
        }

        public virtual void EnableWait()
        {
            lock (_lock)
            {
                _wait.Value = true;
                CountCheck(Count);
            }
        }
    }




    [Obsolete]
    public class SignalingQueue : CustomQueue
    {
        private readonly object _lock = new object();
        private readonly ManualResetEvent _signal = new ManualResetEvent(false);

        public override void Enqueue(object? obj)
        {
            lock (_lock)
            {
                base.Enqueue(obj);
                _signal.Set();
            }
        }

        public virtual void AddSignalWait()
        {
            AddSignalWait(-1);
        }
        public virtual bool AddSignalWait(int millisecondsTimeout)
        {
            lock (_lock)
                if (base.Count > 0)
                    _signal.Reset();

            return _signal.WaitOne(millisecondsTimeout);
        }
    }

    [Obsolete]
    public class SignalingQueue<T> : CustomQueue<T>
    {
        private readonly object _lock = new object();
        private readonly ManualResetEvent _signal = new ManualResetEvent(false);

        public new void Enqueue(T item)
        {
            lock (_lock)
            {
                base.Enqueue(item);
                _signal.Set();
            }

        }

        public virtual void AddSignalWait()
        {
            AddSignalWait(-1);
        }
        public virtual bool AddSignalWait(int millisecondsTimeout)
        {
            lock (_lock)
                if (base.Count > 0)
                    _signal.Reset();

            return _signal.WaitOne(millisecondsTimeout);
        }
    }


}
