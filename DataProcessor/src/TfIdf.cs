using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using pml.file.reader;
using pml.file.writer;
using pml.type;

namespace msra.nlp.tr.dr
{
    class TfIdf
    {
        readonly string corpusPath; // path of file storing the documents to be clustered. One line per document.
        readonly string vectorPath; // path of file to store the vector representation of document
        readonly string dfPath;     // path of file to store the df of corpus
        readonly string wordTablePath;  // path of file to store the word table of corpus
        //List<List<string>> documents = null;
        int docNum = 0;
        Dictionary<string, int> df = null;
        Dictionary<string, int> wordTable = null;


        public TfIdf(string corpusPath, string vectorPath, string dfPath, string wordTablePath) 
        {
            this.corpusPath = corpusPath;
            this.vectorPath = vectorPath;
            this.wordTablePath  = wordTablePath;
            this.dfPath = dfPath;
        }

        public void   GetVectorCorpus()
        {
            AnalyzeCorpus();
            SaveDf();
            SaveWordTable();
            OutputTfIdf();
        }

        private void AnalyzeCorpus()
        {
            df = new Dictionary<string, int>();
            wordTable = new Dictionary<string,int>();
            ReadOneDoc();
            HashSet<string> set = null;


            while(this.doc != null)
            {
                this.docNum++;
                if(this.docNum % 1000==0)
                {
                    Console.WriteLine(this.docNum);
                }
                var tokenizer = TokenizerPool.GetTokenizer();
                var document = tokenizer.Tokenize(doc);
                TokenizerPool.ReturnTokenizer(tokenizer);
                set = new HashSet<string>(document);
                //documents.Add(document);
                foreach(var word in set)
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
            foreach(var word in df.Keys)
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
            int docLabel = 1;
            int num =0;
            ReadOneDoc();

            while(this.doc != null)
            {
                var tokenizer = TokenizerPool.GetTokenizer();

                var document = tokenizer.Tokenize(doc);
                TokenizerPool.ReturnTokenizer(tokenizer);
                if (++num % 1000 == 0)
                {
                    Console.WriteLine(num);
                }
                var vector = GetTfIdf(document);
                writer.Write(docLabel);
                foreach(var value in vector)
                {
                    writer.Write("\t"+value.first+":"+value.second);
                }
                writer.Write("\r");
                ReadOneDoc();
                docLabel++;
            }
            writer.Close();
        }

        // Get Tf-idf vector of a document
        private List<Pair<int,double>> GetTfIdf(List<string> document)
        {
            Dictionary<string, int> tf = new Dictionary<string, int>();
            // get tf
            foreach(var word in document)
            {
                int times;
                tf.TryGetValue(word, out times);
                tf[word] = times + 1;
            }
            // calculate tfidf: (1+log(tf))*log(n/df)
            List<Pair<int, double>> pairs = new List<Pair<int, double>>();
            foreach(var word in tf.Keys)
            {
                var pair = new Pair<int,double>(wordTable[word],(1 + Math.Log(tf[word])) * Math.Log(this.docNum / df[word]));
                pairs.Add(pair);
            }
            pairs.Sort(pairs[0].GetByFirstComparer());
            return pairs;
        }

     

        string doc;
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
                doc = line.Split('\t')[1];
            }
        }
       
        public static void Mains(string[] args)
        {
            TfIdf tfidf = new TfIdf(
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\train.txt",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\docVectors.txt",
               @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\trainDf.txt",
               @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\trainWordTable.txt");
            tfidf.GetVectorCorpus();
        }

    }
}
