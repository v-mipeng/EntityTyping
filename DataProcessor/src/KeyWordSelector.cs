using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using msra.nlp.tr;
using pml.file.writer;

namespace msra.nlp.tr.dp
{
    class KeyWordSelector
    {
        List<string> sourceFiles = null;
        List<string> desFiles = null;

        public KeyWordSelector(string sourceDir, string desDic)
        {
            this.sourceFiles = Directory.GetFiles(sourceDir, "*.txt").ToList();
            this.desFiles = new List<string>();
            for (var i = 0; i < sourceFiles.Count-1 ;i++)
            {
                for (var j = i + 1; j < sourceFiles.Count; j++)
                {
                    this.desFiles.Add(Path.Combine(desDic, Path.GetFileNameWithoutExtension(sourceFiles[i]) + "_" + Path.GetFileNameWithoutExtension(sourceFiles[j])));
                }
            }
        }

        public void GetKeyWords()
        {
            var count = 0;
           for(var i= 0; i<sourceFiles.Count-1;i++)
           {
               for (var j = i + 1; j < sourceFiles.Count; j++)
               {
                   Console.WriteLine("Processing {0} and {1}", Path.GetFileNameWithoutExtension(sourceFiles[i]),Path.GetFileNameWithoutExtension(sourceFiles[j]));
                   GetKeyWords(sourceFiles[i], sourceFiles[j], desFiles[count]);
                   Console.WriteLine("Done!");
                   count++;
               }
           }
        }




        private void GetKeyWords(string fileOne, string fileTwo, string des)
        {
            var readerOne = new LargeFileReader(fileOne);
            var readerTwo = new LargeFileReader(fileTwo);
            var wordOccurNumDic = new Dictionary<string, int[]>();
            var wordMIDic = new Dictionary<string, double>();
            var line = "";
            var classOneNum = 0;
            var classTwoNum = 0;
            var stemmer = StemmerPool.GetStemmer();

            while((line = readerOne.ReadLine())!=null)
            {
                classOneNum++;
                var array = line.Split('\t');
                var context = array[3];
                var tokens = Tokenize(context);
                var set = new HashSet<string>();

                
                foreach(var token in tokens)
                {
                    var tokenStemmed = stemmer.Stem(token)[0];
                    set.Add(tokenStemmed);
                }
                foreach(var token in set)
                {
                    int[] nums = null;
                    try
                    {
                        nums = wordOccurNumDic[token];
                        nums[0] += 1;
                    } 
                    catch(Exception)
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
                var array = line.Split('\t');
                var context = array[3];
                var tokens = Tokenize(context);
                var set = new HashSet<string>();

                foreach (var token in tokens)
                {
                    var tokenStemmed = stemmer.Stem(token)[0];
                    set.Add(tokenStemmed);
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
            foreach(var token in wordOccurNumDic.Keys)
            {
                wordMIDic[token] = MI.GetMI(classOneNum, classTwoNum, wordOccurNumDic[token][0], wordOccurNumDic[token][1]);
            }
            SaveKeyWords(wordMIDic, des);
        }

        private void SaveKeyWords(Dictionary<string,double> keyWordDic, string des)
        {
            var writer = new LargeFileWriter(des, FileMode.Create);
            int count = 0;
            foreach(var item in keyWordDic.OrderByDescending(key => key.Value))
            {
                count++;
                writer.WriteLine(item.Key + "\t" + item.Value);
                if(count>100)
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

        public static void Mains(string[] args)
        {


        }

    }
}
