using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized
{
    public class UniqueNumberGenerator
    {
        private readonly SortedSet<int> _availableIds = new SortedSet<int>();

        // The next unique ID to assign when no reusable IDs are available.
        private int _latestId = -1;

        public int NewId()
        {
            if (_availableIds.Count > 0)
            {
                int result = _availableIds.Min;
                _availableIds.Remove(result);
                return result;
            }
            return ++_latestId;
        }

        public void ReturnId(int id)
        {
            if (id < 0 || id > _latestId)
                return;

            if (_latestId == id)
            {
                _latestId = _availableIds.Max;
            }
            else
            {
                _availableIds.Add(id);
            }
        }
    }
}
