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
    public class EntityCollection<T> : IEntityCollection<T> , IDisposable where T : IIdentifiable
    {
        protected readonly IEntityRepository<T, IEntityCollection<T>> _parent;
        protected virtual ICollection<int> _ids { get; set; } 
        public virtual IEnumerable<int> Ids => _ids;

        protected virtual ICollection<int> CreateItems()
        {
            return new Collection<int>();
        }
        public EntityCollection(IEntityRepository<T, IEntityCollection<T>> parent)
        {
            _parent = parent;
            _ids = CreateItems();
            _parent.RegisterCollection(this);
            Initialize();
        }
        ~EntityCollection()
        {
            Dispose(false);
        }
        protected virtual void Initialize()
        {

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

        public virtual bool Contains(int id) => _ids.Contains(id);



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
                _parent.UnregisterCollection(this);
                _disposed = true;
            }
        }
        #endregion
    }

}
