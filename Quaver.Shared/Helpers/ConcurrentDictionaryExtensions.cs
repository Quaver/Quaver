using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Quaver.Shared.Helpers
{
    public class ConcurrentDictionaryExtensions
    {
        /// <summary>
        ///     Convert list to concurrent dictionary
        /// </summary>
        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>
            (IEnumerable<TValue> source, Func<TValue, TKey> valueSelector)
        {
            return new ConcurrentDictionary<TKey, TValue>(source.ToDictionary(valueSelector));
        }
    }
}