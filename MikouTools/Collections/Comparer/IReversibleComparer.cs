using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Comparer
{
    public interface IReversibleComparer<T> : IComparer<T>
    {
        bool IsAscending { get; set; }
    }
}
