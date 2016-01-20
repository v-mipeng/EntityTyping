using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.dcoref;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.util;
using java.util;

namespace msra.nlp.tr
{
    public class SentenceSpliter
    {
        StanfordCoreNLP pipeline = null;
        private static object locker = new object();

        public SentenceSpliter()
        {
            Initial();
        }

        public  List<string> SplitSequence(string sequence)
        {
            if (pipeline == null)
            {
                Initial();
            }
            var document = new Annotation(sequence);
            pipeline.annotate(document);
            var senObj = new edu.stanford.nlp.ling.CoreAnnotations.SentencesAnnotation();
            var sentences = (ArrayList)document.get(senObj.getClass());
            return (from CoreMap sentence in sentences select sentence.ToString()).ToList();
        }


        void Initial(string modelDir = null)
        {
            var props = new Properties();
            props.put("annotators", "tokenize, ssplit");
            var dir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(modelDir ?? (string)GlobalParameter.Get(DefaultParameter.Field.stanford_model_dir));
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(dir);
        }



    }
}
