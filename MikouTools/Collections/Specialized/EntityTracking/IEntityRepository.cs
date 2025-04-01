using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IEntityRepository<TId, T, TChild> where T : IIdentifiable<TId> where TChild : IEntityCollection<TId, T> where TId : notnull
    {
        void RegisterCollection(TChild idCollection);
        bool UnregisterCollection(TChild idCollection);

        bool TryGet(TId id, [MaybeNullWhen(false)] out T item);

        bool TryAdd(T item);

        bool Remove(TId id);

        bool Contains(TId id);
    }
}
