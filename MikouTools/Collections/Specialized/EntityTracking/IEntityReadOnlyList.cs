using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IEntityReadOnlyList<T> : IEntityCollection<T> ,IReadOnlyList<int> where T : IIdentifiable
    {

    }
}
