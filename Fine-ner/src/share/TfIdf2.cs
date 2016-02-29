using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using pml.file.reader;
using pml.file.writer;
using pml.type;

namespace msra.nlp.tr
{
    public class TfIdf2
    {
        readonly string corpusPath; // path of file storing the documents to be clustered. One line per document.
        readonly string vectorPath; // path of file to store the vector representation of document
        readonly string dfPath;     // path of file to store the df of corpus
        readonly string wordTablePath;  // path of file to store the word table of corpus
        //List<List<string>> documents = null;
        static int docNum = 0;
        static Dictionary<string, int> df = null;
        static Dictionary<string, int> wordTable = null;
        static object dfLocker = new object();
        static object abstractWordTableLocker = new object();




        private TfIdf2()
        {
        }


        public static List<Pair<int, double>> GetDocTfIdf(List<string> document)
        {
            if(df == null)
            {
                docNum = (int)GlobalParameter.Get(DefaultParameter.Field.dbpedia_abstract_num);
                LoadDf();
            }
            if(wordTable == null)
            {
                LoadAbstractWordTable();
            }
            return GetTfIdf(document);
        }

        public List<Pair<int, double>> GetDocTfIdf(string document)
        {
            if (df == null)
            {
                LoadDf();
            }
            if (wordTable == null)
            {
                LoadAbstractWordTable();
            }
            var tokenizer = msra.nlp.tr.TokenizerPool.GetTokenizer();
            var doc = tokenizer.Tokenize(document);
            msra.nlp.tr.TokenizerPool.ReturnTokenizer(tokenizer);
            return GetTfIdf(doc);
        }

        private static void LoadDf()
        {
           lock(dfLocker)
           {
               if(df == null)
               {
                   var dfPath = (string)GlobalParameter.Get(DefaultParameter.Field.dbpedia_abstract_df_file);
                   var reader = new LargeFileReader(dfPath);
                   var dic = new Dictionary<string, int>();
                   string line;

                   while((line = reader.ReadLine())!=null)
                   {
                       var array = line.Split('\t');
                       dic[array[0]] = int.Parse(array[1]);
                   }
                   reader.Close();
                   df = dic;
               }
           }

        }

        private static void LoadAbstractWordTable()
        {
            lock (abstractWordTableLocker)
            {
                if (wordTable == null)
                {
                    var wordTablePath = (string)GlobalParameter.Get(DefaultParameter.Field.dbpedia_abstract_word_table);
                    var reader = new LargeFileReader(wordTablePath);
                    var dic = new Dictionary<string, int>();
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var array = line.Split('\t');
                        dic[array[0]] = int.Parse(array[1]);
                    }
                    reader.Close();
                    wordTable = dic;
                }
            }
        }


        // Get Tf-idf vector of a document
        private static List<Pair<int, double>> GetTfIdf(List<string> document)
        {
            Dictionary<string, int> tf = new Dictionary<string, int>();
            // get tf
            foreach (var word in document)
            {
                int times;
                tf.TryGetValue(word, out times);
                tf[word] = times + 1;
            }
            // calculate tfidf: (1+log(tf))*log(n/df)
            List<Pair<int, double>> pairs = new List<Pair<int, double>>();
            foreach (var word in tf.Keys)
            {
                var tfidf = (1 + Math.Log(tf[word])) * Math.Log(docNum / df[word]);
                if (tfidf > 0.001)
                {
                    var pair = new Pair<int, double>(wordTable[word], tfidf);
                    pairs.Add(pair);
                }
            }
            pairs.Sort(pairs[0].GetByFirstComparer());
            return pairs;
        }


    }
}
