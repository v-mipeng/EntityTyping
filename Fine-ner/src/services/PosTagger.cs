using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using com.sun.tools.jdi;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.tagger.maxent;
using pml.type;
using ikvm.extensions;

namespace msra.nlp.tr
{
    public class PosTagger
    {
        private MaxentTagger tagger = null;

        internal PosTagger()
        {
        }

        public List<Pair<string, string>> TagString(string sequence)
        {
            if (tagger == null)
            {
                Initial();
            }
            if (sequence == null)
            {
                throw new Exception("Sequence should not be null for tagging!");
            }
            var tagged = tagger.tagString(sequence);
            var list = new List<Pair<string, string>>();
            var array = tagged.Split(' ');
            foreach (var w in array)
            {
                try
                {
                    var word = w.trim();
                    var array2 = word.Split(new char[]{'_'},2);
                    var pair = new Pair<string, string>(array2[0], array2[1]);
                    list.Add(pair);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return list;
        }

        void Initial(string modelFile = null)
        {
            var path = Path.Combine((string)Parameter.GetParameter(Parameter.Field.stanford_model_dir), @"edu\stanford\nlp\models\pos-tagger\english-left3words\english-left3words-distsim.tagger");
            tagger = new MaxentTagger(modelFile ?? path);

        }

    }
}
