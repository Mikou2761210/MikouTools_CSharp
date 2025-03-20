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
    public class EntityRepository<T, TChild> : IEntityRepository<T, TChild> where T : IIdentifiable where TChild : IEntityCollection<T>

    {
        protected virtual IDictionary<int, T> _collections { get; set; }
        protected virtual ICollection<TChild> _children { get; set; }
        public virtual ICollection<TChild> Children => _children;

        public virtual IDictionary<int, T> CreateCollection()
        {
            return new Dictionary<int, T>();
        }

        public virtual ICollection<TChild> CreateChildren()
        {
            return new Collection<TChild>();
        }


        public EntityRepository()
        {
            _collections = CreateCollection();
            _children = CreateChildren();
        }

        public virtual void RegisterCollection(TChild idCollection) => _children.Add(idCollection);

        public virtual bool UnregisterCollection(TChild idCollection) => _children.Remove(idCollection);

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
