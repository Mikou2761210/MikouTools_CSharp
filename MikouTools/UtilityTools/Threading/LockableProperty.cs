using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.UtilityTools.Threading
{
    public class LockableProperty<T> : ILockableProperty<T>
    {
        private T _value;
        private readonly object _lock = new object();

        public LockableProperty(T value)
        {
            lock (_lock)
            {
                _value = value;
            }
        }


        public T Value
        {
            get
            {
                lock (_lock) return _value;
            }
            set
            {
                lock (_lock) _value = value;

            }
        }

        public T AccessValueWhileLocked
        {
            get { return _value; }
            set { _value = value; }
        }

        public void Lock()
        {
            Monitor.Enter(_lock);
        }

        public void UnLock()
        {
            Monitor.Exit(_lock);
        }
        public void ExecuteWithLock(Action action)
        {
            lock (_lock)
            {
                action?.Invoke();
            }
        }
        public T SetAndReturnOld(T newvalue)
        {
            lock (_lock)
            {
                T oldvalue = _value;
                _value = newvalue;
                return oldvalue;
            }
        }
    }

    [Obsolete]
    public class AdvancedLockableProperty<T> : ILockableProperty<T>
    {
        private T _value;
        //private readonly object _lockObject = new object();

        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public AdvancedLockableProperty(T value)
        {
            _value = value;
        }


        public T Value
        {
            get
            {
                semaphore.Wait();
                if (_value == null) { semaphore.Release(); throw new InvalidOperationException(); }
                T result = _value;
                semaphore.Release();
                return result;
            }
            set
            {
                semaphore.Wait();
                try
                {
                    _value = value;
                }
                finally { semaphore.Release(); }

            }
        }

        public T AccessValueWhileLocked
        {
            get { return _value; }
            set { _value = value; }
        }


        public void ExecuteWithLock(Action action)
        {
            semaphore.Wait();
            try
            {
                action?.Invoke();
            }
            finally { semaphore.Release(); }
        }
        public T SetAndReturnOld(T newvalue)
        {
            semaphore.Wait();
            T oldvalue = _value;
            _value = newvalue;
            semaphore.Release();
            return oldvalue;
        }

        public void Lock()
        {
            semaphore.Wait();
        }

        public void UnLock()
        {
            semaphore.Release();
        }
        // 暗黙的な型変換
        /*public static implicit operator T(LockablePropertyBase<T> lockableProperty)
        {
            return lockableProperty.Value;
        }

        public static implicit operator LockablePropertyBase<T>(T value)
        {
            var lockableProperty = new LockablePropertyBase<T>();
            lockableProperty.Value = value;
            return lockableProperty;
        }*/
    }
}
