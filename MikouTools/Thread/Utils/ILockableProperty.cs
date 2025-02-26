namespace MikouTools.Thread.Utils
{
    public interface ILockableProperty<T>
    {
        T Value { get; set; }
        T AccessValueWhileLocked { get; set; }
        void EnterLock();
        void ExitLock();
        void ExecuteWithLock(Action action);
        T SetAndReturnOld(T newvalue);
    }

}
