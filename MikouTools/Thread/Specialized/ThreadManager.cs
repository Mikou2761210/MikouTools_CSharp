using MikouTools.Collections.Queue.Signaling;
using MikouTools.Thread.Utils;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading;

namespace MikouTools.Thread.Specialized
{
    /// <summary>
    /// Defines the possible states for the ThreadManager.
    /// </summary>
    public enum ThreadManagerState
    {
        Idle,    // The thread is idle and not processing any tasks.
        Running, // The thread is actively executing a task.
        Wait,    // The thread is waiting for tasks to be enqueued.
        Dispose  // The thread is in the process of being disposed.
    }

    /// <summary>
    /// Manages a dedicated thread that processes a queue of actions (tasks) sequentially.
    /// </summary>
    public class ThreadManager : IDisposable
    {
        // The underlying thread used to process queued tasks.
        internal System.Threading.Thread thread;

        // A specialized queue that signals when there are items (Process tasks) to process.
        readonly CountSignalingQueue<Process> ProcessQueue = new(count => count <= 0, count => count > 0);

        // Provides get/set access to the thread's name.
        public string? ThreadName
        {
            get { return thread.Name; }
            set { thread.Name = value; }
        }

        /// <summary>
        /// Initializes a new instance of ThreadManager.
        /// </summary>
        /// <param name="_ThreadName">Optional name for the thread.</param>
        /// <param name="IsBackground">Specifies whether the thread should run in the background.</param>
        /// <param name="ApartmentState">Optional apartment state for the thread.</param>
        public ThreadManager(string? _ThreadName = null, bool IsBackground = true, ApartmentState? ApartmentState = null)
        {
            // Create the underlying thread with a loop that continuously processes tasks.
            thread = new System.Threading.Thread(() =>
            {
                while (!dispose.Value)
                {
                    // Set the thread's state to 'Wait' before checking for new tasks.
                    _threadManagerState.Value = ThreadManagerState.Wait;
                    // Wait until there is at least one task in the queue.
                    ProcessQueue.CountCheckAndWait();

                    if (dispose.Value) break;

                    // Set the state to 'Running' just before executing a task.
                    _threadManagerState.Value = ThreadManagerState.Running;
                    // Dequeue and invoke the next task.
                    ProcessQueue.Dequeue().Invoke();
                }
            });
            ThreadName = _ThreadName;
            thread.IsBackground = IsBackground;
            if (ApartmentState != null)
                thread.SetApartmentState((ApartmentState)ApartmentState);
        }

        // Manages the current state of the ThreadManager (Idle, Running, Wait, or Dispose).
        readonly LockableProperty<ThreadManagerState> _threadManagerState = new(ThreadManagerState.Idle);
        public ThreadManagerState ThreadState { get { return _threadManagerState.Value; } }

        /// <summary>
        /// Synchronously invokes an action on the dedicated thread.
        /// Waits until the action has completed and returns any exception that was thrown.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <returns>An exception if one occurred during execution; otherwise, null.</returns>
        public Exception? Invoke(Action action)
        {
            ObjectDisposedException.ThrowIf(dispose.Value, this);

            // If the thread is idle, start it.
            if (_threadManagerState.SetAndReturnOld(ThreadManagerState.Wait) == ThreadManagerState.Idle)
                thread.Start();
            if (System.Threading.Thread.CurrentThread == thread)
            {
                Exception? exception = null;
                try
                {
                    action();
                }
                catch (Exception ex) { exception = ex; }
                return exception;
            }
            // Wrap the action in a Process.
            Process process = new(action);
            // Enqueue the Process for execution.
            ProcessQueue.Enqueue(process);
            // Wait for the Process to complete.
            process.ProcessCompletedWait();
            // Return any exception encountered during execution.
            return process.InvokeException;
        }

        /// <summary>
        /// Asynchronously invokes an action on the dedicated thread.
        /// The caller does not wait for the action to complete.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public void InvokeAsync(Action action)
        {
            ObjectDisposedException.ThrowIf(dispose.Value, this);

            // If the thread is idle, start it.
            if (_threadManagerState.SetAndReturnOld(ThreadManagerState.Wait) == ThreadManagerState.Idle)
                thread.Start();

            // Enqueue a new Process wrapping the action without waiting for its completion.
            ProcessQueue.Enqueue(new Process(action));
        }

        // Flag indicating whether the ThreadManager has been disposed.
        readonly LockableProperty<bool> dispose = new(false);

        /// <summary>
        /// Disposes the ThreadManager by stopping the thread and cleaning up resources.
        /// </summary>
        public void Dispose()
        {
            if (!dispose.SetAndReturnOld(true))
            {
                // Prevent further additions to the process queue.
                ProcessQueue.AddLock = true;
                // Disable the wait mechanism on the queue.
                ProcessQueue.DisableWait();
                // Wait for the processing thread to finish.
                thread.Join();
                // Set the state to Dispose.
                _threadManagerState.Value = ThreadManagerState.Dispose;
                // Dispose all remaining processes in the queue.
                while (ProcessQueue.Count > 0)
                {
                    ProcessQueue.Dequeue().Dispose();
                }
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Private class representing a task (Process) to be executed on the thread.
        /// </summary>
        private class Process(Action action) : IDisposable
        {
            // The action to be executed.
            readonly Action _action = action;

            // A manual reset event used to signal when the process has completed.
            readonly CountingManualResetEvent _manualResetEvent = new(false);

            // Ensures that the action is only invoked once.
            readonly LockableProperty<bool> _invokecheck = new(false);

            /// <summary>
            /// Exception captured during the action invocation, if any.
            /// </summary>
            public Exception? InvokeException { get; private set; }

            /// <summary>
            /// Executes the action if it has not already been executed.
            /// Signals completion via the manual reset event.
            /// </summary>
            /// <returns>True if the action executed successfully; otherwise, false.</returns>
            public bool Invoke()
            {
                ObjectDisposedException.ThrowIf(dispose.Value, this);
                bool result = false;
                // Ensure the action is only executed once.
                if (!_invokecheck.SetAndReturnOld(true))
                {
                    try
                    {
                        // Execute the provided action.
                        _action.Invoke();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        // Capture any exception thrown during execution.
                        InvokeException = ex;
                    }
                    finally
                    {
                        // Signal that the process has completed.
                        _manualResetEvent.Set();
                    }
                }
                return result;
            }

            /// <summary>
            /// Waits indefinitely for the process to complete.
            /// </summary>
            public void ProcessCompletedWait()
            {
                ProcessCompletedWait(-1);
            }

            /// <summary>
            /// Waits for the process to complete, with an optional timeout.
            /// </summary>
            /// <param name="millisecondsTimeout">The maximum time to wait (in milliseconds). Pass -1 for infinite wait.</param>
            public void ProcessCompletedWait(int millisecondsTimeout)
            {
                ObjectDisposedException.ThrowIf(dispose.Value, this);
                _manualResetEvent.WaitOne(millisecondsTimeout);
            }

            // Flag indicating whether this Process has been disposed.
            readonly LockableProperty<bool> dispose = new(false);

            /// <summary>
            /// Disposes the process, ensuring the action has completed or the wait event is set.
            /// </summary>
            public void Dispose()
            {
                if (!dispose.SetAndReturnOld(true))
                {
                    // If the action was already invoked, wait for it to complete.
                    if (_invokecheck.SetAndReturnOld(true))
                    {
                        ProcessCompletedWait();
                    }
                    else
                    {
                        // Otherwise, ensure that the wait event is signaled.
                        _manualResetEvent.Set();
                    }
                    _manualResetEvent.Dispose();
                }
            }
        }
    }
}

