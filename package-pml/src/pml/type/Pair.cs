using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pml.type
{
    /*TODO: Complete this type
     */
    public class Pair<T1, T2>
    {
        public T1 first;
        public T2 second;
        
        public Pair()
        {
        }

        public Pair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }
        public Comparer<Pair<T1,T2>> GetByFirstComparer()
        {
            return new ComparerByFirst();
        }
        public Comparer<Pair<T1, T2>> GetBySecondComparer()
        {
            return new ComparerBySecond();
        }
        public Comparer<Pair<T1, T2>> GetByFirstReverseComparer()
        {
            return new ComparerReverseByFirst();
        }
        public Comparer<Pair<T1, T2>> GetBySecondReverseComparer()
        {
            return new ComparerReverseBySecond();
        }
        class ComparerByFirst : Comparer<Pair<T1, T2>>
        {
            public ComparerByFirst() { }
            public override int Compare(Pair<T1, T2> pair1, Pair<T1, T2> pair2)
            {
                return ((IComparable<T1>)pair1.first).CompareTo(pair2.first);
            }
        }
         class ComparerReverseByFirst : Comparer<Pair<T1, T2>>
        {
            public override int Compare(Pair<T1, T2> pair1, Pair<T1, T2> pair2)
            {
                return -((IComparable<T1>)pair1.first).CompareTo(pair2.first);
            }
        }
        class ComparerBySecond : Comparer<Pair<T1, T2>>
        {
            public override int Compare(Pair<T1, T2> pair1, Pair<T1, T2> pair2)
            {
                return ((IComparable<T2>)pair1.second).CompareTo(pair2.second);
            }
        }
         class ComparerReverseBySecond : Comparer<Pair<T1, T2>>
        {
            public override int Compare(Pair<T1, T2> pair1, Pair<T1, T2> pair2)
            {
                return -((IComparable<T2>)pair1.second).CompareTo(pair2.second);
            }
        }

    }
}
