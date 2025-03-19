using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IEntityRepository<T>
    {
        bool RegisterCollection(IEntityCollection<T> idCollection);
        bool UnregisterCollection(IEntityCollection<T> idCollection);

        bool TryGet(int id, [MaybeNullWhen(false)] out T item);

        bool TryAdd(T item);

        bool Remove(int id);

        bool Contains(int id);
    }
}
