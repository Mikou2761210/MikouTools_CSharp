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
    public class EntityCollection<T, TChild> : IEntityCollection<T> , IDisposable where T : IIdentifiable where TChild : EntityCollection<T,TChild>
    {
        protected readonly IEntityRepository<T, TChild> _parent;
        protected virtual ICollection<int>? _ids { get; set; } 
        public virtual IEnumerable<int> Ids => _ids!;

        protected virtual ICollection<int> CreateItems()
        {
            return new Collection<int>();
        }
        public EntityCollection(IEntityRepository<T, TChild> parent)
        {
            _parent = parent;
            Initialize();
        }
        ~EntityCollection()
        {
            Dispose(false);
        }
        protected virtual void Initialize()
        {
            _ids = CreateItems();
            _parent.RegisterCollection((TChild)this);
        }

        public virtual bool Add(int id)
        {
            if (_ids == null) throw new NullReferenceException(nameof(_ids));
            if (_parent.Contains(id))
            {
                _ids.Add(id);
                return true;
            }
            return false;
        }

        public virtual bool Remove(int id)
        {
            if (_ids == null) throw new NullReferenceException(nameof(_ids));
            return _ids.Remove(id);
        }

        public virtual bool TryGet(int id, [MaybeNullWhen(false)] out T item) => _parent.TryGet(id, out item);

        public virtual bool Contains(int id)
        {
            if (_ids == null) throw new NullReferenceException(nameof(_ids));
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
