using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.DirtySort
{
    public interface IDirtySort<T>
    {
        public bool IsDirty { get; set; }
        public bool RedoLastSort();

        public IComparer<T>? LastComparer { get; set; }

    }
}
