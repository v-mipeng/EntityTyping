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
using pml.collection.map;
using pml.file.reader;
using pml.file.writer;

namespace msra.nlp.tr
{
    public class Tokenizer
    {
        private Tokenizer() { }

        static StanfordCoreNLP pipeline = null;
        private static object locker = new object();

        public static List<string> Tokenize(string sequence)
        {

            lock (locker)
            {
                if (pipeline == null)
                {
                    Initial();
                }
                var document = new Annotation(sequence);
                pipeline.annotate(document);
                var tokenObj = new edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation();
                var tokens = (ArrayList)document.get(tokenObj.getClass());
                return (from CoreMap token in tokens select token.ToString()).ToList();
            }
        }

        private static void Initial(string modelDir = null)
        {
            var props = new Properties();
            props.put("annotators", "tokenize");
            var dir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory((string)GlobalParameter.Get(DefaultParameter.Field.stanford_model_dir));
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(dir);
        }
    }
}
