using MikouTools.Thread.Specialized;
using MikouTools.Thread.ThreadSafe.Collections;
using MikouTools.Thread.Utils;
using System.Diagnostics;
using System.Threading;

namespace MikouTools.ThreadTools
{
    namespace MikouTools.ThreadTools
    {
        /// <summary>
        /// Manages thread resources and executes cleanup actions when specified conditions are met.
        /// </summary>
        public partial class ThreadResourceManager : IDisposable
        {
            // Underlying thread that periodically checks and executes cleanup tasks.
            private readonly System.Threading.Thread _thread;

            // A thread-safe list that holds cleanup tasks for threads.
            private readonly ThreadSafeList<List<ThreadResourceCleanup>, ThreadResourceCleanup> _threadCleanupTasks = new([]);

            // Polling interval (in milliseconds) for checking cleanup tasks.
            private int _pollingInterval = 1000;

            // Indicates whether this manager has been disposed.
            private readonly LockableProperty<bool> _dispose = new(false);

            // Indicates whether the internal thread has been started.
            private readonly LockableProperty<bool> _threadStart = new(false);

            /// <summary>
            /// Gets or sets the polling interval in milliseconds.
            /// The interval must be at least 10 milliseconds.
            /// </summary>
            public int PollingInterval
            {
                get
                {
                    // Check if the instance has been disposed.
                    ObjectDisposedException.ThrowIf(_dispose.Value, this);
                    return _pollingInterval;
                }
                set
                {
                    ObjectDisposedException.ThrowIf(_dispose.Value, this);
                    if (value < 10)
                        throw new IndexOutOfRangeException("value >= 10");
                    _pollingInterval = value;
                }
            }

            // Optional error callback action.
            public Action<Exception>? Error;

            // Helper method to log errors and invoke the error callback.
            private void ErrorHelper(Exception ex)
            {
                Debug.WriteLine(ex);
                Error?.Invoke(ex);
            }

