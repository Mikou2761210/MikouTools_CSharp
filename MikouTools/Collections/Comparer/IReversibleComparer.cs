namespace MikouTools.Collections.Comparer
{
    public interface IReversibleComparer<T> : IComparer<T>
    {
        bool IsAscending { get; set; }
    }
}
