using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using pml.file.writer;
using System.IO;
using pml.type;

namespace msra.nlp.tr
{
    public class Statistic
    {
        /************************************************************************/
        /* Statistic the co-occurrence rate between test data and train data
         * 
        /************************************************************************/
        public static string StatisticCooccurrence(String trainFilePath, String testFilePath)
        {
            string path1 = trainFilePath;
            string path2 = testFilePath;
            FileReader reader = new LargeFileReader(path1);
            String line;
            HashSet<String> set = new HashSet<string>();
            Dictionary<string, int> hitNumByType = new Dictionary<string, int>();
            Dictionary<string, int> numByType = new Dictionary<string, int>();
            string[] array;

            // store mentions of train data into a set
            while ((line = reader.ReadLine()) != null)
            {
                array = line.Split('\t');
                set.Add(array[0]);
               
            }
            reader.Close();
            reader.Open(path2);
            int total = 0;
            int coNum = 0;
            //  get test data
            while ((line = reader.ReadLine()) != null)
            {
                total++;
                array = line.Split('\t');
                if (set.Contains(line.Split('\t')[0]))
                {
                    try
                    {
                        hitNumByType[array[1]] += 1;
                    }
                    catch (Exception)
                    {
                        hitNumByType[array[1]] = 1;
                    }
                    coNum++;
                }
                try
                {
                    numByType[array[1]] += 1;
                }
                catch (Exception)
                {
                    numByType[array[1]] = 1;
                }
            }
            reader.Close();
            StringBuilder buffer = new StringBuilder();
            foreach (String type in numByType.Keys)
            {
                buffer.Append("\t" + type + "\t" + (hitNumByType.ContainsKey(type) ? hitNumByType[type] : 0) + "\t" + numByType[type] + "\t" + 1.0 * (hitNumByType.ContainsKey(type) ? hitNumByType[type] : 0) / numByType[type] + "\r");
            }
            buffer.Append("\ttotal coverage is: " + (1.0 * coNum / total));
            return buffer.ToString();
        }

        /************************************************************************/
        /* Statistic the coverage of the  dictionary   
         * Note: all the entity is represent as lower case format*/
        /************************************************************************/
        public static string StatisticDicCoverage(String dicFile, String sourceFile)
        {
            string path1 = dicFile;
            string path2 = sourceFile;
            FileReader reader = new LargeFileReader(path1);
            String line;
            HashSet<String> set = new HashSet<string>();
            Dictionary<string, int> hitNumByType = new Dictionary<string, int>();
            Dictionary<string, int> NumByType = new Dictionary<string, int>();
            String[] array;

            while ((line = reader.ReadLine()) != null)
            {
                set.Add(line.Split('\t')[0]);
            }
            reader.Close();
            reader.Open(path2);
            int total = 0;
            int coNum = 0;
            while ((line = reader.ReadLine()) != null)
            {
                total++;
                array = line.Split('\t');
                if (set.Contains(array[0].ToLower()))
                {
                    coNum++;
                    try
                    {
                        hitNumByType[array[1]] += 1;
                    }
                    catch (Exception)
                    {
                        hitNumByType[array[1]] = 1;
                    }
                }
                try
                {
                    NumByType[array[1]] += 1;
                }
                catch (Exception)
                {
                    NumByType[array[1]] = 1;
                }
            }
            reader.Close();
            Console.WriteLine("dic coverage rate is: " + 1.0 * coNum / total);
            StringBuilder buffer = new StringBuilder();

            foreach (String type in NumByType.Keys)
            {
                buffer.Append("\t" + type + "\t" + (hitNumByType.ContainsKey(type) ? hitNumByType[type] : 0) + "\t" + NumByType[type] + "\t" + 1.0 * (hitNumByType.ContainsKey(type) ? hitNumByType[type] : 0) / NumByType[type]+"\r");
            }
            return buffer.ToString();
        }
    
