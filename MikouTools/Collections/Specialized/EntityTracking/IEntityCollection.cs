using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IEntityCollection<T> where T : IIdentifiable
    {
        IEnumerable<int> Ids { get; }
        bool Add(T item);
        bool Remove(int id);
        bool TryGet(int id, [MaybeNullWhen(false)] out T item);
    }
}
