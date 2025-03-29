using MikouTools.Collections.QueueEx.MultiLock;
using MikouTools.Thread.Utils;

namespace MikouTools.Collections.QueueEx.Signaling
{
    /// <summary>
    /// Represents the waiting state of the signaling queue.
    /// </summary>
    public enum SignalingQueueState
    {
        EnableWait,

        DisableWait,

        AlwaysWait
    }

    /// <summary>
    /// A thread-safe queue that signals waiting based on the count and provided conditions.
    /// When <c>StopWaitCondition</c> is true and <c>StartWaitCondition</c> is false, waiting is stopped (signal set).
    /// </summary>
    /// <typeparam name="T">The type of elements in the queue.</typeparam>
    public class CountSignalingQueue<T>(Func<int, bool> startWaitCondition, Func<int, bool> stopWaitCondition) : MultiLockQueue<T>
    {
        // An object used for synchronizing access to the queue.
        private readonly object _lock = new();

        // A ManualResetEventSlim used to signal threads waiting on the queue.
        private readonly ManualResetEventSlim _signal = new(false);

        // The current state of the queue's signaling behavior.
        private SignalingQueueState _state = SignalingQueueState.EnableWait;

        // Condition to determine when to start waiting.
        public Func<int, bool> StartWaitCondition = startWaitCondition;

        // Condition to determine when to stop waiting.
        public Func<int, bool> StopWaitCondition = stopWaitCondition;

        /// <summary>
        /// Gets or sets a value indicating whether adding items is locked.
        /// Thread-safe access via lock.
        /// </summary>
        public override bool AddLock
        {
            get { lock (_lock) return base.AddLock; }
            set { lock (_lock) base.AddLock = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether removing items is locked.
        /// Thread-safe access via lock.
        /// </summary>
        public override bool RemoveLock
        {
            get { lock (_lock) return base.RemoveLock; }
            set { lock (_lock) base.RemoveLock = value; }
        }

        /// <summary>
        /// Gets the number of items in the queue.
        /// Thread-safe access via lock.
        /// </summary>
        public new int Count
        {
            get { lock (_lock) return base.Count; }
        }

        /// <summary>
        /// Gets or sets the current signaling state of the queue.
        /// When setting the state, the signal is updated accordingly.
        /// </summary>
        public SignalingQueueState State
        {
            get { lock (_lock) return _state; }
            set
            {
                lock (_lock)
                {
                    _state = value;
                    switch (value)
                    {
                        case SignalingQueueState.EnableWait:
                            // Update signal based on current count when waiting is enabled.
                            CountCheck(base.Count);
                            break;
                        case SignalingQueueState.DisableWait:
                            // Always set the signal to disable waiting.
                            _signal.Set();
                            break;
                        case SignalingQueueState.AlwaysWait:
                            // Update signal based on current count when always waiting.
                            CountCheck(base.Count);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Enqueues an item into the queue and updates the signal.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public new void Enqueue(T item)
        {
            lock (_lock)
            {
                base.Enqueue(item);
                CountCheck(base.Count);
            }
        }

        /// <summary>
        /// Dequeues an item from the queue and updates the signal.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        public new T Dequeue()
        {
            lock (_lock)
            {
                T result = base.Dequeue();
                CountCheck(base.Count);
                return result;
            }
        }

        /// <summary>
        /// Attempts to dequeue an item from the queue.
        /// Updates the signal if successful.
        /// </summary>
        /// <param name="result">When this method returns, contains the dequeued item if successful.</param>
        /// <returns><c>true</c> if an element was dequeued; otherwise, <c>false</c>.</returns>
        public new bool TryDequeue(out T? result)
        {
            lock (_lock)
            {
                bool success = base.TryDequeue(out result);
                if (success)
                {
                    CountCheck(base.Count);
                }
                return success;
            }
        }

        /// <summary>
        /// Returns the item at the beginning of the queue without removing it.
        /// </summary>
        /// <returns>The item at the beginning of the queue.</returns>
        public new T Peek()
        {
            lock (_lock)
            {
                return base.Peek();
            }
        }

        /// <summary>
        /// Attempts to return the item at the beginning of the queue without removing it.
        /// </summary>
        /// <param name="result">When this method returns, contains the item at the beginning if successful.</param>
        /// <returns><c>true</c> if an item was found; otherwise, <c>false</c>.</returns>
        public new bool TryPeek(out T? result)
        {
            lock (_lock)
            {
                return base.TryPeek(out result);
            }
        }

        /// <summary>
        /// Clears all items from the queue and updates the signal.
        /// </summary>
        public new void Clear()
        {
            lock (_lock)
            {
                base.Clear();
                CountCheck(base.Count);
            }
        }

        /// <summary>
        /// Checks the count and updates the signal based on the current state and conditions.
        /// </summary>
        /// <param name="count">The current count to check.</param>
        private void CountCheck(int count)
        {
            // If waiting is disabled OR (stop condition is met and start condition is not met)
            // AND the state is not AlwaysWait, then set the signal; otherwise, reset it.
            if ((_state == SignalingQueueState.DisableWait || StopWaitCondition(count) && !StartWaitCondition(count))
                 && _state != SignalingQueueState.AlwaysWait)
            {
                _signal.Set();
            }
            else
            {
                _signal.Reset();
            }
        }

        /// <summary>
        /// Checks the count and waits indefinitely until the signal is set.
        /// </summary>
        public virtual void CountCheckAndWait() => CountCheckAndWait(-1);

        /// <summary>
        /// Checks the count and waits for the specified timeout until the signal is set.
        /// </summary>
        /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
        /// <returns><c>true</c> if the signal was set; otherwise, <c>false</c>.</returns>
        public virtual bool CountCheckAndWait(int millisecondsTimeout)
        {
            return _signal.Wait(millisecondsTimeout);
        }

        /// <summary>
        /// Re-checks the count and updates the signal.
        /// Useful when external changes might affect the waiting conditions.
        /// </summary>
        public void RecheckCount()
        {
            lock (_lock)
            {
                CountCheck(base.Count);
            }
        }

        /// <summary>
        /// Disables waiting by setting the state to DisableWait.
        /// </summary>
        public virtual void DisableWait()
        {
            State = SignalingQueueState.DisableWait;
        }

        /// <summary>
        /// Enables waiting by setting the state to EnableWait.
        /// </summary>
        public virtual void EnableWait()
        {
            State = SignalingQueueState.EnableWait;
        }

        /// <summary>
        /// Forces waiting by setting the state to AlwaysWait.
        /// </summary>
        public virtual void AlwaysWait()
        {
            State = SignalingQueueState.AlwaysWait;
        }
    }

}