        public static string StatisticNameListCoverage(String nameListFile, String sourceFile)
        {
            string path1 = nameListFile;
            string path2 = sourceFile;
            FileReader reader = new LargeFileReader(path1);
            String line;
            HashSet<String> set = new HashSet<string>();
            String[] array;

            while ((line = reader.ReadLine()) != null)
            {
                set.Add(line.Split('\t')[0]);
            }
            reader.Close();
            reader.Open(path2);
            int total = 0;
            int coNum = 0;
            while ((line = reader.ReadLine()) != null)
            {
                array = line.Split('\t');
                if(array[1].Equals("people.person"))
                {
                    total++;
                    if(set.Contains(array[0].ToLower()))
                    {
                        coNum++;
                    }
                }
            }
            reader.Close();
            Console.WriteLine("name list coverage is: " + 1.0 * coNum / total);

            return string.Format("{0}", 1.0*coNum/total);
        }
        
        public static string StatisticNameListCoverageByType(String nameListFile, String sourceFile)
        {
            string path1 = nameListFile;
            string path2 = sourceFile;
            FileReader reader = new LargeFileReader(path1);
            String line;
            HashSet<String> set = new HashSet<string>();
            Dictionary<string, int> hitNumByType = new Dictionary<string, int>();
            Dictionary<string, int> NumByType = new Dictionary<string, int>();
            String[] array;

            while ((line = reader.ReadLine()) != null)
            {
                set.Add(line);
                array = line.Split(' ');
                //foreach(string item in array)
                //{
                //    set.Add(item);
                //}
            }
            reader.Close();
            reader.Open(path2);
            int total = 0;
            int coNum = 0;
            while ((line = reader.ReadLine()) != null)
            {
                total++;
                array = line.Split('\t');

                if (set.Contains(array[0].ToLower()))
                {
                    if(!array[1].Equals("people.person"))
                    {
                        set.Remove(array[0].ToLower());
                    }
                    coNum++;
                    try
                    {
                        hitNumByType[array[1]] += 1;
                    }
                    catch (Exception)
                    {
                        hitNumByType[array[1]] = 1;
                    }
                }
                try
                {
                    NumByType[array[1]] += 1;
                }
                catch (Exception)
                {
                    NumByType[array[1]] = 1;
                }
            }
            reader.Close();
            Console.WriteLine("name list coverage rate is: " + 1.0 * coNum / total);
            StringBuilder buffer = new StringBuilder();

            foreach (String type in NumByType.Keys)
            {
                buffer.Append("\t" + type + "\t" + (hitNumByType.ContainsKey(type) ? hitNumByType[type] : 0) + "\t" + NumByType[type] + "\t" + 1.0 * (hitNumByType.ContainsKey(type) ? hitNumByType[type] : 0) / NumByType[type] + "\r");
            }
            return buffer.ToString();
        }

        public static string StatisticItemNumberByType(String sourceFile)
        {
            FileReader reader = new LargeFileReader(sourceFile);
            Dictionary<string, int> NumByType = new Dictionary<string, int>();
            string line;
            String[] array;

            int total = 0;
            while ((line = reader.ReadLine()) != null)
            {
                total++;
                array = line.Split('\t');
                try
                {
                    NumByType[array[1]] += 1;
                }
                catch (Exception)
                {
                    NumByType[array[1]] = 1;
                }
            }
            reader.Close();
            StringBuilder buffer = new StringBuilder();

            foreach (String type in NumByType.Keys)
            {
                buffer.Append("\t" + type + "\t" + NumByType[type] + "\r");
            }
            buffer.Append("\ttotal\t" + total);
            return buffer.ToString();
        }
        
