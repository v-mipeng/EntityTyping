using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pml.collection.map
{
    public class HashMap<K, V> : Dictionary<K, V>, IMap<K, V>
    {
        public HashMap():base()
        {
        }

        public HashMap(int initialSize) : base(initialSize)
        {
        }

        public HashMap(IMap<K, V> map) : base(map)
        {
        }


        public object Get(K key)
        {
            try
            {
                object value = null;
                value = base[key];
                return value;
            }
            catch (Exception)
            {
                return null;
            }
        }
      
        public IEnumerable<K> SortByValue(Comparer<V> comparer = null)
        {
            var keys = this.Keys.ToList();
            var mapComparer = new MapComparer<K>(this, comparer);
            keys.Sort(mapComparer);
            return keys;
        }

        public IEnumerable<K> SortReverseByValue()
        {
            var keys = this.Keys.ToList();
            var mapComparer = new ReverseMapComparer<K>(this);
            keys.Sort(mapComparer);
            return keys;
        }

        private class MapComparer<K> : Comparer<K>
        {
            IMap<K, V> map = null;
            private Comparer<V> comparer = null;

            public MapComparer(IMap<K, V> map, Comparer<V> comparer)
            {
                this.map = map;
                this.comparer = comparer;
            }

            public override int Compare(K x, K y)
            {
                return comparer == null ? ((IComparable) map[x]).CompareTo(map[y]) : comparer.Compare(map[x], map[y]);
            }
        }

        private class ReverseMapComparer<K> : Comparer<K>
        {
            private IMap<K, V> map = null; 
            public ReverseMapComparer(IMap<K, V> map)
            {
                this.map = map;
            }

            public override int Compare(K x, K y)
            {
                return -((IComparable) map[x]).CompareTo(map[y]);
            }
        }

    }
}
