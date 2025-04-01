using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.EntityTracking
{
    public interface IIdentifiable<TId> where TId : notnull
    {
        TId ID { get; }
    }
}
