namespace MikouTools.CollectionTools.SyncCollections
{


    [Obsolete]
    public class SyncableDictionary<TMasterKey, TSyncKey, TValue> where TMasterKey : notnull where TSyncKey : notnull
    {
        // マスターキーとインデックスの対応辞書
        private readonly Dictionary<TSyncKey, int> _masterKeyToIndex = new Dictionary<TSyncKey, int>();

        // マスター値のリスト
        private readonly List<TValue> _masterValues = new List<TValue>();

        // 同期された値のリスト（キーごと）
        private readonly Dictionary<TMasterKey, List<TValue>> _syncedValuesByKey = new Dictionary<TMasterKey, List<TValue>>();

        public SyncableDictionary()
        {
        }

        public TValue this[TMasterKey key, TSyncKey key2]
        {
            get
            {
                return _syncedValuesByKey[key][_masterKeyToIndex[key2]];
            }
            set
            {
                _syncedValuesByKey[key][_masterKeyToIndex[key2]] = value;
            }
        }

        // 新しいマスター項目を追加するメソッド
        public void AddMasterItem(TSyncKey key, TValue value)
        {
            if (!_masterKeyToIndex.ContainsKey(key))
            {
                // 新しいマスターキーとインデックスを追加
                _masterKeyToIndex.Add(key, _masterValues.Count);
                _masterValues.Add(value);

                // 同期された全てのリストに新しい値を追加
                foreach (var syncedValue in _syncedValuesByKey)
                {
                    syncedValue.Value.Add(value);
                }
            }
        }
    }
}
