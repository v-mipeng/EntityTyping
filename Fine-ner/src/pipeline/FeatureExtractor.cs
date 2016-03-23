using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.type;

namespace msra.nlp.tr
{
    abstract class FeatureExtractor
    {
        public abstract void ExtractFeature();

        public List<Event> ExtractFeatureForQuery(List<Pair<string, string>> queries, int begin, int end) 
        {
            return null;
        }

        public void AddFeature()
        { }
    }
}
