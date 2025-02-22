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
