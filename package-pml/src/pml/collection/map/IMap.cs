using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pml.collection.map
{
    public  interface IMap<K,V>  : IDictionary<K,V>
    {
        /// <summary>
        ///     Get the value corresponding to the given key.
        /// </summary>
        /// <param name="key">
        ///     The key corresponding to the value wanted to get.
        /// </param>
        /// <returns>
        ///     The value if exists, elsewhere null.
        /// </returns>
        object Get(K key);

        /// <summary>
        ///     Sort map by value.
        ///     If not given the value comparer, this will sort the map with default comparer of value.
        /// </summary>
        /// <param name="comparer">
        ///     Custom comparer of the value, default is null
        /// </param>
        /// <returns>
        ///     A list of keys whose corresponding values is sorted.
        /// </returns>
        IEnumerable<K> SortByValue(Comparer<V> comparer = null);

        /// <summary>
        ///     Sort map by the reverse order of values.
        /// </summary>
        /// <returns>
        ///     A list of keys whose corresponding values is reversely sorted.
        /// </returns>
        IEnumerable<K> SortReverseByValue();

    }
}
