using MikouTools.CollectionTools.ThreadSafeCollections;
using MikouTools.UtilityTools.Threading;
using System.Diagnostics;

namespace MikouTools.ThreadTools
{
    public partial class ThreadResourceManager : IDisposable
    {
        Thread _thread;
        ThreadSafeList<ThreadResourceCleanup> _threadCleanupTasks = new ThreadSafeList<ThreadResourceCleanup>();
        int _pollingInterval = 1000;
        private readonly LockableProperty<bool> _dispose = new LockableProperty<bool>(false); 
        private readonly LockableProperty<bool> _threadStart = new LockableProperty<bool>(false);


        public int PollingInterval
        {
            get 
            {
                if (_dispose.Value) throw new ObjectDisposedException("ThreadResourceManager");
                return _pollingInterval; 
            }
            set
            {
                if (_dispose.Value) throw new ObjectDisposedException("ThreadResourceManager");
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
            _threadCleanupTasks.Lock();
            _thread = new Thread(() =>
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
                    Thread.Sleep(_pollingInterval);
                }
            });
            _thread.IsBackground = false;
            _thread.Name = ThreadName;
            _threadCleanupTasks.UnLock();
        }



        //Add
        public void CleanupTaskAdd(Thread TargetThread,Action DisposeAction, Func<Thread, bool>? DisposeCondition = null)
        {
            if (_dispose.Value) throw new ObjectDisposedException("ThreadResourceManager");

            _threadCleanupTasks.Add(new ThreadResourceCleanup(TargetThread, DisposeAction, DisposeCondition));

            if (!_threadStart.SetAndReturnOld(true))
            {
                _thread.Start();
            }
        }

        public void CleanupTaskAdd(ThreadManager TargetThreadManager, Action DisposeAction, Func<Thread, bool>? DisposeCondition = null)
        {
            CleanupTaskAdd(TargetThreadManager.thread, DisposeAction, DisposeCondition);
        }

        //Remove
        public bool Remove(Thread TargetThread)
        {
            if (_dispose.Value) throw new ObjectDisposedException("ThreadResourceManager");

            using (var LockHandle = _threadCleanupTasks.LockAndGetList())
            {
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
        }
        public bool RemoveAll(Thread TargetThread)
        {
            if (_dispose.Value) throw new ObjectDisposedException("ThreadResourceManager");

            bool removeflag = false;
            using (var LockHandle = _threadCleanupTasks.LockAndGetList())
            {
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
        }
        public bool DisposeRemove(Thread TargetThread)
        {
            if (_dispose.Value) throw new ObjectDisposedException("ThreadResourceManager");

            using (var LockHandle = _threadCleanupTasks.LockAndGetList())
            {
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
        }
        public bool DisposeRemoveAll(Thread TargetThread)
        {
            if (_dispose.Value) throw new ObjectDisposedException("ThreadResourceManager");

            bool removeflag = false;
            using (var LockHandle = _threadCleanupTasks.LockAndGetList())
            {
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
        }

        //Clear


        public void Clear()
        {
            if (_dispose.Value) throw new ObjectDisposedException("ThreadResourceManager");

            using (var LockHandle = _threadCleanupTasks.LockAndGetList())
            {
                for (int i = 0; i < LockHandle.List.Count; i++)
                {
                    LockHandle.List.RemoveAt(i);
                    i--;
                }
            }
        }

        public void DisposeClear()
        {
            if (_dispose.Value) throw new ObjectDisposedException("ThreadResourceManager");

            using (var LockHandle = _threadCleanupTasks.LockAndGetList())
            {
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
        }

        public void Dispose()
        {
            if(!_dispose.SetAndReturnOld(true))
            {
                _thread.Join();
            }
        }

    }
    public partial class ThreadResourceManager
    {
        internal class ThreadResourceCleanup
        {
            internal Thread TargetThread { get; private set; }
            internal Action DisposeAction { get; private set; }


            internal Func<Thread, bool>? DisposeCondition;


            internal ThreadResourceCleanup(Thread _TargetThread, Action _DisposeAction, Func<Thread, bool>? _DisposeCondition = null)
            {
                TargetThread = _TargetThread;
                DisposeAction = _DisposeAction;
                DisposeCondition = _DisposeCondition;
            }
        }
    }

}
