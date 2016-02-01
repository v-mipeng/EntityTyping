using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using msra.nlp.tr;
using pml.file.writer;
using System.Threading;

namespace msra.nlp.tr
{
    class KeyWordSelector
    {
        List<string> sourceFiles = null;
        List<string> desFiles = null;
        static List<Tuple> tuples = new List<Tuple>();

        public KeyWordSelector(string sourceDir, string desDic)
        {
            this.sourceFiles = Directory.GetFiles(sourceDir, "*.txt").ToList();
            this.desFiles = new List<string>();
            for (var i = 0; i < sourceFiles.Count - 1; i++)
            {
                for (var j = i + 1; j < sourceFiles.Count; j++)
                {
                    this.desFiles.Add(Path.Combine(desDic, Path.GetFileNameWithoutExtension(sourceFiles[i]) + " & " + Path.GetFileName(sourceFiles[j])));
                }
            }
            for(var i = 0;i<sourceFiles.Count;i++)
            {
                tuples.Add(null);
            }
        }

        public void GetKeyWords()
        {
            var count = 0;
            List<Thread> threads = new List<Thread>();
            for (var i = 0; i < sourceFiles.Count;i++ )
            {
                var selector = new KeywordThread(sourceFiles[i], i);
                var thread = new Thread(new ThreadStart(selector.GetKeyWordInfo));
                threads.Add(thread);
                thread.Start();
            }
            for (var i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }
            threads.Clear();
            for (var i = 0; i < sourceFiles.Count - 1; i++)
            {
                for (var j = i + 1; j < sourceFiles.Count; j++)
                {
                    var selector = new MIThread(tuples[i], tuples[j], desFiles[count]);
                    var thread = new Thread(new ThreadStart(selector.GetMI));
                    threads.Add(thread);
                    thread.Start();
                    count++;
                }
            }
            for (var i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }
        }


        class KeywordThread
        {
            string source = null;
            int threadID = -1;

            public KeywordThread(string source, int threadID)
            {
                this.source = source;
                this.threadID = threadID;
            }

            public void GetKeyWordInfo()
            {
                Console.WriteLine("Thread {0} start.", threadID);
                var reader = new LargeFileReader(source);
                var wordOccurNumDic = new Dictionary<string, int>();
                var line = "";
                var classNum = 0;
                var tagger = PosTaggerPool.GetPosTagger();
                var set = new HashSet<string>();

                while ((line = reader.ReadLine()) != null)
                {
                    if(classNum>10000)
                    {
                        break;
                    }
                    classNum++;
                    if (classNum % 1000 == 0)
                    {
                        Console.WriteLine("Thread {0} has processed: {1}",threadID,classNum);
                    }
                    var array = line.Split('\t');
                    var pairs = tagger.TagString(array[3]);
                    set.Clear();

                    foreach (var pair in pairs)
                    {
                        if (pair.second.StartsWith("N") || pair.second.StartsWith("V") || pair.second.StartsWith("J"))
                        {
                            var tokenStemmed = Generalizer.Generalize(pair.first).ToLower();
                            set.Add(tokenStemmed);
                        }
                    }
                    foreach (var token in set)
                    {
                        int num = 0;
                        wordOccurNumDic.TryGetValue(token, out num);
                        wordOccurNumDic[token] = num + 1;
                    }
                }
                reader.Close();
                PosTaggerPool.ReturnPosTagger(tagger);
                KeyWordSelector.tuples[threadID] = new Tuple(classNum, wordOccurNumDic);
            }


        }


        class MIThread
        {
            Tuple tupleOne = null;
            Tuple tupleTwo = null;
            string des = null;
            public MIThread(Tuple tupleOne, Tuple tupleTwo, string des)
            {
                this.tupleOne = tupleOne;
                this.tupleTwo = tupleTwo;
                this.des = des;
            }
            public void GetMI()
            {
                var classOneNum = tupleOne.ItemNum;
                var classTwoNum = tupleTwo.ItemNum;
                var keys = tupleOne.WordOccurDic.Keys.ToList();
                keys.AddRange(tupleTwo.WordOccurDic.Keys);
                var wordMIDic = new Dictionary<string,double>();

                foreach (var token in keys)
                {
                    int N1 = 0;
                    int N0 = 0;
                   if(!tupleOne.WordOccurDic.TryGetValue(token, out N1))
                   {
                       N1 = 0;
                   }
                   if (!tupleTwo.WordOccurDic.TryGetValue(token, out N0))
                   {
                       N0 = 0;
                   }
                    wordMIDic[token] = MI.GetMI(classOneNum, classTwoNum, N1,N0);
                }
                SaveKeyWords(wordMIDic, des);
                Console.WriteLine("Done!");
            }
            private void SaveKeyWords(Dictionary<string, double> keyWordDic, string des)
            {
                var writer = new LargeFileWriter(des, FileMode.Create);
                int count = 0;
                foreach (var item in keyWordDic.OrderByDescending(key => key.Value))
                {
                    count++;
                    writer.WriteLine(item.Key + "\t" + item.Value);
                    if (count > 1000)
                    {
                        break;
                    }
                }
                writer.Close();
            }

        }

        class Tuple
        {
            int itemNum = -1;
            Dictionary<string, int> wordOccurDic = null;

            public Tuple(int itemNum, Dictionary<string, int> wordOccurDic)
            {
                this.itemNum = itemNum;
                this.wordOccurDic = wordOccurDic;
            }

            public int ItemNum
            {
                get
                {
                    return itemNum;
                }
            }

            public Dictionary<string, int> WordOccurDic
            {
                get
                {
                    return wordOccurDic;
                }
            }
        }

        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\b[\w]{2,}\b");

        private List<string> Tokenize(string sequence)
        {
            var matchCollection = regex.Matches(sequence);
            List<string> list = new List<string>();
            foreach (System.Text.RegularExpressions.Match match in matchCollection)
            {
                list.Add(match.Groups[0].Value);
            }
            return list;
        }


        public static void Mains(string[] args)
        {
            //var sourceDir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\satori\train";
            //var desDir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\tmp\";
            //var selector = new KeyWordSelector(sourceDir, desDir);
            //selector.GetKeyWords();
            //var str = "I like this beautiful Beijing.";
            Temp();
        }

        static void Temp()
        {
            var sourceDir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\tmp\";
            var des = @"D:\Codes\Project\EntityTyping\Fine-ner\input\keywords.txt";
            var files = Directory.GetFiles(sourceDir);
            var reader = new LargeFileReader();
            var writer = new LargeFileWriter(des, FileMode.Create);
            var line = "";
            var keyWords = new HashSet<string>();

            foreach(var file in files)
            {
                reader.Open(file);
                int count = 0;
                while((line = reader.ReadLine())!=null)
                {
                    count++;
                    if(count>100)
                    {
                        break;
                    }
                    var array = line.Split('\t');
                    keyWords.Add(array[0]);
                }
            }
            reader.Close();
            foreach(var word in keyWords)
            {
                writer.WriteLine(word);
            }
            writer.Close();
        }

    }
}
