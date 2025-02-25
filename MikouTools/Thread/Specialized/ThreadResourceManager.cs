using MikouTools.Thread.Specialized;
using MikouTools.Thread.ThreadSafe.Collections;
using MikouTools.Thread.Utils;
using System.Diagnostics;
using System.Threading;

namespace MikouTools.ThreadTools
{
    public partial class ThreadResourceManager : IDisposable
    {
        private readonly System.Threading.Thread _thread;
        private readonly ThreadSafeList<List<ThreadResourceCleanup>, ThreadResourceCleanup> _threadCleanupTasks = new([]);
        private int _pollingInterval = 1000;
        private readonly LockableProperty<bool> _dispose = new(false); 
        private readonly LockableProperty<bool> _threadStart = new(false);


        public int PollingInterval
        {
            get 
            {
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
        public Action<Exception>? Error;
        private void ErrorHelper(Exception ex)
        {
            Debug.WriteLine(ex);
            Error?.Invoke(ex);
        }



        public ThreadResourceManager(string? ThreadName = null)
        {
            _threadCleanupTasks.EnterLock();
            _thread = new System.Threading.Thread(() =>
            {
                while (_threadCleanupTasks.Count > 0 && !_dispose.Value)
                {
                    using (var LockHandle = _threadCleanupTasks.LockAndGetList())
                    {
                        for (int i = 0; i < LockHandle.List.Count; i++)
                        {

                            ThreadResourceCleanup cleanup = LockHandle.List[i];
                            try
                            {
                                if (cleanup.DisposeCondition == null)
                                {
                                    if ((!cleanup.TargetThread.IsAlive && cleanup.TargetThread.ThreadState == System.Threading.ThreadState.Stopped))
                                    {
                                        cleanup.DisposeAction();
                                        _threadCleanupTasks.RemoveAt(i);
                                        i--;
                                    }
                                }
                                else if (cleanup.DisposeCondition != null && cleanup.DisposeCondition(cleanup.TargetThread))
                                {
                                    cleanup.DisposeAction();
                                    _threadCleanupTasks.RemoveAt(i);
                                    i--;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                                _threadCleanupTasks.RemoveAt(i);
                                i--;
                            }

                        }
                    }
                    if (_dispose.Value) break;
                    System.Threading.Thread.Sleep(_pollingInterval);
                }
            })
            {
                IsBackground = false,
                Name = ThreadName
            };
            _threadCleanupTasks.ExitLock();
        }



        //Add
        public void CleanupTaskAdd(System.Threading.Thread TargetThread,Action DisposeAction, Func<System.Threading.Thread, bool>? DisposeCondition = null)
        {
            ObjectDisposedException.ThrowIf(_dispose.Value, this);

            _threadCleanupTasks.Add(new ThreadResourceCleanup(TargetThread, DisposeAction, DisposeCondition));

            if (!_threadStart.SetAndReturnOld(true))
            {
                _thread.Start();
            }
        }

        public void CleanupTaskAdd(ThreadManager TargetThreadManager, Action DisposeAction, Func<System.Threading.Thread, bool>? DisposeCondition = null)
        {
            CleanupTaskAdd(TargetThreadManager.thread, DisposeAction, DisposeCondition);
        }

        //Remove
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

        //Clear


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

        public void Dispose()
        {
            if(!_dispose.SetAndReturnOld(true))
            {
                _thread.Join();
                
                GC.SuppressFinalize(this);
            }
        }

    }
    public partial class ThreadResourceManager
    {
        internal class ThreadResourceCleanup
        {
            internal System.Threading.Thread TargetThread { get; private set; }
            internal Action DisposeAction { get; private set; }


            internal Func<System.Threading.Thread, bool>? DisposeCondition;


            internal ThreadResourceCleanup(System.Threading.Thread _TargetThread, Action _DisposeAction, Func<System.Threading.Thread, bool>? _DisposeCondition = null)
            {
                TargetThread = _TargetThread;
                DisposeAction = _DisposeAction;
                DisposeCondition = _DisposeCondition;
            }
        }
    }

}
