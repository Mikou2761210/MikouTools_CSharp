using MikouTools.UtilityTools.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.ThreadTools
{
    public sealed class CustomManualResetEvent : EventWaitHandle
    {
        public CustomManualResetEvent(bool initialState) : base(initialState, EventResetMode.ManualReset) { }

        LockableProperty<int> count = new LockableProperty<int>(0);
        public int WaitCount
        {
            get { return count.Value; }
        }
        public new void WaitOne()
        {
            count.Lock();
            count.AccessValueWhileLocked++;
            count.UnLock();
            base.WaitOne();
            count.Lock();
            count.AccessValueWhileLocked--;
            count.UnLock();

        }
    }
}