            /// <summary>
            /// Initializes a new instance of ThreadResourceManager.
            /// </summary>
            /// <param name="ThreadName">Optional name for the internal monitoring thread.</param>
            public ThreadResourceManager(string? ThreadName = null)
            {
                _threadCleanupTasks.EnterLock();
                _thread = new System.Threading.Thread(() =>
                {
                    // Continuously loop as long as there are cleanup tasks and the manager is not disposed.
                    while (_threadCleanupTasks.Count > 0 && !_dispose.Value)
                    {
                        // Lock the cleanup task list for safe enumeration.
                        using (var LockHandle = _threadCleanupTasks.LockAndGetList())
                        {
                            for (int i = 0; i < LockHandle.List.Count; i++)
                            {
                                ThreadResourceCleanup cleanup = LockHandle.List[i];
                                try
                                {
                                    // If no custom dispose condition is provided,
                                    // check if the target thread is not alive and has stopped.
                                    if (cleanup.DisposeCondition == null)
                                    {
                                        if ((!cleanup.TargetThread.IsAlive && cleanup.TargetThread.ThreadState == System.Threading.ThreadState.Stopped))
                                        {
                                            // Execute the cleanup action.
                                            cleanup.DisposeAction();
                                            // Remove the cleanup task.
                                            _threadCleanupTasks.RemoveAt(i);
                                            i--;
                                        }
                                    }
                                    // If a custom dispose condition is provided, use it.
                                    else if (cleanup.DisposeCondition != null && cleanup.DisposeCondition(cleanup.TargetThread))
                                    {
                                        cleanup.DisposeAction();
                                        _threadCleanupTasks.RemoveAt(i);
                                        i--;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log the exception and remove the problematic task.
                                    Debug.WriteLine(ex);
                                    _threadCleanupTasks.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                        if (_dispose.Value) break;
                        // Sleep for the specified polling interval before the next check.
                        System.Threading.Thread.Sleep(_pollingInterval);
                    }
                })
                {
                    IsBackground = false,
                    Name = ThreadName
                };
                _threadCleanupTasks.ExitLock();
            }

            /// <summary>
            /// Adds a cleanup task associated with a target thread.
            /// The cleanup action is executed when the target thread meets the dispose condition.
            /// </summary>
            /// <param name="TargetThread">The thread to monitor.</param>
            /// <param name="DisposeAction">The action to execute for cleanup.</param>
            /// <param name="DisposeCondition">
            /// Optional custom condition. If null, cleanup occurs when the thread is not alive and stopped.
            /// </param>
            public void CleanupTaskAdd(System.Threading.Thread TargetThread, Action DisposeAction, Func<System.Threading.Thread, bool>? DisposeCondition = null)
            {
                ObjectDisposedException.ThrowIf(_dispose.Value, this);

                _threadCleanupTasks.Add(new ThreadResourceCleanup(TargetThread, DisposeAction, DisposeCondition));

                // Start the internal thread if it hasn't already been started.
                if (!_threadStart.SetAndReturnOld(true))
                {
                    _thread.Start();
                }
            }

            /// <summary>
            /// Adds a cleanup task using a ThreadManager's thread.
            /// </summary>
            /// <param name="TargetThreadManager">The ThreadManager whose thread will be monitored.</param>
            /// <param name="DisposeAction">The action to execute for cleanup.</param>
            /// <param name="DisposeCondition">Optional custom condition for cleanup.</param>
            public void CleanupTaskAdd(ThreadManager TargetThreadManager, Action DisposeAction, Func<System.Threading.Thread, bool>? DisposeCondition = null)
            {
                CleanupTaskAdd(TargetThreadManager.thread, DisposeAction, DisposeCondition);
            }

            /// <summary>
            /// Removes the first cleanup task associated with the specified thread.
            /// </summary>
            /// <param name="TargetThread">The thread whose cleanup task should be removed.</param>
            /// <returns>True if a task was removed; otherwise, false.</returns>
            public bool Remove(System.Threading.Thread TargetThread)
            {
                ObjectDisposedException.ThrowIf(_dispose.Value, this);

                using var LockHandle = _threadCleanupTasks.LockAndGetList();
                for (int i = 0; i < LockHandle.List.Count; i++)
                {
                    if (LockHandle.List[i].TargetThread == TargetThread)
                    {
                        LockHandle.List.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Removes all cleanup tasks associated with the specified thread.
            /// </summary>
            /// <param name="TargetThread">The thread whose cleanup tasks should be removed.</param>
            /// <returns>True if any tasks were removed; otherwise, false.</returns>
            public bool RemoveAll(System.Threading.Thread TargetThread)
            {
                ObjectDisposedException.ThrowIf(_dispose.Value, this);

                bool removeflag = false;
                using var LockHandle = _threadCleanupTasks.LockAndGetList();
                for (int i = 0; i < LockHandle.List.Count; i++)
                {
                    if (LockHandle.List[i].TargetThread == TargetThread)
                    {
                        removeflag = true;
                        LockHandle.List.RemoveAt(i);
                        i--;
                    }
                }
                return removeflag;
            }

            /// <summary>
            /// Executes the cleanup action for the first task associated with the specified thread and removes it.
            /// </summary>
            /// <param name="TargetThread">The thread whose cleanup task should be executed and removed.</param>
            /// <returns>True if a task was found and removed; otherwise, false.</returns>
            public bool DisposeRemove(System.Threading.Thread TargetThread)
            {
                ObjectDisposedException.ThrowIf(_dispose.Value, this);

                using var LockHandle = _threadCleanupTasks.LockAndGetList();
                for (int i = 0; i < LockHandle.List.Count; i++)
                {
                    if (LockHandle.List[i].TargetThread == TargetThread)
                    {
                        try
                        {
                            LockHandle.List[i].DisposeAction();
                        }
                        catch (Exception ex) { ErrorHelper(ex); }
                        LockHandle.List.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Executes the cleanup action for all tasks associated with the specified thread and removes them.
            /// </summary>
            /// <param name="TargetThread">The thread whose cleanup tasks should be executed and removed.</param>
            /// <returns>True if any tasks were found and removed; otherwise, false.</returns>
            public bool DisposeRemoveAll(System.Threading.Thread TargetThread)
            {
                ObjectDisposedException.ThrowIf(_dispose.Value, this);

                bool removeflag = false;
                using var LockHandle = _threadCleanupTasks.LockAndGetList();
                for (int i = 0; i < LockHandle.List.Count; i++)
                {
                    if (LockHandle.List[i].TargetThread == TargetThread)
                    {
                        removeflag = true;
                        try
                        {
                            LockHandle.List[i].DisposeAction();
                        }
                        catch (Exception ex) { ErrorHelper(ex); }
                        LockHandle.List.RemoveAt(i);
                        i--;
                    }
                }
                return removeflag;
            }

            /// <summary>
            /// Clears all cleanup tasks without executing their cleanup actions.
            /// </summary>
            public void Clear()
            {
                ObjectDisposedException.ThrowIf(_dispose.Value, this);

                using var LockHandle = _threadCleanupTasks.LockAndGetList();
                for (int i = 0; i < LockHandle.List.Count; i++)
                {
                    LockHandle.List.RemoveAt(i);
                    i--;
                }
            }

            /// <summary>
            /// Executes the cleanup action for all tasks and then clears the task list.
            /// </summary>
            public void DisposeClear()
            {
                ObjectDisposedException.ThrowIf(_dispose.Value, this);

                using var LockHandle = _threadCleanupTasks.LockAndGetList();
                for (int i = 0; i < LockHandle.List.Count; i++)
                {
                    try
                    {
                        LockHandle.List[i].DisposeAction();
                    }
                    catch (Exception ex) { ErrorHelper(ex); }
                    LockHandle.List.RemoveAt(i);
                    i--;
                }
            }

            /// <summary>
            /// Disposes the ThreadResourceManager by stopping the monitoring thread and suppressing finalization.
            /// </summary>
            public void Dispose()
            {
                if (!_dispose.SetAndReturnOld(true))
                {
                    // Wait for the internal thread to finish execution.
                    _thread.Join();

                    GC.SuppressFinalize(this);
                }
            }
        }

        public partial class ThreadResourceManager
        {
            /// <summary>
            /// Represents a cleanup task for a specific thread resource.
            /// </summary>
            internal class ThreadResourceCleanup
            {
                /// <summary>
                /// The thread to be monitored.
                /// </summary>
                internal System.Threading.Thread TargetThread { get; private set; }

                /// <summary>
                /// The cleanup action to be executed when the dispose condition is met.
                /// </summary>
                internal Action DisposeAction { get; private set; }

                /// <summary>
                /// Optional condition to determine when the cleanup action should be executed.
                /// </summary>
                internal Func<System.Threading.Thread, bool>? DisposeCondition;

                /// <summary>
                /// Initializes a new instance of the ThreadResourceCleanup class.
                /// </summary>
                /// <param name="_TargetThread">The thread to monitor.</param>
                /// <param name="_DisposeAction">The action to execute for cleanup.</param>
                /// <param name="_DisposeCondition">
                /// Optional condition; if null, the default condition is that the thread is not alive and is stopped.
                /// </param>
                internal ThreadResourceCleanup(System.Threading.Thread _TargetThread, Action _DisposeAction, Func<System.Threading.Thread, bool>? _DisposeCondition = null)
                {
                    TargetThread = _TargetThread;
                    DisposeAction = _DisposeAction;
                    DisposeCondition = _DisposeCondition;
                }
            }
        }
    }


}
