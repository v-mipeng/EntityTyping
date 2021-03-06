﻿using pml.file.reader;
using pml.file.writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using msra.nlp.tr.eval;

namespace msra.nlp.tr
{
    public class OpenNer
    {

        public static void Temp()
        {
            var source = @"D:\Data\DBpedia\mapping based types";

            var desDir = "";
            var dic = new Dictionary<string, FileWriter>();
            var reader = new pml.file.reader.LargeFileReader(source);
            var des = @"D:\Data\DBpedia\entity type pairs.txt";
            var writer = new LargeFileWriter(des, FileMode.Create);
            string line;
            System.Text.RegularExpressions.Regex entityRegex = new System.Text.RegularExpressions.Regex(@"/([^>/]+)>\s<");
            System.Text.RegularExpressions.Regex typeRegex = new System.Text.RegularExpressions.Regex(@"ontology/(\w+)>\s\.$");
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {

                string entity = null;
                string type = null;
                if (entityRegex.IsMatch(line))
                {
                    var match = entityRegex.Match(line);
                    entity = match.Groups[1].Value;
                }
                if (typeRegex.IsMatch(line))
                {
                    var match = typeRegex.Match(line);
                    type = match.Groups[1].Value;
                }
                if (entity != null && type != null)
                {
                    if (++count % 10000 == 0)
                    {
                        Console.WriteLine(count);
                    }
                    writer.WriteLine(entity + "\t" + type);
                }
            }
            reader.Close();
            writer.Close();
        }

        /// <summary>
        /// map types
        /// </summary>
        public static void Temp2()
        {
            var dbpediaToSatoriDic = new Dictionary<string, Dictionary<string, int>>();
            var satoriMentionDic = new Dictionary<string, string>();
            var satoriEntityDic = new Dictionary<string, string>();
            var dbpedia = @"D:\Data\DBpedia\entity type pairs.txt";
            var satori = @"D:\Codes\C#\EntityTyping\Fine-ner\input\feature\train.txt";
            var des = @"D:\Codes\C#\EntityTyping\Fine-ner\input\db2satori.txt";
            var dbpediaReader = new LargeFileReader(dbpedia);
            var satoriReader = new LargeFileReader(satori);
            var writer = new LargeFileWriter(des, FileMode.Create);
            var line = "";
            Dictionary<string, int> dic = null;

            while ((line = satoriReader.ReadLine()) != null)
            {
                var array = line.Split('\t');
                satoriMentionDic[array[0]] = array[2];
                satoriEntityDic[array[1]] = array[2];
            }
            satoriReader.Close();
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\s+");
            System.Text.RegularExpressions.Regex deleteBrace = new System.Text.RegularExpressions.Regex(@"\(\w+\)");
            var count = 0;

            while ((line = dbpediaReader.ReadLine()) != null)
            {
                if (++count % 10000 == 0)
                {
                    Console.WriteLine(count);
                }
                var array = line.Split('\t');
                var entity = deleteBrace.Replace(array[0], "");
                entity = regex.Replace(entity, " ").Trim();
                if (satoriEntityDic.ContainsKey(entity))
                {

                    try
                    {
                        dic = dbpediaToSatoriDic[array[1]];
                    }
                    catch (Exception)
                    {
                        dic = new Dictionary<string, int>();
                        dbpediaToSatoriDic[array[1]] = dic;
                    }
                    try
                    {
                        dic[satoriEntityDic[entity]] += 1;
                    }
                    catch (Exception)
                    {
                        dic[satoriEntityDic[entity]] = 1;
                    }
                }
                else if (satoriMentionDic.ContainsKey(entity))
                {

                    try
                    {
                        dic = dbpediaToSatoriDic[array[1]];
                    }
                    catch (Exception)
                    {
                        dic = new Dictionary<string, int>();
                        dbpediaToSatoriDic[array[1]] = dic;
                    }
                    try
                    {
                        dic[satoriMentionDic[entity]] += 1;
                    }
                    catch (Exception)
                    {
                        dic[satoriMentionDic[entity]] = 1;
                    }
                }
            }
            dbpediaReader.Close();
            foreach (var item in dbpediaToSatoriDic)
            {
                foreach (var d in item.Value)
                {
                    writer.WriteLine(item.Key + "\t" + d.Key + "\t" + d.Value);
                }
            }
            writer.Close();

        }

