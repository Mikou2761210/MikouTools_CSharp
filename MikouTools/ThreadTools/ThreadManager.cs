using MikouTools.CollectionTools.SignalingCollections;
using MikouTools.CollectionTools.ThreadSafeCollections;
using MikouTools.UtilityTools.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikouTools.ThreadTools
{
    public enum ThreadManagerState
    {
        Idle, Running, Wait, Dispose
    }
    public class ThreadManager : IDisposable
    {
        internal Thread thread;

        CountSignalingQueue<Process> ProcessQueue = new CountSignalingQueue<Process>(count => count > 0);

        //public Exception? LastException = null;

        public string? ThreadName
        {
            get { return thread.Name; }
            set { thread.Name = value; }
        }

        public ThreadManager(string? _ThreadName = null, bool IsBackground = true , ApartmentState? ApartmentState = null)
        {
            thread = new Thread(() =>
            {
                while (!dispose.Value)
                {
                    _threadManagerState.Value = ThreadManagerState.Wait;
                    ProcessQueue.CountCheckAndWait();

                    if (dispose.Value) break;

                    _threadManagerState.Value = ThreadManagerState.Running;
                    ProcessQueue.Dequeue().Invoke();

                }

            });
            ThreadName = _ThreadName;
            thread.IsBackground = IsBackground;
            if (ApartmentState != null)
                thread.SetApartmentState((ApartmentState)ApartmentState);
        }

        LockableProperty<ThreadManagerState> _threadManagerState = new LockableProperty<ThreadManagerState>(ThreadManagerState.Idle);
        public ThreadManagerState ThreadState { get { return _threadManagerState.Value; } }


        public bool Invoke(Action action, Exception? exception)
        {
            if (dispose.Value) throw new ObjectDisposedException("ThreadManager");

            if (_threadManagerState.SetAndReturnOld(ThreadManagerState.Wait) == ThreadManagerState.Idle)
                thread.Start();

            Process process = new Process(action);
            ProcessQueue.Enqueue(process);
            process.ProcessCompletedWait();
            if(process.InvokeException!= null)
            {
                exception = process.InvokeException;
                return false;
            }
            else
            {
                return true;
            }


        }

        public void InvokeAsync(Action action)
        {
            if (dispose.Value) throw new ObjectDisposedException("ThreadManager");

            if (_threadManagerState.SetAndReturnOld(ThreadManagerState.Wait) == ThreadManagerState.Idle)
                thread.Start();

            ProcessQueue.Enqueue(new Process(action));
        }
        LockableProperty<bool> dispose = new LockableProperty<bool>(false);

        public void Dispose()
        {
            if (!dispose.SetAndReturnOld(true))
            {
                ProcessQueue.AddAllow = false;
                ProcessQueue.DisableWait();
                thread.Join();
                _threadManagerState.Value  = ThreadManagerState.Dispose;
                while(ProcessQueue.Count > 0)
                {
                    ProcessQueue.Dequeue().Dispose();
                }
            }
        }



        private class Process : IDisposable
        {
            Action _action;

            CustomManualResetEvent _manualResetEvent = new CustomManualResetEvent(false);

            LockableProperty<bool> _invokecheck = new LockableProperty<bool>(false);

            public Exception? InvokeException { get; private set; }
            public Process(Action action)
            {
                _action = action;
            }

            public bool Invoke()
            {
                if (dispose.Value) throw new ObjectDisposedException("Process");
                bool result = false;
                if (!_invokecheck.SetAndReturnOld(true))
                {
                    try
                    {
                        _action.Invoke();
                        result = true;
                    }
                    catch(Exception ex)
                    {
                        InvokeException = ex;
                    }
                    finally
                    {
                        _manualResetEvent.Set();
                    }
                }
                return result;
            }
            public void ProcessCompletedWait()
            {
                ProcessCompletedWait(-1);
            }
            public void ProcessCompletedWait(int millisecondsTimeout)
            {
                if (dispose.Value) throw new ObjectDisposedException("Process");
                _manualResetEvent.WaitOne(millisecondsTimeout);
            }


            LockableProperty<bool> dispose = new LockableProperty<bool>(false);
            public void Dispose()
            {
                if (!dispose.SetAndReturnOld(true))
                {
                    if (_invokecheck.SetAndReturnOld(true))
                    {
                        ProcessCompletedWait();
                    }
                    else
                    {
                        _manualResetEvent.Set();
                    }
                    _manualResetEvent.Dispose();
                }
            }
        }
    }



}
