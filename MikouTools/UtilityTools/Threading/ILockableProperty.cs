namespace MikouTools.UtilityTools.Threading
{
    public interface ILockableProperty<T>
    {
        T Value { get; set; }
        T AccessValueWhileLocked { get; set; }
        void ExecuteWithLock(Action action);
        T SetAndReturnOld(T newvalue);
    }

}
