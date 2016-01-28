using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.util;
using java.util;
using pml.type;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    public class StanfordNer : Ner
    {
        StanfordCoreNLP pipeline = null;
        List<Pair<string, string>> nerPairs = null;
        edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation tokenObj = new edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation();
        edu.stanford.nlp.ling.CoreAnnotations.NamedEntityTagAnnotation nerObj = new edu.stanford.nlp.ling.CoreAnnotations.NamedEntityTagAnnotation();
        edu.stanford.nlp.ling.CoreAnnotations.TextAnnotation textObj = new edu.stanford.nlp.ling.CoreAnnotations.TextAnnotation();


        public StanfordNer()
        {
        }

        void Initial()
        {
            // Create StanfordCoreNLP object properties, with POS tagging
            // (required for lemmatization), and lemmatization
            Properties props;
            props = new Properties();
            props.put("annotators", "tokenize, ssplit, pos,lemma, ner");
            props.setProperty("tokenizer.whitespace", "true");
            props.setProperty("ssplit.eolonly", "true");
            props.setProperty("ner.useSUTime", "0");
            props.setProperty("ner.model", @"D:\Codes\C#\EntityTyping\Fine-ner\input\stanford models\edu\stanford\nlp\models\ner\english.all.3class.distsim.crf.ser.gz");
            var dir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(@"D:\Codes\C#\EntityTyping\Fine-ner\input\stanford models\");
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(dir);
        }

        /*Stem the given word with, return the stemmed word
         */
        public void FindNer(string context)
        {
            if (context == null)
            {
                throw new Exception("Input should not be null for finding NER!");
            }
            if (pipeline == null)
            {
                Initial();
            }
            // create an empty Annotation just with the given text
            var annotation = new Annotation(context);
            // run all Annotators on this text
            pipeline.annotate(annotation);
            var ners = (edu.stanford.nlp.util.CoreMap)annotation.get(nerObj.getClass());
            var sentences = (ArrayList)annotation.get(new edu.stanford.nlp.ling.CoreAnnotations.SentencesAnnotation().getClass());
            nerPairs = new List<Pair<string, string>>();
            foreach (CoreMap sentence in sentences)
            {
                var prevNerToken = "O";
                var currNerToken = "O";
                bool newToken = true;
                var buffer = new StringBuilder();

                foreach (CoreLabel token in (ArrayList)sentence.get(tokenObj.getClass()))
                {
                    currNerToken = (string)token.get(nerObj.getClass());
                    var word = (string)token.get(textObj.getClass());
                    // Strip out "O"s completely, makes code below easier to understand
                    if (currNerToken.Equals("O"))
                    {
                        if (!prevNerToken.Equals("O") && (buffer.Length > 0))
                        {
                            if (prevNerToken.Equals("LOCATION") || prevNerToken.Equals("PERSON") || prevNerToken.Equals("ORGANIZATION"))
                            {
                                nerPairs.Add(new Pair<string, string>(buffer.ToString(), prevNerToken));
                            }
                            buffer.Clear();
                            newToken = true;
                            prevNerToken = "O";
                        }
                        continue;
                    }
                    if (newToken)
                    {
                        prevNerToken = currNerToken;
                        newToken = false;
                        buffer.Append(word);
                        continue;
                    }

                    if (currNerToken.Equals(prevNerToken))
                    {
                        buffer.Append(" " + word);
                    }
                    else
                    {
                        // We're done with the current entity - print it out and reset
                        // TODO save this token into an appropriate ADT to return for useful processing..
                        if (prevNerToken.Equals("LOCATION") || prevNerToken.Equals("PERSON") || prevNerToken.Equals("ORGANIZATION"))
                        {
                            nerPairs.Add(new Pair<string, string>(buffer.ToString(), prevNerToken));
                        }
                        buffer.Clear();
                        buffer.Append(word);
                        newToken = true;
                    }
                    prevNerToken = currNerToken;
                }
                if (!prevNerToken.Equals("O") && buffer.Length > 0)
                {
                    if (prevNerToken.Equals("LOCATION") || prevNerToken.Equals("PERSON") || prevNerToken.Equals("ORGANIZATION"))
                    {
                        nerPairs.Add(new Pair<string, string>(buffer.ToString(), prevNerToken));
                    }
                }
            }
        }

        public  List<Pair<string, string>> GetEntities()
        {
            return nerPairs;
        }

        Regex regex = new Regex(@"\s");

        /// <summary>
        /// Get the ner type of mention after invoking FindNer() function
        /// </summary>
        /// <param name="mention"></param>
        /// <returns>
        /// The mention types: PERSON, LOCATION, ORGANIZATION and UNKNOW.
        /// </returns>
        public string GetNerType(string mention)
        {
            if (mention == null)
            {
                throw new Exception("Mention should not be null for finding ner type");
            }
            mention = regex.Replace(mention, "").ToLower();
            foreach (var pair in nerPairs)
            {
                var str1 = regex.Replace(pair.first, "").ToLower();
                if (str1.Contains(mention) || mention.Contains(str1))
                {
                    return pair.second;
                }
            }
            return "UNKNOW";
        }

    }
}
