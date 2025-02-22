using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.MultiLock
{
    public interface IMultiLock
    {
        /// <summary>
        /// True disables Add operations.
        /// </summary>
        bool AddLock { get; set; }
        /// <summary>
        /// True disables Remove operations.
        /// </summary>
        bool RemoveLock { get; set; }
    }
}
