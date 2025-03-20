using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IEntityRepository<T, TChild> where TChild : IEntityCollection<T> where T : IIdentifiable
    {
        void RegisterCollection(TChild idCollection);
        bool UnregisterCollection(TChild idCollection);

        bool TryGet(int id, [MaybeNullWhen(false)] out T item);

        bool TryAdd(T item);

        bool Remove(int id);

        bool Contains(int id);
    }
}
