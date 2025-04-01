using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public struct EntityEnumerator<TId, T> : IEnumerator<T>, System.Collections.IEnumerator where T : IIdentifiable<TId> where TId : notnull
    {
        private readonly IEntityReadOnlyList<TId, T> _entityReadOnlyList;

        private int _index;
        private T? _current;

        internal EntityEnumerator(IEntityReadOnlyList<TId, T> entityCollection)
        {
            _entityReadOnlyList = entityCollection;
            _index = 0;
            _current = default;
        }

        public readonly void Dispose() { }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_entityReadOnlyList.Count)
            {
                _entityReadOnlyList.TryGet(_entityReadOnlyList[_index], out _current);
                _index++;
                if(_current == null) return MoveNext();
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            _index = _entityReadOnlyList.Count + 1;
            _current = default;
            return false;
        }

        public readonly T Current => _current!;

        readonly object? System.Collections.IEnumerator.Current
        {
            get
            {
                if (_index == 0 || _index > _entityReadOnlyList.Count)
                    throw new InvalidOperationException();
                return Current;
            }
        }

        void System.Collections.IEnumerator.Reset()
        {
            _index = 0;
            _current = default;
        }
    }
}
