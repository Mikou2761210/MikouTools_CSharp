using MikouTools.CollectionTools.CustomCollections;
using MikouTools.UtilityTools.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.CollectionTools.SignalingCollections
{


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

    public class CountSignalingQueue : CustomQueue
    {
        private readonly object _lock = new object();
        private readonly ManualResetEvent _signal = new ManualResetEvent(false);

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
            if (count > 0 || Wait.Value)
                _signal.Set();
            else
                _signal.Reset();
        }




        LockableProperty<bool> Wait = new LockableProperty<bool>(true);

        public virtual void CountCheckAndWait() => CountCheckAndWait(-1);
        public virtual bool CountCheckAndWait(int millisecondsTimeout)
        {
            if (Wait.Value)
                return _signal.WaitOne(millisecondsTimeout);
            return true;
        }

        public virtual void DisableWait()
        {
            lock (_lock)
            {
                Wait.Value = false;
                _signal.Set();
            }
        }

        public virtual void EnableWait()
        {
            lock (_lock)
            {
                Wait.Value = true;
                CountCheck(Count);
            }
        }
    }
    public class CountSignalingQueue<T> : CustomQueue<T>
    {
        private readonly object _lock = new object();
        private readonly ManualResetEvent _signal = new ManualResetEvent(false);

        public Func<int, bool> WaitCondition;

        /// <summary>
        /// (_WaitCondition == true)時にWaitをしない
        /// </summary>
        /// <param name="_WaitCondition"></param>
        public CountSignalingQueue(Func<int, bool>? _WaitCondition  = null)
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
            if (WaitCondition(count) || !Wait.Value)
                _signal.Set();
            else
                _signal.Reset();
        }




        LockableProperty<bool> Wait = new LockableProperty<bool>(true);

        public virtual void CountCheckAndWait() => CountCheckAndWait(-1);
        public virtual bool CountCheckAndWait(int millisecondsTimeout)
        {
            if (Wait.Value)
                return _signal.WaitOne(millisecondsTimeout);
            return true;
        }

        public virtual void DisableWait()
        {
            lock (_lock)
            {
                Wait.Value = false;
                _signal.Set();
            }
        }

        public virtual void EnableWait()
        {
            lock (_lock)
            {
                Wait.Value = true;
                CountCheck(Count);
            }
        }
    }
}
