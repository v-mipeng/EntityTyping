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
        }

        public void GetKeyWords()
        {
            var count = 0;
            for (var i = 0; i < sourceFiles.Count - 1; i++)
            {
                for (var j = i + 1; j < sourceFiles.Count; j++)
                {
                    var selector = new SelectorThread(sourceFiles[i], sourceFiles[j], desFiles[count]);
                    var thread = new Thread(new ThreadStart(selector.GetKeyWords));
                    thread.Start();
                    count++;
                }
            }
        }

       class SelectorThread
        {
            string fileOne = null;
            string fileTwo = null;
            string des = null;
            public  SelectorThread(string fileOne, string fileTwo, string des)
            {
                this.fileOne = fileOne;
                this.fileTwo = fileTwo;
                this.des = des;
            }
            public void GetKeyWords()
            {
                Console.WriteLine("Processing {0} and {1}", Path.GetFileNameWithoutExtension(fileOne), Path.GetFileNameWithoutExtension(fileTwo));
                var readerOne = new LargeFileReader(fileOne);
                var readerTwo = new LargeFileReader(fileTwo);
                var wordOccurNumDic = new Dictionary<string, int[]>();
                var wordMIDic = new Dictionary<string, double>();
                var line = "";
                var classOneNum = 0;
                var classTwoNum = 0;
                var tagger = PosTaggerPool.GetPosTagger();


                while ((line = readerOne.ReadLine()) != null)
                {
                    classOneNum++;
                    if (classOneNum % 1000 == 0)
                    {
                        Console.WriteLine("Have Processed: " + classOneNum);
                    }
                    var array = line.Split('\t');
                    var context = array[3];
                    //var tokens = Tokenize(context);
                    var set = new HashSet<string>();
                    var pairs = tagger.TagString(context);

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
                        int[] nums = null;
                        try
                        {
                            nums = wordOccurNumDic[token];
                            nums[0] += 1;
                        }
                        catch (Exception)
                        {
                            nums = new int[2];
                            nums[0] = 1;
                            wordOccurNumDic[token] = nums;
                        }
                    }
                }
                readerOne.Close();
                while ((line = readerTwo.ReadLine()) != null)
                {
                    classTwoNum++;
                    if (classTwoNum % 1000 == 0)
                    {
                        Console.WriteLine("Have Processed: " + classTwoNum);
                    }
                    var array = line.Split('\t');
                    var context = array[3];
                    //var tokens = Tokenize(context);
                    var set = new HashSet<string>();
                    var pairs = tagger.TagString(context);

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
                        int[] nums = null;
                        try
                        {
                            nums = wordOccurNumDic[token];
                            nums[1] += 1;
                        }
                        catch (Exception)
                        {
                            nums = new int[2];
                            nums[1] = 1;
                            wordOccurNumDic[token] = nums;
                        }
                    }
                }
                readerTwo.Close();
                PosTaggerPool.ReturnPosTagger(tagger);
                foreach (var token in wordOccurNumDic.Keys)
                {
                    wordMIDic[token] = MI.GetMI(classOneNum, classTwoNum, wordOccurNumDic[token][0], wordOccurNumDic[token][1]);
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

        }
        public static void Main(string[] args)
        {
            var sourceDir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\satori\train";
            var desDir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\tmp\";
            var selector = new KeyWordSelector(sourceDir, desDir);
            selector.GetKeyWords();
            //var str = "I like this beautiful Beijing.";

        }

    }
}
