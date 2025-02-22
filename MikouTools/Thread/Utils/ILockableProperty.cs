namespace MikouTools.Thread.Utils
{
    public interface ILockableProperty<T>
    {
        T Value { get; set; }
        T AccessValueWhileLocked { get; set; }
        void Lock();
        void UnLock();
        void ExecuteWithLock(Action action);
        T SetAndReturnOld(T newvalue);
    }

}
