using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp;
using edu.stanford.nlp.util;
using java.util;
using System.IO;

namespace msra.nlp.tr
{
    public class Stemmer
    {                                                                
       StanfordCoreNLP pipeline = null;

        public Stemmer()
        {
        }

        void  Initial()
        {
            // Create StanfordCoreNLP object properties, with POS tagging
            // (required for lemmatization), and lemmatization
            Properties props;
            props = new Properties();
            props.put("annotators", "tokenize, ssplit, pos,lemma");
            props.setProperty("tokenizer.whitespace", "true");
            props.setProperty("ssplit.eolonly", "true");
            var dir = Directory.GetCurrentDirectory();
            //Directory.SetCurrentDirectory(@"E:\Users\v-mipeng\Software Install\Stanford NLP\stanford-corenlp-full-2015-04-20\");
            Directory.SetCurrentDirectory((string)GlobalParameter.Get(DefaultParameter.Field.stanford_model_dir));
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(dir);
        }

        /*Stem the given word with, return the stemmed word
         */ 
        public List<string> Stem(string word)
        {
                if (pipeline == null)
                {
                    Initial();
                }
                var lemmas = new List<String>();
                // create an empty Annotation just with the given text
                var document = new Annotation(word);
                // run all Annotators on this text
                try
                {
                    pipeline.annotate(document);
                }
                catch (Exception)
                {
                    return null;
                }
                // Iterate over all of the sentences found
                var senObj = new edu.stanford.nlp.ling.CoreAnnotations.SentencesAnnotation();
                var obj = document.get(senObj.getClass());
                var tokenObj = new edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation();
                var lemmaObj = new edu.stanford.nlp.ling.CoreAnnotations.LemmaAnnotation();
                var sentences = (ArrayList) obj;
                foreach (CoreMap sentence in sentences)
                {
                    // Iterate over all tokens in a sentence
                    lemmas.AddRange(from CoreLabel token in (ArrayList) sentence.get(tokenObj.getClass())
                        select (string) token.get(lemmaObj.getClass()));
                }
                return lemmas;
            }


    }
}
