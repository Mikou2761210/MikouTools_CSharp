namespace MikouTools.Utils
{
    public class UniqueNumberGenerator
    {
        private readonly SortedSet<int> _availableIds = [];

        // The next unique ID to assign when no reusable IDs are available.
        private int _latestId = -1;

        public int GenerateUniqueNumber()
        {
            if (_availableIds.Count > 0)
            {
                int result = _availableIds.Min;
                _availableIds.Remove(result);
                return result;
            }
            return ++_latestId;
        }

        public void ReleaseUniqueNumber(int id)
        {
            if (id < 0 || id > _latestId)
                return;

            if (_latestId == id)
            {
                _latestId--;
                while (_availableIds.Remove(_latestId))
                {
                    _latestId--;
                }
            }
            else
            {
                _availableIds.Add(id);
            }
        }

        public bool TryClaimUniqueNumber(int id)
        {
            if (id < 0)
            {
                return false;
            }

            if (id <= _latestId)
            {
                return _availableIds.Remove(id);
            }
            else
            {
                for (int i = _latestId + 1; i < id; i++)
                {
                    _availableIds.Add(i);
                }
                _latestId = id;
                return true;
            }
        }
    }

}