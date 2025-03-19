using MikouTools.Collections.Dictionary.DualKey;
using MikouTools.Collections.Dictionary.Extend;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public class EntityRepository<T> : IEntityRepository<T> where T : IIdentifiable
    {
        protected readonly IExtendDictionary<int, T> _collections;
        protected readonly HashSet<IEntityCollection<T>> _children = [];
        public IReadOnlyCollection<IEntityCollection<T>> Children => _children;

        public virtual IExtendDictionary<int, T> CreateCollection()
        {
            return new DualKeyDictionary<int, T>();
        }

        public EntityRepository()
        {
            _collections = CreateCollection();
        }

        public virtual bool RegisterCollection(IEntityCollection<T> idCollection) => _children.Add(idCollection);

        public virtual bool UnregisterCollection(IEntityCollection<T> idCollection) => _children.Remove(idCollection);

        public virtual bool TryGet(int id, [MaybeNullWhen(false)] out T item) => _collections.TryGetValue(id, out item);

        public virtual bool TryAdd(T item) => _collections.TryAdd(item.ID, item);

        public virtual bool Remove(int id)
        {
            if (_collections.Remove(id))
            {
                foreach (var child in _children)
                {
                    child.Remove(id);
                }
                return true;
            }
            return false;
        }

        public virtual bool Contains(int id) => _collections.ContainsKey(id);
    }
}
