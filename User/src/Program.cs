using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using pml.file.writer;
using System.IO;
using pml.type;
using msra.nlp.tr;

namespace User
{
    class Program
    {
        public static void Mains(string[] args)
        {
            //Script.ExtractCoNLL();
            //Temp();
            Analyse();
            //Start();
            //Statistic.Refresh();  // test tfs
        }

        public static void Analyse()
        {
            // String basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input";
            String reportFile = @" E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\report.txt";
            FileWriter writer = new LargeFileWriter(reportFile, FileMode.Append);
            //// statistic train data number by type and coverage of UIUC by type
            //String dicCovReport = Statistic.StatisticDicCoverage(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\UIUC.txt", basedir + @"\train\train.txt");
            //writer.WriteLine("Coverage of UIUC within train data:\r"+dicCovReport);
            //writer.WriteLine("");
            //basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori";
            //// statistic satori develop data number by type and coverage of UIUC by type
            //dicCovReport = Statistic.StatisticDicCoverage(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\UIUC.txt", basedir + @"\develop.txt");
            //writer.WriteLine("Coverage of UIUC within satori develop data:\r" + dicCovReport);
            //writer.WriteLine("");
            //// statistic satori test data number by type and coverage of UIUC by type
            //dicCovReport = Statistic.StatisticDicCoverage(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\UIUC.txt", basedir + @"\test.txt");
            //writer.WriteLine("Coverage of UIUC within satori test data:\r" + dicCovReport);
            //writer.WriteLine("");
            //basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori_lc";
            //// statistic satori_lc develop data number by type and coverage of UIUC by type
            //dicCovReport = Statistic.StatisticDicCoverage(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\UIUC.txt", basedir + @"\develop.txt");
            //writer.WriteLine("Coverage of UIUC within satori_lc develop data:\r" + dicCovReport);
            //writer.WriteLine("");
            //// statistic satori_lc test data number by type and coverage of UIUC by type
            //dicCovReport = Statistic.StatisticDicCoverage(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\UIUC.txt", basedir + @"\test.txt");
            //writer.WriteLine("Coverage of UIUC within satori_lc test data:\r" + dicCovReport);
            //writer.WriteLine("");

            string basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input";
            // // statistic co-occurrence rate between train and satori develop data
            //string corate = Statistic.StatisticCooccurrence(basedir + @"\train\train.txt", basedir + "/satori/develop.txt");
            //writer.WriteLine("Co-occurrence rate between train and satori develop data is:\r "+corate);
            //writer.WriteLine("");
            //// statistic co-occurrence rate between train and satori test data
            //corate = Statistic.StatisticCooccurrence(basedir + @"\train\train.txt", basedir + "/satori/test.txt");
            //writer.WriteLine("Co-occurrence rate between train and satori test data is:\r " + corate);
            //writer.WriteLine("");
            //// statistic co-occurrence rate between train and satori_lc develop data
            //corate = Statistic.StatisticCooccurrence(basedir + @"\train\train.txt", basedir + "/satori_lc/develop.txt");
            //writer.WriteLine("Co-occurrence rate between train and satori_lc develop data is:\r " + corate);
            //writer.WriteLine("");
            //// statistic co-occurrence rate between train and satori_lc test data
            //corate = Statistic.StatisticCooccurrence(basedir + @"\train\train.txt", basedir + "/satori_lc/test.txt");
            //writer.WriteLine("co-occurrence rate between train and satori_lc test data is:\r " + corate);
             //statistic name list coverage
            //String report = Statistic.StatisticNameListCoverageByType(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\name-list.txt", @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\train\limited train.txt");
            //writer.WriteLine("Name list coverage by type is :\r " + report);
            // statistic item number by type
            //writer.WriteLine("Item number by type:\r" + Statistic.StatisticItemNumberByType(basedir + @"\train\train.txt"));
            writer.WriteLine(Statistic.StatisticRoundTokenInformation(basedir + @"\train\train.txt"));
            //writer.WriteLine(Statistic.StatisticWithinTokenInfomation(basedir + @"\train\train.txt"));
            writer.Close();                                         
        }

