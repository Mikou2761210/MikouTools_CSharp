using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Utils
{
    public class ConcurrentUniqueNumberGenerator : UniqueNumberGenerator
    {
        private readonly object _lock = new();

        public ConcurrentUniqueNumberGenerator(IEnumerable<int>? initialUsedIDs = null) : base(null)
        {
            lock (_lock)
                if (initialUsedIDs != null)
                    foreach (int id in initialUsedIDs) base.TryClaimUniqueNumber(id);
        }
        public new int GenerateUniqueNumber()
        {
            lock(_lock) return base.GenerateUniqueNumber();
        }

        public new void ReleaseUniqueNumber(int id)
        {
            lock (_lock) base.ReleaseUniqueNumber(id);
        }

        public new bool TryClaimUniqueNumber(int id)
        {
            lock (_lock) return base.TryClaimUniqueNumber(id);
        }
    }
}
