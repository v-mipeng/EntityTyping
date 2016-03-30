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
using pml.type;

namespace msra.nlp.tr
{
    public class Tokenizer
    {
        internal Tokenizer() 
        {
            Initial();
        }

        StanfordCoreNLP pipeline = null;
        edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation tokenObj = new edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation();
        edu.stanford.nlp.ling.CoreAnnotations.BeginIndexAnnotation offsetObj = new edu.stanford.nlp.ling.CoreAnnotations.BeginIndexAnnotation();

        public  List<Pair<string, int>> Tokenize(string sequence)
        {
            if(sequence == null)
            {
                throw new Exception("Sequence should not be null for tokenizer.");
            }
            if (pipeline == null)
            {
                Initial();
            }
            var document = new Annotation(sequence);
            pipeline.annotate(document);

            var tokens = (ArrayList)document.get(tokenObj.getClass());
            var list = new List<Pair<string, int>>();
            foreach (edu.stanford.nlp.ling.CoreLabel token in tokens)
            {
                var offset = (int)token.get(offsetObj.getClass());
                var text = token.toString();
                list.Add(new Pair<string, int>(text, offset));
            }
            return list;
        }

        private  void Initial(string modelDir = null)
        {
            var props = new Properties();
            props.put("annotators", "tokenize");
            props.setProperty("ner.useSUTime", "false");
            var dir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory((string)Parameter.GetParameter(Parameter.Field.stanford_model_dir));
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(dir);
        }
    }
}
