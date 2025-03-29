namespace MikouTools.Collections.ComparerEx
{
    public interface IReversibleComparer<T> : IComparer<T>
    {
        bool IsAscending { get; set; }
    }
}
