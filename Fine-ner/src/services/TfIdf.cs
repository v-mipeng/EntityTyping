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
    public class TfIdf
    {
        //List<List<string>> documents = null;
        static int docNum = 0;
        static Dictionary<string, int> df = null;
        static Dictionary<string, int> wordTable = null;
        static object dfLocker = new object();
        static object abstractWordTableLocker = new object();




        private TfIdf()
        {
        }


        public static Dictionary<int, double> GetDocTfIdf(List<string> document)
        {
            if(df == null)
            {
                //docNum = (int)GlobalParameter.Get(DefaultParameter.Field.dbpedia_abstract_num);
                docNum = 4305029;
                LoadDf();
            }
            if(wordTable == null)
            {
                LoadAbstractWordTable();
            }
            return GetTfIdf(document);
        }

        public Dictionary<int, double> GetDocTfIdf(string document)
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
                   var dfPath = (string)Parameter.GetParameter(Parameter.Field.dbpedia_abstract_df_file);
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
                    var wordTablePath = (string)Parameter.GetParameter(Parameter.Field.dbpedia_abstract_word_table);
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
        private static Dictionary<int,double> GetTfIdf(List<string> document)
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
            Dictionary<int, double> dic = new Dictionary<int, double>();
            foreach (var word in tf.Keys)
            {
                try
                {
                    var tfidf = (1 + Math.Log(tf[word])) * Math.Log(docNum / df[word]);
                    if (tfidf > 0.001)
                    {
                        dic[wordTable[word]] = tfidf;
                    }
                }
                catch(Exception)
                {
                    continue;
                }
            }
            return dic;
        }


    }
}
