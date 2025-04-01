using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IEntityCollection<TId, T> where T : IIdentifiable<TId> where TId : notnull
    {
        IEnumerable<TId> Ids { get; }
        bool Add(TId id);
        bool Remove(TId id);
        bool TryGet(TId id, [MaybeNullWhen(false)] out T item);
    }
}
