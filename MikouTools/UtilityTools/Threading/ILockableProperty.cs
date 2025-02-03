using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.UtilityTools.Threading
{
    public interface ILockableProperty<T>
    {
        T Value { get; set; }
        T AccessValueWhileLocked { get; set; }
        void ExecuteWithLock(Action action);
        T SetAndReturnOld(T newvalue);
    }

}
