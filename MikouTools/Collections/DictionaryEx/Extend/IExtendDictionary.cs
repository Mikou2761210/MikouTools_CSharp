using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.DictionaryEx.Extend
{
    public interface IExtendDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        bool ContainsValue(TValue value);
        bool TryGetKey(TValue value, [MaybeNullWhen(false)] out TKey key);
        TValue Pop(TKey key);
        bool TryPop(TKey key, [MaybeNullWhen(false)] out TValue result);
        bool TryAdd(TKey key, TValue value);
    }
}