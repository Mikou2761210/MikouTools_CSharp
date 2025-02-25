using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    /// <summary>
    /// The current structure of MultiLevelCascadeFilteredViewBase cannot be made thread-safe. It may simply be that my technical skills and knowledge are insufficient, but in order to implement thread safety, we have no choice but to either completely change the structure of MultiLevelCascadeFilteredViewBase or create a thread-safe version of it. Since there are no plans to use it at the moment, development has been halted.
    /// </summary>
    /// <typeparam name="FilterKey"></typeparam>
    /// <typeparam name="ItemValue"></typeparam>
    [ObsoleteAttribute("The current structure of MultiLevelCascadeFilteredViewBase cannot be made thread-safe. It may simply be that my technical skills and knowledge are insufficient, but in order to implement thread safety, we have no choice but to either completely change the structure of MultiLevelCascadeFilteredViewBase or create a thread-safe version of it. Since there are no plans to use it at the moment, development has been halted.", false)]

    public class ConcurrentMultiLevelCascadeCollection<FilterKey, ItemValue> : MultiLevelCascadeCollection<FilterKey, ItemValue> where FilterKey : notnull where ItemValue : notnull
    {
        //Not implemented
    }
}