        public static void Start()
        {
            Property props = new Property();
            String basedir = null ;
            Pipeline pipeline = null;
            /************************************************************************/
            /* Feature extractor                                                                     */
            /************************************************************************/
            if (false)
            {
                //extract conll feature
                props.SetProperty("method", @"/ef -all");
                basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\CoNLL\";
                props.SetProperty("train_data_file", basedir + "train.txt");
                props.SetProperty("develop_data_file", basedir + "develop.txt");
                props.SetProperty("test_data_file", basedir + "test.txt");
                basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\conll feature\";
                props.SetProperty("train_feature_file", basedir + "trainFeature.txt");
                props.SetProperty("develop_feature_file", basedir + "developFeature.txt");
                props.SetProperty("test_feature_file", basedir + "testFeature.txt");
                pipeline = new Pipeline(props);
                pipeline.Execute();
            }
            if (false)
            {
                //extract satori train - develop data feature
                props.SetProperty("method", @"/ef -all");
                basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori\";
                props.SetProperty("train_data_file", @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\train\train.txt");
                props.SetProperty("develop_data_file", basedir + "develop.txt");
                props.SetProperty("test_data_file", basedir + "test.txt");
                basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\satori feature\";
                props.SetProperty("train_feature_file", @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\train feature\trainFeature.txt");
                props.SetProperty("develop_feature_file", basedir + "developFeature.txt");
                props.SetProperty("test_feature_file", basedir + "testFeature.txt");
                pipeline = new Pipeline(props);
                pipeline.Execute();
            }
            if(false)
            { 
                ////extract limited satori train-develop data feature, without dictionary information
                basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori_lc\";
                props.SetProperty("develop_data_file", basedir + "develop.txt");
                props.SetProperty("test_data_file", basedir + "test.txt");
                basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\satori_lc feature\";
                props.SetProperty("develop_feature_file", basedir + "developFeature.txt");
                props.SetProperty("test_feature_file", basedir + "testFeature.txt");
                props.SetProperty("method", @"/ef -b -test -dev");
                pipeline = new Pipeline(props);
                pipeline.SetProperty(props);
                pipeline.Execute();
            }
            /************************************************************************/
            /* Bayes train and test                                                                     */
            /************************************************************************/
            if (false)
            {
                // train
            }
            if(true)
             {
                // test
                 basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\satori_lc feature\";
                 props.SetProperty("method", @"/ts -b");
                 props.SetProperty("train_feature_file", @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\train feature\"+"trainFeature.txt");
                 props.SetProperty("develop_feature_file", basedir + "developFeature.txt");
                 props.SetProperty("test_feature_file", basedir + "testFeature.txt");
                 props.SetProperty("result_file", @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\result\satori_lc\result.txt");
                 pipeline = new Pipeline(props);
                 pipeline.Execute();
             }

        }

        public static void ExtractUIUC()
        {
            string source = @"E:\Users\v-mipeng\Data\Dictionary\name-list.freq.txt";
            FileReader reader = new LargeFileReader(source);
            string des = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\data\name-list.txt";
            FileWriter writer = new LargeFileWriter(des, FileMode.OpenOrCreate);
            String line;
            String[] array;

            while ((line = reader.ReadLine()) != null)
            {
                array = line.Split('\t');
                writer.WriteLine(array[0]);
            }
        }

        public static void Temp()
        {
            if (false)
            {
                FileReader reader = new LargeFileReader(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\train\train.txt");
                FileWriter writer = new LargeFileWriter(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\train\limited train.txt", FileMode.OpenOrCreate);
                Dictionary<string, int> numByType = new Dictionary<string, int>(16);
                String line;
                String[] array;
                int count = 0;
                int num = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    count++;
                    if (count % 1000 == 0)
                    {
                        Console.Error.WriteLine(count + " items processed!");
                    }
                    array = line.Split('\t');
                    try
                    {
                        num = numByType[array[1]];
                    }
                    catch (Exception)
                    {
                        num = 0;
                    }
                    if (num > 100000)   // do not limit train data number by type
                    {
                        continue;
                    }
                    writer.WriteLine(line);
                    numByType[array[1]] = ++num;
                }
                reader.Close();
                writer.Close();
            }

            if (false)
            {
                string result = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\result\satori_lc\-1.inst.txt";
                string source = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori_lc\develop.txt";
                String tmpFile = "./tmp.txt";
                FileReader reader1 = new LargeFileReader(result);
                FileReader reader2 = new LargeFileReader(source);
                FileWriter writer = new LargeFileWriter(tmpFile, FileMode.OpenOrCreate);
                String line;
                String line2;


                writer.WriteLine(reader1.ReadLine());

                while ((line = reader1.ReadLine()) != null)
                {
                    line2 = reader2.ReadLine();
                    writer.WriteLine(line2.Split('\t')[0] + "\t" + line.Split(new char[] { '\t' }, 2)[1]);
                }
                reader1.Close();
                reader2.Close();
                writer.Close();
                File.Copy(tmpFile, @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\result\satori_lc\.inst.txt");
                File.Delete(tmpFile);
            }
            if (false)
            {
                string wordTableFile = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\word table\wordTable.txt";
                FileReader reader = new LargeFileReader(wordTableFile);
                FileWriter writer = new LargeFileWriter();
                HashSet<string> wordSet = new HashSet<string>();
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    wordSet.Add(Stemmer.Stem(line.Split('\t')[0])[0]);
                }
                reader.Close();
                writer.Open(wordTableFile);
                int i = 0;
                foreach (String word in wordSet)
                {
                    writer.WriteLine(word + '\t' + (i++));
                }
                writer.Close();
            }
            if(false)
            {
                String dir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\names";
                string des = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\name-all.txt";
                FileReader reader = new LargeFileReader();
                FileWriter writer = new LargeFileWriter(des, FileMode.Create);
               string[] files =  Directory.GetFiles(dir, "*.txt");
               string line;

               foreach (String file in files)
               {
                   reader.Open(file);
                   while ((line = reader.ReadLine()) != null)
                   {
                       writer.WriteLine(line.Split(',')[0]);
                   }
               }
               reader.Close();
               writer.Close();
            }
            if (false)
            {
                string path1 = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\name-all.txt";
                string path2 = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\train\limited train.txt";
                string des = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\tmp.txt";
                FileReader reader = new LargeFileReader(path1);
                FileWriter writer = new LargeFileWriter(des);
                String line;
                HashSet<String> set = new HashSet<string>();
                String[] array;

                while ((line = reader.ReadLine()) != null)
                {
                    set.Add(line);
                    array = line.Split(' ');
                }
                reader.Close();
                reader.Open(path2);

                while ((line = reader.ReadLine()) != null)
                {
                    array = line.Split('\t');

                    if (set.Contains(array[0].ToLower()))
                    {
                        if (!array[1].Equals("people.person"))
                        {
                            set.Remove(array[0].ToLower());
                        }
                    }

                }
                reader.Close();
                foreach(String name in set)
                {
                    writer.WriteLine(name);
                }
                writer.Close();
            }
            if(false)
            {
                FileReader reader  = new LargeFileReader(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori_lc\backup\version 1-2\develop.txt");
                FileWriter writer = new LargeFileWriter(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori_lc\develop.txt", FileMode.OpenOrCreate);
                String line;
                string[] array;
                HashSet<string> interestTypes = new HashSet<string>();
                interestTypes.Add("people.person");
                interestTypes.Add("location.location");
                interestTypes.Add("organization.organization");
                while((line = reader.ReadLine())!=null)
                {
                    array = line.Split('\t');
                    if(interestTypes.Contains(array[1]))
                    {
                        writer.WriteLine(line);
                    }
                }
                reader.Close();
                writer.Close();
            }
            if(false)
            {
                FileReader reader = new LargeFileReader(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\tmp.txt");
                FileWriter writer = new LargeFileWriter(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\tmp2.txt",FileMode.Create);
                String line;
                string[] array;
                string[] pairString;
                List<Pair<string, int>> list = new List<Pair<string, int>>();
                Pair<string, int> pair = new Pair<string, int>();
                Comparer<Pair<string,int>> comparer = pair.GetBySecondReverseComparer();

                while((line = reader.ReadLine())!=null)
                {
                    array = line.Split('\t');
                     for(int i=1;i<array.Length;i++)
                     {
                         pairString = new string[] { array[i].Substring(0, array[i].LastIndexOf(":")), array[i].Substring(array[i].LastIndexOf(":") + 1) };
                         pair = new Pair<string, int>();
                         pair.first = pairString[0];
                         pair.second = int.Parse(pairString[1]);
                         list.Add(pair);
                     }
                     list.Sort(comparer);
                     foreach (Pair<string, int> item in list)
                     {
                         writer.Write("\t" + item.first + ":" + item.second);
                     }
                     writer.Write("\r");
                     list.Clear();
                }
                reader.Close();
                writer.Close();

            }
            if(true)
            {
                FileReader reader = new LargeFileReader(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\tmp.txt");
                FileWriter writer = new LargeFileWriter(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\tmp2.txt", FileMode.Create);
                String line;
                string[] lines = new string[3];
                string[] array;
                string[] pairString;
                Dictionary<string,int>[] dics = new Dictionary<string,int>[3];
                List<Pair<string, int>> list = new List<Pair<string, int>>();
                Pair<string, int> pair = new Pair<string, int>();
                Comparer<Pair<string, int>> comparer = pair.GetBySecondReverseComparer();

               for(int i = 0;i<3;i++)
                {
                   line = reader.ReadLine();
                    array = line.Split('\t');
                    dics[i] = new Dictionary<string, int>();
                    if (i == 0)
                    {
                        for (int j = 1; j < array.Length; j++)
                        {
                            pairString = new string[] { array[j].Substring(0, array[j].LastIndexOf(":")), array[j].Substring(array[j].LastIndexOf(":") + 1) };
                            pair = new Pair<string, int>();
                            pair.first = pairString[0];
                            pair.second = int.Parse(pairString[1]);
                            dics[i][pair.first] = pair.second;
                            list.Add(pair);
                        }
                    }
                    else
                    {
                        for (int j = 1; j < array.Length; j++)
                        {
                            pairString = new string[] { array[j].Substring(0, array[j].LastIndexOf(":")), array[j].Substring(array[j].LastIndexOf(":") + 1) };
                            dics[i][pairString[0]] = int.Parse(pairString[1]);
                        }
                    }
               }
                    list.Sort(comparer);
                    int count = 10;
                    int locNum;
                    int orgNum;
                    foreach (Pair<string, int> item in list)
                    {
                        count++;
                        try
                        {
                            locNum = dics[1][item.first];
                        }
                        catch(Exception)
                        {
                            locNum = 0;
                        }
                        try
                        {
                            orgNum = dics[2][item.first];
                        }catch(Exception )
                        {
                            orgNum = 0;
                        }
                        writer.Write("\t" + item.first + ":(" + item.second+"|"+locNum+"|"+orgNum+")");
                        if(count %5 ==0)
                        {
                            writer.Write("\r");
                        }
                    }
                    reader.Close();
                    writer.Close();
            }
        }
    }
}
