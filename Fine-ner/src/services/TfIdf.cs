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
        readonly string corpusPath; // path of file storing the documents to be clustered. One line per document.
        readonly string vectorPath; // path of file to store the vector representation of document
        readonly string dfPath;     // path of file to store the df of corpus
        readonly string wordTablePath;  // path of file to store the word table of corpus
        //List<List<string>> documents = null;
        int docNum = 4305029;
        Dictionary<string, int> df = null;
        Dictionary<string, int> wordTable = null;


        public TfIdf(string corpusPath, string vectorPath, string dfPath, string wordTablePath)
        {
            this.corpusPath = corpusPath;
            this.vectorPath = vectorPath;
            this.wordTablePath = wordTablePath;
            this.dfPath = dfPath;
        }

        public void GetVectorCorpus()
        {
            //AnalyzeCorpus();
            //SaveDf();
            //SaveWordTable();
            LoadDf();
            LoadAbstractWordTable();
            OutputTfIdf();
        }

        private void AnalyzeCorpus()
        {
            df = new Dictionary<string, int>();
            wordTable = new Dictionary<string, int>();
            ReadOneDoc();
            HashSet<string> set = null;


            while (this.doc != null)
            {
                this.doc = this.doc.ToLower();
                this.docNum++;
                if (this.docNum % 1000 == 0)
                {
                    Console.WriteLine(this.docNum);
                }
                var tokenizer = msra.nlp.tr.TokenizerPool.GetTokenizer();
                var document = tokenizer.Tokenize(doc);
                msra.nlp.tr.TokenizerPool.ReturnTokenizer(tokenizer);
                set = new HashSet<string>(document);
                //documents.Add(document);
                foreach (var word in set)
                {
                    int times;
                    df.TryGetValue(word, out times);
                    df[word] = times + 1;
                    if (!wordTable.ContainsKey(word))
                    {
                        int count = wordTable.Count;
                        wordTable[word] = count;
                    }
                }
                ReadOneDoc();
            }

        }

        private void SaveDf()
        {
            var writer = new LargeFileWriter(dfPath, FileMode.Create);
            foreach (var word in df.Keys)
            {
                writer.WriteLine(word + "\t" + df[word]);
            }
            writer.Close();
        }

        private void SaveWordTable()
        {
            var writer = new LargeFileWriter(wordTablePath, FileMode.Create);
            foreach (var word in wordTable.Keys)
            {
                writer.WriteLine(word + "\t" + wordTable[word]);
            }
            writer.Close();
        }

        private void OutputTfIdf()
        {
            var writer = new LargeFileWriter(vectorPath, FileMode.Create);
            int num = 0;
            ReadOneDoc();
            var tokenizer = TokenizerPool.GetTokenizer();

            while (this.doc != null)
            {
                this.doc = this.doc.ToLower();
                var document = tokenizer.Tokenize(doc);
                if ((++num % 1000) == 0)
                {
                    Console.WriteLine(num);
                }
                try
                {
                    var vector = GetTfIdf(document);
                    writer.Write(title);
                    foreach (var value in vector)
                    {
                        writer.Write("\t" + value.first + ":" + value.second);
                    }
                    writer.WriteLine("");
                    ReadOneDoc();
                }
                catch (Exception)
                {
                    ReadOneDoc();
                }

            }
            TokenizerPool.ReturnTokenizer(tokenizer);
            writer.Close();
        }

        private void LoadDf()
        {
            {
                if (df == null)
                {
                    var reader = new LargeFileReader(dfPath);
                    var dic = new Dictionary<string, int>();
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var array = line.Split('\t');
                        dic[array[0]] = int.Parse(array[1]);
                    }
                    reader.Close();
                    df = dic;
                }
            }
        }

        private void LoadAbstractWordTable()
        {
            if (wordTable == null)
            {
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



        // Get Tf-idf vector of a document
        private List<Pair<int, double>> GetTfIdf(List<string> document)
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
                var tfidf = (1 + Math.Log(tf[word])) * Math.Log(this.docNum / df[word]);
                if (tfidf > 0.001)
                {
                    var pair = new Pair<int, double>(wordTable[word], tfidf);
                    pairs.Add(pair);
                }
            }
            pairs.Sort(pairs[0].GetByFirstComparer());
            return pairs;
        }

        string doc;
        string title;
        FileReader reader = null;
        private void ReadOneDoc()
        {
            if (reader == null)
            {
                reader = new LargeFileReader(corpusPath);
            }
            var line = reader.ReadLine();
            if (line == null)
            {
                reader.Close();
                doc = null;
                reader = null;
                return;
            }
            else
            {
                var array = line.Split('\t');
                title = array[0];
                doc = array[1];
            }
        }
    }
}
