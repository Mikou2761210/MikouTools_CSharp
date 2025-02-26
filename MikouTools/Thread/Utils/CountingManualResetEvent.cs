namespace MikouTools.Thread.Utils
{
    public sealed class CountingManualResetEvent : EventWaitHandle
    {
        public CountingManualResetEvent(bool initialState) : base(initialState, EventResetMode.ManualReset) { }

        private readonly LockableProperty<int> count = new(0);
        public int WaitCount
        {
            get { return count.Value; }
        }
        public new void WaitOne()
        {
            count.EnterLock();
            count.AccessValueWhileLocked++;
            count.ExitLock();
            base.WaitOne();
            count.EnterLock();
            count.AccessValueWhileLocked--;
            count.ExitLock();

        }
    }
}
