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
    public interface IEntityCollectionInitArgs { }
    public class EntityCollection<T, TChild> : IEntityCollection<T> , IDisposable where T : IIdentifiable where TChild : EntityCollection<T,TChild>
    {
        protected virtual IEntityRepository<T, TChild> _parent { get; set; }
        protected virtual ICollection<int> _ids { get; set; } 
        public virtual IEnumerable<int> Ids => _ids;

        protected virtual ICollection<int> CreateItems()
        {
            return new Collection<int>();
        }
        public EntityCollection(IEntityRepository<T, TChild> parent, IEntityCollectionInitArgs? args = null)
        {
            Initialize(parent, args);

            if(_parent is null) throw new NullReferenceException(nameof(_parent));
            if (_ids is null) throw new NullReferenceException(nameof(_ids));
        }
        ~EntityCollection()
        {
            Dispose(false);
        }
        protected virtual void Initialize(IEntityRepository<T, TChild> parent, IEntityCollectionInitArgs? args)
        {
            _parent = parent;
            _ids = CreateItems();
            _parent.RegisterCollection((TChild)this);
        }

        public virtual bool Add(int id)
        {
            if (_parent.Contains(id))
            {
                _ids.Add(id);
                return true;
            }
            return false;
        }

        public virtual bool Remove(int id)
        {
            return _ids.Remove(id);
        }

        public virtual bool TryGet(int id, [MaybeNullWhen(false)] out T item) => _parent.TryGet(id, out item);

        public virtual bool Contains(int id)
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
