using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IEntityRepository<T, TChild>  where T : IIdentifiable where TChild : IEntityCollection<T>
    {
        void RegisterCollection(TChild idCollection);
        bool UnregisterCollection(TChild idCollection);

        bool TryGet(int id, [MaybeNullWhen(false)] out T item);

        bool TryAdd(T item);

        bool Remove(int id);

        bool Contains(int id);
    }
}
