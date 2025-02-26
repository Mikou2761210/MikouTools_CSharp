namespace MikouTools.Thread.Utils
{
    /// <summary>
    /// A thread-safe wrapper for a property of type T.
    /// Provides synchronized access and modification of the property's value.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    public class LockableProperty<T> : ILockableProperty<T>
    {
        // The underlying value of the property.
        private T _value;

        // A private lock object used to synchronize access to the property.
        private readonly object _lock = new();

        /// <summary>
        /// Initializes a new instance of the LockableProperty class with the specified initial value.
        /// </summary>
        /// <param name="value">The initial value of the property.</param>
        public LockableProperty(T value)
        {
            // Locking during initialization ensures thread-safety if multiple threads are initializing concurrently.
            lock (_lock)
            {
                _value = value;
            }
        }

        /// <summary>
        /// Gets or sets the value in a thread-safe manner.
        /// </summary>
        public T Value
        {
            get
            {
                // Lock to safely read the value.
                lock (_lock)
                {
                    return _value;
                }
            }
            set
            {
                // Lock to safely write the new value.
                lock (_lock)
                {
                    _value = value;
                }
            }
        }



        /// <summary>
        /// A disposable handle that automatically releases the lock on the LockableProperty when disposed.
        /// </summary>
        public sealed class LockHandle : IDisposable
        {
            // Indicates whether the lock has already been released.
            bool _disposed = false;

            // Reference to the LockableProperty instance whose lock is held.
            readonly LockableProperty<T> _lockableProperty;

            /// <summary>
            /// Gets or sets the value of the underlying LockableProperty while the lock is held.
            /// This property ensures that the value is accessed only when the lock is not disposed.
            /// </summary>
            public T Value
            {
                get
                {
                    // Ensure that the lock handle has not been disposed.
                    ObjectDisposedException.ThrowIf(_disposed, this);
                    // Return the value from the underlying property while the lock is held.
                    return _lockableProperty.AccessValueWhileLocked;
                }
                set
                {
                    // Ensure that the lock handle has not been disposed.
                    ObjectDisposedException.ThrowIf(_disposed, this);
                    // Set the value of the underlying property while the lock is held.
                    _lockableProperty.AccessValueWhileLocked = value;
                }
            }

            /// <summary>
            /// Initializes a new instance of LockHandle and acquires the lock on the provided LockableProperty.
            /// </summary>
            /// <param name="lockableProperty">The LockableProperty instance to lock.</param>
            internal LockHandle(LockableProperty<T> lockableProperty)
            {
                _lockableProperty = lockableProperty;
                // Acquire the lock immediately upon creation.
                _lockableProperty.EnterLock();
            }

            /// <summary>
            /// Releases the lock on the associated LockableProperty if it has not been released already.
            /// </summary>
            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    // Release the lock when disposing.
                    _lockableProperty.ExitLock();
                }
            }
        }

        /// <summary>
        /// Acquires the lock on this LockableProperty and returns a disposable handle that maintains the lock.
        /// Use the returned handle to safely access or modify the property's value via its Value property.
        /// The lock will be automatically released when the handle is disposed (for example, when used in a using statement).
        /// </summary>
        /// <returns>A LockHandle instance that holds the lock on the property.</returns>
        public LockHandle LockAndGetList()
        {
            return new LockHandle(this);
        }


        /// <summary>
        /// Gets or sets the value without acquiring the lock.
        /// This property should only be used when you have already acquired the lock externally.
        /// </summary>
        public T AccessValueWhileLocked
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Acquires the lock on the property.
        /// Use this when you need to perform multiple operations atomically.
        /// </summary>
        public void EnterLock()
        {
            Monitor.Enter(_lock);
        }

        /// <summary>
        /// Releases the lock on the property.
        /// Ensure that each call to EnterLock() is balanced with a call to ExitLock().
        /// </summary>
        public void ExitLock()
        {
            Monitor.Exit(_lock);
        }

        /// <summary>
        /// Executes the specified action while holding the lock.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void ExecuteWithLock(Action action)
        {
            lock (_lock)
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// Sets the property's value to a new value and returns the old value.
        /// This operation is performed atomically.
        /// </summary>
        /// <param name="newvalue">The new value to set.</param>
        /// <returns>The old value before the update.</returns>
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

}