        public static void Temp3()
        {
            var source = @"D:\Data\DBpedia\redirects.ttl";
            var reader = new pml.file.reader.LargeFileReader(source);
            var des = @"D:\Data\DBpedia\redirects.txt";
            var writer = new LargeFileWriter(des, FileMode.Create);
            string line;
            System.Text.RegularExpressions.Regex firstRegex = new System.Text.RegularExpressions.Regex(@"/([^>/]+)>\s<");
            System.Text.RegularExpressions.Regex secondRegex = new System.Text.RegularExpressions.Regex(@"/(\w+)>\s\.$");
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                string first = null;
                string second = null;

                if (firstRegex.IsMatch(line))
                {
                    var match = firstRegex.Match(line);
                    first = match.Groups[1].Value;
                }
                if (secondRegex.IsMatch(line))
                {
                    var match = secondRegex.Match(line);
                    second = match.Groups[1].Value;
                }
                if (first != null && second != null)
                {
                    if (++count % 10000 == 0)
                    {
                        Console.WriteLine(count);
                    }
                    writer.WriteLine(first + "\t" + second);
                }
            }
            reader.Close();
            writer.Close();

        }
        public static void Temp4()
        {
            var source = @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\dbpedia entity type.txt";
            var reader = new pml.file.reader.LargeFileReader(source);
            var des = @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\tmp.txt";
            var writer = new LargeFileWriter(des, FileMode.Create);
            string line;
            int count = 0;
            var set = new HashSet<string>();
            var dic = new Dictionary<string, int>();
            var times = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if(++count%10000 == 0)
                {
                    Console.WriteLine(count);
                }
                var array = line.Split('\t');
                dic.TryGetValue(array[1], out times);
                dic[array[1]] = times + 1;
            }
            reader.Close();
            foreach (var type in dic.OrderByDescending(key => key.Value))
            {
                writer.WriteLine(type.Key+"\t"+type.Value);
            }
            writer.Close();
        }

        public static void Temp5()
        {
            var sourceDir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\test";
            var sourceFiles = Directory.GetFiles(sourceDir).ToList();
            var desFile = @"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\test data in dbpedia info.txt";
            var writer = new LargeFileWriter(desFile, FileMode.Create);

            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var reader = new EventReaderByLine(sourceFiles[i]);
                int count = 0;
                int total = 0;
                while (reader.HasNext())
                {
                    total++;
                    var event1 = reader.GetNextEvent();
                    var rawFeature = event1.Feature.ToList();
                    if(!rawFeature[(int)Event.Field.dbpediaTypes].Equals("UNKNOW"))
                    {
                        count++;
                    }
                }
                reader.Close();
                writer.WriteLine(Path.GetFileNameWithoutExtension(sourceFiles[i]) + "\t" + count+"\t"+(1.0*count/total));
            }
            writer.Close();
        }

        public static void Temp6()
        {
            var sourceFile = @"D:\Codes\Project\EntityTyping\Fine-ner\output\conll feature\raw\train.txt";
            var desFile = @"D:\Codes\Project\EntityTyping\Fine-ner\output\conll feature\raw\train data in dbpedia info.txt";
            var writer = new LargeFileWriter(desFile, FileMode.Create);
            var coverNumByType = new Dictionary<string, int>();
            var totals = new Dictionary<string, int>();

            var reader = new EventReaderByLine(sourceFile);
            while (reader.HasNext())
            {

                var event1 = reader.GetNextEvent();
                var rawFeature = event1.Feature.ToList();
                try
                {
                    totals[event1.Label.ToString()] += 1;
                }
                catch(Exception)
                {
                    totals[event1.Label.ToString()] = 1;
                }
                if (!rawFeature[(int)Event.Field.dbpediaTypes].Equals("UNKNOW"))
                {
                    try
                    {
                        coverNumByType[event1.Label.ToString()] += 1;
                    }
                    catch(Exception)
                    {
                        coverNumByType[event1.Label.ToString()] = 1;
                    }
                }
            }
            reader.Close();
            foreach (var type in totals.Keys)
            {
                writer.WriteLine(type + "\t" + coverNumByType[type] + "\t" + totals[type] + "\t" + (1.0 * coverNumByType[type] / totals[type]));
            }
            writer.Close();
        }


        public static void Main(string[] args)
        {
            Temp6();
            //var currentFolderPath = Environment.CurrentDirectory;
            //var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
            //var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
            //basedir = Path.Combine(basedir, "Fine-ner/");
            //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\svm\train\"), Path.Combine(basedir, @"output\svm\train.txt"));
            //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\svm\develop\"), Path.Combine(basedir, @"output\svm\develop.txt"));
            //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\svm\test\"), Path.Combine(basedir, @"output\svm\test.txt"));

            //var sourceDir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\develop2";
            //var sourceDir2 = @"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\develop3";
            //var desDir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\develop4";
            //var sourceFiles = Directory.GetFiles(sourceDir).ToList();
            //var sourceFiles2 = Directory.GetFiles(sourceDir2).ToList();
            //var desFiles = new List<string>();
            //foreach (var file in sourceFiles)
            //{
            //    desFiles.Add(Path.Combine(desDir, Path.GetFileName(file)));
            //}

            //for (var i = 0; i < sourceFiles.Count; i++)
            //{
            //    var reader = new EventReaderByLine(sourceFiles[i]);
            //    var reader2 = new EventReaderByLine(sourceFiles2[i]);
            //    var writer = new EventWriterByLine(desFiles[i]);

            //    while (reader.HasNext())
            //    {
            //        var event1 = reader.GetNextEvent();
            //        var event2 = reader2.GetNextEvent();
            //        var rawFeature = event1.Feature.ToList();
            //        rawFeature[(int)Event.Field.opennlpNerType] = event2.Feature.ElementAt((int)Event.Field.opennlpNerType);
            //        writer.WriteEvent(new Event(event1.Label, rawFeature));
            //    }
            //    writer.Close();
            //    reader.Close();
            //    reader2.Close();
            //}

        }

        public static void GetItemNumByType()
        {
            var sourceDir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\test";
            var desFile = @"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\data info.txt";
            var sourceFiles = Directory.GetFiles(sourceDir).ToList();
            var writer = new LargeFileWriter(desFile, FileMode.Append);
            writer.WriteLine(sourceDir.Substring(sourceDir.LastIndexOf("\\")+1)+":");
            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var reader = new LargeFileReader(sourceFiles[i]);
                var line = "";
                int count = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    count++;
                }
                reader.Close();
                writer.WriteLine(Path.GetFileNameWithoutExtension(sourceFiles[i]) + "\t:\t" + count);
            }
            writer.Close();
        }

        public static void EvaluateResult()
        {
            var evaluator = new ClassByClassEvaluator();
            var sourceDir = @"D:\Codes\Project\EntityTyping\Fine-ner\output\result\satori\eval\instance result\inst result with stanford ner\";
            var desDir = @"D:\Codes\Project\EntityTyping\Fine-ner\output\result\satori\eval\statistic result\with stanford ner\";
            var sourceFiles = Directory.GetFiles(sourceDir).ToList();
            var desFile = "";
            foreach (var file in sourceFiles)
            {
                desFile = Path.Combine(desDir, Path.GetFileName(file));
                evaluator.EvaluateResult(file, desFile);
            }
         
        }


        /// <summary>
        /// pair.first: word
        /// pair.second: pos tag of word
        /// </summary>
        /// <param name="pairs"></param>
        public pml.type.Pair<string,string> GetMentionHead(List<pml.type.Pair<string, string>> pairs)
        {
            string head = null, posTag = null;
            for (int i = 0; i <= pairs.Count; i++)
            {
                if (pairs.ElementAt(i).second.StartsWith("N"))
                {
                    // last noun
                    head = pairs.ElementAt(i).first;
                    posTag = pairs.ElementAt(i).second;
                }
                else if (pairs.ElementAt(i).second.Equals("IN") || pairs.ElementAt(i).second.Equals(","))
                {
                    // before IN
                    break;
                }
            }
            if(head == null)
            {
                head = pairs[pairs.Count - 1].first;
                posTag = pairs[pairs.Count - 1].second;
            }
            return new pml.type.Pair<string, string>(head, posTag);
        }

    }
}
