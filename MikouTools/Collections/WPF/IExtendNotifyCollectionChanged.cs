using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.WPF
{
    public interface IExtendNotifyCollectionChanged : INotifyCollectionChanged
    {
        void BeginBulkUpdate();
        void EndBulkUpdate();
    }
}
