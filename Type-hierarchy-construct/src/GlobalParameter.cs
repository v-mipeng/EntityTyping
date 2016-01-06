using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr.HierarchyExtractor
{
    class GlobalParameter
    {
        internal static Dictionary<object, object> parameters = new Dictionary<object, object>();

        internal static void Set(Object key, Object value)
        {
            parameters[key] = value;
        }

        internal static Object Get(Object key)
        {
            Object value;
            return parameters.TryGetValue(key, out value) ? value : null;
        }

        internal static int featureNum = 0;

        private GlobalParameter() { }
    }
}
