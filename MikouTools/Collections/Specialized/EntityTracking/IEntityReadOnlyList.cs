using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IEntityReadOnlyList<TId, T> : IEntityCollection<TId, T> ,IReadOnlyList<TId> where T : IIdentifiable<TId> where TId : notnull
    {

    }
}