        public static string StatisticRoundTokenInformation(String sourceFile)
        {
            FileReader reader = new LargeFileReader(sourceFile);
            FeatureExtractor extractor = new FeatureExtractor();
            // type-->(word-->times)
            Dictionary<string, Dictionary<string, int>> lastTokenNumByType = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, Dictionary<string, int>> nextTokenNumByType = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, int> dic = null ;

            string line;
            string lastToken;
            string nextToken;
            string type;
            String[] array;
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if((++count) % 1000 ==0)
                {
                    Console.WriteLine(count);
                }
                try
                {
                    array = line.Split('\t');
                    type = array[1];
                    // get last token
                    lastToken = extractor.GetLastToken(array[2], array[0]).ToLower();
                    if (lastToken == null)
                    {
                        lastToken = "null";
                    }
                    else
                    {
                        lastToken = DataCenter.GetStemmedWord(lastToken);
                    }
                    // get next token
                    nextToken = extractor.GetNextToken(array[2], array[0]).ToLower();
                    if (nextToken == null)
                    {
                        nextToken = "null";
                    }
                    else
                    {
                        nextToken = DataCenter.GetStemmedWord(nextToken);
                    }
                    // deal last token
                    lastTokenNumByType.TryGetValue(type, out dic);
                    if (dic == null)
                    {
                        dic = new Dictionary<string, int>();
                    }
                    try
                    {
                        dic[lastToken] += 1;
                    }
                    catch (Exception)
                    {
                        dic[lastToken] = 1;
                    }
                    lastTokenNumByType[type] = dic;
                    // deal next token
                    nextTokenNumByType.TryGetValue(type, out dic);
                    if (dic == null)
                    {
                        dic = new Dictionary<string, int>();
                    }
                    try
                    {
                        dic[nextToken] += 1;
                    }
                    catch (Exception)
                    {
                        dic[nextToken] = 1;
                    }
                    nextTokenNumByType[type] = dic;
                }
                catch(Exception)
                {
                    continue; 
                }
            }
            reader.Close();
            StringBuilder buffer = new StringBuilder();
            // report last token information
            buffer.Append("last token report: word:(per times|loc times|org times)\r");
            List<Pair<string, int>> list = new List<Pair<string, int>>();
            Comparer<Pair<string, int>> comparer = new Pair<string,int>().GetBySecondReverseComparer();
            foreach(String item in lastTokenNumByType["people.person"].Keys )
            {
                Pair<string, int> pair = new Pair<string, int>(item, lastTokenNumByType["people.person"][item]);
                list.Add(pair);
            }
            list.Sort(comparer);
            count = 0;
            int locNum;
            int orgNum;
            foreach (Pair<string,int> item in list)
            {
                count++;
                try
                {
                    locNum = lastTokenNumByType["location.location"][item.first];
                }
                catch (Exception)
                {
                    locNum = 0;
                }
                try
                {
                    orgNum = lastTokenNumByType["organization.organization"][item.first];
                }
                catch (Exception)
                {
                    orgNum = 0;
                }
                buffer.Append("\t" + item.first + ":(" + item.second + "|" + locNum + "|" + orgNum + ")");
                if (count % 5 == 0)
                {
                    buffer.Append("\r");
                }
            }
            buffer.Append("\r");
            // report next token information
            buffer.Append("next token report:  word:(per times|loc times|org times)\r");
            list.Clear();
            foreach (String item in nextTokenNumByType["people.person"].Keys)
            {
                Pair<string, int> pair = new Pair<string, int>(item, nextTokenNumByType["people.person"][item]);
                list.Add(pair);
            }
            list.Sort(comparer);
             count = 0;
            foreach (Pair<string, int> item in list)
            {
                count++;
                try
                {
                    locNum = nextTokenNumByType["location.location"][item.first];
                }
                catch (Exception)
                {
                    locNum = 0;
                }
                try
                {
                    orgNum = nextTokenNumByType["organization.organization"][item.first];
                }
                catch (Exception)
                {
                    orgNum = 0;
                }
                buffer.Append("\t" + item.first + ":(" + item.second + "|" + locNum + "|" + orgNum + ")");
                if (count % 5 == 0)
                {
                    buffer.Append("\r");
                }
            }
            return buffer.ToString();
        }
   
