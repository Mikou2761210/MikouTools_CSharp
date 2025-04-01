using MikouTools.Collections.DictionaryEx.DualKey;
using MikouTools.Collections.DictionaryEx.Extend;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IEntityRepositoryInitArgs { }
    public class EntityRepository<TId, T, TChild> : IEntityRepository<TId, T, TChild> where T : IIdentifiable<TId> where TChild : IEntityCollection<TId, T> where TId : notnull

    {
        protected virtual IDictionary<TId, T> _collections { get; set; }
        protected virtual ICollection<TChild> _children { get; set; }
        public virtual ICollection<TChild> Children => _children;

        protected virtual IDictionary<TId, T> CreateCollection()
        {
            return new Dictionary<TId, T>();
        }

        protected virtual ICollection<TChild> CreateChildren()
        {
            return new Collection<TChild>();
        }


        public EntityRepository(IEntityRepositoryInitArgs? args = null)
        {
            Initialize(args);

            if (_collections is null) throw new NullReferenceException(nameof(_collections));
            if (_children is null) throw new NullReferenceException(nameof(_children));
        }
        protected virtual void Initialize(IEntityRepositoryInitArgs? args)
        {
            _collections = CreateCollection();
            _children = CreateChildren();
        }

        public virtual void RegisterCollection(TChild idCollection) => _children.Add(idCollection);

        public virtual bool UnregisterCollection(TChild idCollection) => _children.Remove(idCollection);

        public virtual bool TryGet(TId id, [MaybeNullWhen(false)] out T item) => _collections.TryGetValue(id, out item);

        public virtual T? TryGet(TId? id) 
        {
            T? result = default;
            if (id != null)
                TryGet((TId)id, out result);
            return result;
        }

        public virtual bool TryAdd(T item) => _collections.TryAdd(item.ID, item);

        public virtual bool Remove(TId id)
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

        public virtual bool Contains(TId id) => _collections.ContainsKey(id);
    }
}
