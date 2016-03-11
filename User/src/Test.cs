//#define debug

using pml.file.reader;
using pml.file.writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using msra.nlp.tr.eval;
using User.src;


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
                    if(!rawFeature[(int)Event.Field.dbpediaTypesWithAbstract].Equals("UNKNOW"))
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
                if (!rawFeature[(int)Event.Field.dbpediaTypesWithAbstract].Equals("UNKNOW"))
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

        public static void Temp7()
        {
            var source = @"D:\Data\DBpedia\short-abstracts_en.nt";
            var abst = @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract.txt";
            var line = "";
            var reader = new LargeFileReader(source);
            var writer = new LargeFileWriter(abst, FileMode.Create);
            reader.ReadLine();
            System.Text.RegularExpressions.Regex regex1 = new System.Text.RegularExpressions.Regex(@"([^/]+)>");
            System.Text.RegularExpressions.Regex regex2 = new System.Text.RegularExpressions.Regex("\"(.+)\"@en .");
            System.Text.RegularExpressions.Regex regex3 = new System.Text.RegularExpressions.Regex(@"\\u\w{4}");
            int count = 0;

            while((line = reader.ReadLine())!=null)
            {
                if(count++%1000==0)
                {
                    Console.WriteLine(count);
                }
                try
                {
                    var title = regex1.Match(line).Groups[1].Value;
                    var abs = regex2.Match(line).Groups[1].Value;
                    abs = regex3.Replace(abs, " ");
                    writer.WriteLine(title + "\t" + abs);
                }
                catch(Exception)
                {
                    continue;
                }
            }
            reader.Close();
            writer.Close();
        }

        public static void Temp8()
        {
            var source = @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract vector.txt";
            var des = @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract vector trimed.txt";
            var reader = new LargeFileReader(source);
            var writer = new LargeFileWriter(des, FileMode.Create);
            string line;
            while((line = reader.ReadLine())!=null)
            {
                var array = line.Split(new char[] { '\t' }, 2);
                if(array[0].Contains("_("))
                {
                    writer.WriteLine(line);
                }
            }
            reader.Close();
            writer.Close();
        }

        public static void Temp9(string interestWordFile, string word2vecFile, string selectedWord2VectorFile)
        {
            var reader = new LargeFileReader(interestWordFile);
            string line;
            var set = new HashSet<string>();

            while ((line = reader.ReadLine()) != null)
            {
                set.Add(line.Trim());
            }
            reader.Close();
            var writer = new LargeFileWriter(selectedWord2VectorFile, FileMode.Create);
            int count = 0;
            reader.Open(word2vecFile);
            while ((line =reader.ReadLine())!=null)
            {
                if (++count % 1000 == 0)
                {
                    Console.WriteLine(count);
                }
                var array = line.Split(new char[]{' '},2);
                if(set.Contains(array[0]))
                {
                    writer.WriteLine(line);
                }
            }
            writer.Close();
        }
        public static void Temp10(string vectorFile, string centroidInfoFile, string wordClusterIDFile)
        {
            var cluster = new VectorCluster(vectorFile, centroidInfoFile, wordClusterIDFile);
            cluster.Cluster(1000);
        }  
        
        public static void Temp11()
        {
            var dir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\test";
            var writer = new LargeFileWriter(@"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\test.txt", FileMode.Create);
            foreach(var file in Directory.GetFiles(dir))
            {
                var text = File.ReadAllText(file);
                writer.Write(text);
            }
            writer.Close();
        }

        public static void Temp12()
        {
            var testDataFile = @"D:\Codes\Project\EntityTyping\Fine-ner\output\svm\test.txt";
            var filterFile = @"D:\Codes\Project\EntityTyping\Fine-ner\output\result\satori\base ners dbpedia-abstract\1.inst.txt";
            var desFile = @"D:\Codes\Project\EntityTyping\Fine-ner\output\svm\temp test.txt";
            var traResultFile = @"D:\Codes\Project\EntityTyping\Fine-ner\output\svm\result of traditional types.txt";
            //var traditionalTypeFile;
            string line;
            var reader = new LargeFileReader(filterFile);
            var writer = new LargeFileWriter(desFile, FileMode.Create);
            var traResultWriter = new LargeFileWriter(traResultFile, FileMode.Create);
            var set = new HashSet<int>();

            while((line = reader.ReadLine())!=null)
            {
                var array = line.Split(new char[]{'\t'});
                if (array[2].Equals("9") || array[2].Equals("10") || array[2].Equals("11"))
                {
                    traResultWriter.WriteLine(line);
                }
                else
                {
                    set.Add(int.Parse(array[0]));
                }
            }
            traResultWriter.Close();
            reader.Close();
            reader.Open(testDataFile);
            int count = 0;
            while((line = reader.ReadLine())!=null)
            {
                if (set.Contains(count))
                {
                    writer.WriteLine(line);
                }
                count++;
            }
            reader.Close();
            writer.Close();
        }

        public static void Temp13()
        {
            var lineListFile = @"D:\Codes\Project\EntityTyping\Fine-ner\output\result\conll\base ners dbpedia-abstract keyword\line list.txt";
            var featureFile = @"D:\Codes\Project\EntityTyping\Fine-ner\output\result\conll\base ners dbpedia-abstract keyword\features.txt";
            var documentFile = @"D:\Codes\Project\EntityTyping\Fine-ner\output\result\conll\base ners dbpedia-abstract keyword\documents.txt";
            var sourceDocumentFile = @"D:\Codes\Project\EntityTyping\Fine-ner\input\conll\develop.txt";
            var sourceFeatureFile = @"D:\Codes\Project\EntityTyping\Fine-ner\output\conll feature\raw\develop.txt";
            var docReader = new LargeFileReader(sourceDocumentFile);
            var featureReader = new LargeFileReader(sourceFeatureFile);
            var lineReader = new LargeFileReader(lineListFile);
            var docWriter = new LargeFileWriter(documentFile, FileMode.Create);
            var featureWriter = new LargeFileWriter(featureFile, FileMode.Create);
            var set = new HashSet<int>();
            string line;
            int count = 0;
            while((line = lineReader.ReadLine())!=null)
            {
                if (count < 2)
                {
                    set.Add(int.Parse(line));
                }
                else if(count<3)
                {
                    set.Add(int.Parse(line)+1);
                }
                else
                {
                    set.Add(int.Parse(line) + 2);
                }
                count++;
            }
            lineReader.Close();
            count = 0;
            while((line = docReader.ReadLine())!=null)
            {
                if (set.Contains(count))
                {
                    docWriter.WriteLine(line);
                }
                count++;
            }
            docReader.Close();
            docWriter.Close();
            //count = 0;
            //while ((line = featureReader.ReadLine()) != null)
            //{
            //    if (set.Contains(count))
            //    {
            //        featureWriter.WriteLine(line);
            //    }
            //    count++;
            //}
            //featureReader.Close();
            //featureWriter.Close();
        }

        public static void Temp14()
        {
            var pipline = new Pipeline();
           var types =  DataCenter.GetDBpediaType("london");
        }

        public static void Temp15()
        {
            var source = @"D:\Data\Wikipedia\enwiki-20150602-iwlinks.sql";
            var reader = new LargeFileReader(source);
            var des = @"D:\Data\Wikipedia\pagelinks-extracted.txt";
            var writer = new LargeFileWriter(des, FileMode.Create);
            string line;
            var indegree = new Dictionary<string, int>();
            var regex = new System.Text.RegularExpressions.Regex(@"\),\(");
            var count = 0;
            var buffer = new StringBuilder();
            var commentRegex = new System.Text.RegularExpressions.Regex(@"\\\*.+?\\\*");

            while((line = reader.ReadLine())!=null)
            {
                if (!line.StartsWith("INSERT"))
                {
                    buffer.Append(line);
                    if (!reader.reachFileEnd)
                    {
                        continue;
                    }
                }
                if (buffer.Length == 0)
                {
                    buffer.Append(line);
                    continue;
                }
                else
                {
                    var temp = line;
                    line = buffer.ToString();
                    buffer.Clear();
                    buffer.Append(temp);
                }
                Console.WriteLine(++count);
                line = line.Replace("INSERT INTO `iwlinks` VALUES ", "");
                line = commentRegex.Replace(line, "");
                var array = regex.Split(line);
                array[array.Length-1] = array[array.Length-1].Replace(";","");
                for(var i = 0;i<array.Length;i++)
                {
                    var a = array[i].Split(',');
                    try
                    {
                        var entity = a[2].Substring(1, a[2].Length - 2);
                        try
                        {
                            indegree[entity] += 1;
                        }
                        catch (Exception)
                        {
                            indegree[entity] = 1;
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        Console.WriteLine(i);
                        Console.WriteLine(array.Length);
                        Console.WriteLine(string.Join("\t", array[i]));
                        //Console.ReadKey();
                    }
                }
            }
            reader.Close();
            foreach(var key in indegree.Keys)
            {
                writer.WriteLine(key + "\t" + indegree[key]);
            }
            writer.Close();
        }

        public static void Temp16()
        {
            var regex = new System.Text.RegularExpressions.Regex(@"\A[A-Za-z_'()]+\Z");
            var source = @"D:\Data\Wikipedia\pagelinks-extracted.txt";
            var reader = new LargeFileReader(source);
            var des = @"D:\Data\Wikipedia\pagelinks-extracted-filtered.txt";
            var writer = new LargeFileWriter(des, FileMode.Create);
            string line;

            while((line =reader.ReadLine())!=null)
            {
                var array = line.Split('\t');
               if(regex.IsMatch(array[0]))
               {
                   writer.WriteLine(line);
               }
               else
               {
                   continue;
               }
            }
            reader.Close();
            writer.Close();

        }

        public static void Temp17()
        {
            var source = @"D:\Codes\Project\EntityTyping\Fine-ner\output\conll feature\raw\test.txt";
            var reader = new LargeFileReader(source);
            string line;
            var count = 0;
            var total = 0;

            while((line = reader.ReadLine())!=null)
            {
                var array = line.Split('\t');
                total += 1;
                if(array[0].ToLower().Contains(array[(int)Event.Field.stanfordNerType+2].ToLower()))
                {
                    count += 1;
                }
            }
            reader.Close();
            Console.WriteLine(string.Format("Total number:{0}\nPositive number:{1}\nPrecision:{2}", total, count, 1.0 * count / total));
            Console.ReadKey();
        }

        public static void Temp18()
        {
            var source = @"D:\Temp\sina\sina_text.txt";
            var posFile = @"D:\Temp\sina\sina_text_pos.txt";
            var nerFile = @"D:\Temp\sina\sina_text_ner.txt";
            var reader = new LargeFileReader(source);
            var posWriter = new LargeFileWriter(posFile, FileMode.Create);
            var nerWriter = new LargeFileWriter(nerFile, FileMode.Create);
            var tagger = new PosTagger();
            var ner = new StanfordNer();
            var line = "";
            
            while((line = reader.ReadLine())!=null)
            {
                try
                {
                    var pairs = tagger.TagString(line);
                    foreach (var pair in pairs)
                    {
                        posWriter.Write(string.Format("{0}({1}) ", pair.first, pair.second));
                    }
                    posWriter.WriteLine("");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    posWriter.WriteLine("");
                }
                try
                {
                    ner.FindNer(line);
                    var pairs = ner.GetEntities();
                    foreach (var pair in pairs)
                    {
                        nerWriter.Write(string.Format("{0}({1}) ", pair.first, pair.second));
                    }
                    nerWriter.WriteLine("");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    nerWriter.WriteLine("");
                }
            }
            reader.Close();
            posWriter.Close();
            nerWriter.Close();
        }
        public static void Main(string[] args)
        {
            //pml.file.util.Util.CombineFiles(@"D:\Codes\Project\EntityTyping\Fine-ner\input\satori+conll", @"D:\Codes\Project\EntityTyping\Fine-ner\input\satori+conll\train.txt");
            Temp18();
            //var commentRegex = new System.Text.RegularExpressions.Regex(@"\\\*(.(?!\\\*))+\\\*");
            //string str = "I like \\*lskdjlfd\\*lskdl\\*";
            //str = commentRegex.Replace(str, "");
            //Console.WriteLine(str);
            //Temp17();
            //Temp12();
            //Temp10(@"D:\Data\Google-word2vec\GoogleNews-vectors-negative300-seleted.txt", @"D:\Data\Google-word2vec\KMeans on selected vectors\centroids-1000.txt", @"D:\Data\Google-word2vec\KMeans on selected vectors\cluster IDs-1000.txt");
            //var pipeline = new Pipeline();
            //TfIdf tfidf = new TfIdf(
            //   @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract.txt",
            //   @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract vector.txt",
            //   @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract df.txt",
            //   @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract word table.txt");
            //tfidf.GetVectorCorpus();
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
            var sourceDir = @"D:\Codes\Project\EntityTyping\Fine-ner\output\svm\inst.txt";
            var desDir = @"D:\Codes\Project\EntityTyping\Fine-ner\output\svm\result.txt";
            //var sourceFiles = Directory.GetFiles(sourceDir).ToList();
            //var desFile = "";
            //foreach (var file in sourceFiles)
            //{
            //    desFile = Path.Combine(desDir, Path.GetFileName(file));
            evaluator.EvaluateResult(sourceDir, desDir);
            //}
         
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