        public static string StatisticWithinTokenInfomation(String sourceFile)
        {
            FileReader reader = new LargeFileReader(sourceFile);
            FeatureExtractor extractor = new FeatureExtractor();
            // type-->(word-->times)
            Dictionary<string, Dictionary<string, int>> firstTokenNumByType = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, Dictionary<string, int>> finalTokenNumByType = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, int> dic = null ;

            string line;
            string firstToken;
            string finalToken;
            string type;
            String[] array;
            string[] wordArray;
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if ((++count) % 1000 == 0)
                {
                    Console.WriteLine(count);
                }
                try
                {
                    array = line.Split('\t');
                    type = array[1];
                    wordArray = array[0].Split('\t');
                    // get first token
                    firstToken = wordArray[0].ToLower();
                    if (firstToken == null)
                    {
                        firstToken = "null";
                    }
                    else
                    {
                        firstToken = DataCenter.GetStemmedWord(firstToken);
                    }
                    // get final token
                    finalToken = wordArray[wordArray.Length - 1].ToLower();
                    if (finalToken == null)
                    {
                        finalToken = "null";
                    }
                    else
                    {
                        finalToken = DataCenter.GetStemmedWord(finalToken);
                    }
                    // deal first token
                    firstTokenNumByType.TryGetValue(type, out dic);
                    if (dic == null)
                    {
                        dic = new Dictionary<string, int>();
                    }
                    try
                    {
                        dic[firstToken] += 1;
                    }
                    catch (Exception)
                    {
                        dic[firstToken] = 1;
                    }
                    firstTokenNumByType[type] = dic;
                    // deal final token
                    finalTokenNumByType.TryGetValue(type, out dic);
                    if (dic == null)
                    {
                        dic = new Dictionary<string, int>();
                    }
                    try
                    {
                        dic[finalToken] += 1;
                    }
                    catch (Exception)
                    {
                        dic[finalToken] = 1;
                    }
                    finalTokenNumByType[type] = dic;
                }
                catch(Exception)
                {
                    continue; 
                }
            }
            reader.Close();
            StringBuilder buffer = new StringBuilder();
            // report first token information
            buffer.Append("first token report: word:(per times|loc times|org times)\r");
            List<Pair<string, int>> list = new List<Pair<string, int>>();
            Comparer<Pair<string, int>> comparer = new Pair<string,int>().GetBySecondReverseComparer();
            foreach(String item in firstTokenNumByType["people.person"].Keys )
            {
                Pair<string, int> pair = new Pair<string, int>(item, firstTokenNumByType["people.person"][item]);
                list.Add(pair);
            }
            list.Sort(comparer);
            count = 0;
            int locNum;
            int orgNum;
            foreach (Pair<string,int> item in list)
            {
                count++;
                try
                {
                    locNum = firstTokenNumByType["location.location"][item.first];
                }
                catch (Exception)
                {
                    locNum = 0;
                }
                try
                {
                    orgNum = firstTokenNumByType["organization.organization"][item.first];
                }
                catch (Exception)
                {
                    orgNum = 0;
                }
                buffer.Append("\t" + item.first + ":(" + item.second + "|" + locNum + "|" + orgNum + ")");
                if (count % 5 == 0)
                {
                    buffer.Append("\r");
                }
            }
            buffer.Append("\r");
            // report final token information
            buffer.Append("final token report:  word:(per times|loc times|org times)\r");
            list.Clear();
            foreach (String item in finalTokenNumByType["people.person"].Keys)
            {
                Pair<string, int> pair = new Pair<string, int>(item, finalTokenNumByType["people.person"][item]);
                list.Add(pair);
            }
            list.Sort(comparer);
             count = 0;
            foreach (Pair<string, int> item in list)
            {
                count++;
                try
                {
                    locNum = finalTokenNumByType["location.location"][item.first];
                }
                catch (Exception)
                {
                    locNum = 0;
                }
                try
                {
                    orgNum = finalTokenNumByType["organization.organization"][item.first];
                }
                catch (Exception)
                {
                    orgNum = 0;
                }
                buffer.Append("\t" + item.first + ":(" + item.second + "|" + locNum + "|" + orgNum + ")");
                if (count % 5 == 0)
                {
                    buffer.Append("\r");
                }
            }
            return buffer.ToString();
            }
        
        public static void Refresh()
        {
            DataCenter.RefreshStemDic();
        }
    }
}
