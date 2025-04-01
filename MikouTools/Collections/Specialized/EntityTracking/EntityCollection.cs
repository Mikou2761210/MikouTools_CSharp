using MikouTools.Collections.DictionaryEx.DualKey;
using MikouTools.Collections.DictionaryEx.Extend;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IEntityCollectionInitArgs { }
    public class EntityCollection<TId, T, TChild> : IEntityCollection<TId, T> , IDisposable where T : IIdentifiable<TId> where TChild : EntityCollection<TId, T, TChild> where TId : notnull
    {
        protected virtual IEntityRepository<TId ,T, TChild> _parent { get; set; }
        protected virtual ICollection<TId> _ids { get; set; } 
        public virtual IEnumerable<TId> Ids => _ids;

        protected virtual ICollection<TId> CreateItems()
        {
            return new Collection<TId>();
        }
        public EntityCollection(IEntityRepository<TId, T, TChild> parent, IEntityCollectionInitArgs? args = null)
        {
            Initialize(parent, args);

            if(_parent is null) throw new NullReferenceException(nameof(_parent));
            if (_ids is null) throw new NullReferenceException(nameof(_ids));
        }
        ~EntityCollection()
        {
            Dispose(false);
        }
        protected virtual void Initialize(IEntityRepository<TId, T, TChild> parent, IEntityCollectionInitArgs? args)
        {
            _parent = parent;
            _ids = CreateItems();
            _parent.RegisterCollection((TChild)this);
        }

        public virtual bool Add(TId id)
        {
            if (_parent.Contains(id))
            {
                _ids.Add(id);
                return true;
            }
            return false;
        }

        public virtual bool Remove(TId id)
        {
            return _ids.Remove(id);
        }

        public virtual bool TryGet(TId id, [MaybeNullWhen(false)] out T item) => _parent.TryGet(id, out item);

        public virtual bool Contains(TId id)
        {
            return _ids.Contains(id);
        }



        #region IDisposable    
        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
                _parent.UnregisterCollection((TChild)this);
                _disposed = true;
            }
        }
        #endregion
    }

}
