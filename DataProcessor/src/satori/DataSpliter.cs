using pml.file.reader;
using pml.file.writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;

namespace msra.nlp.tr.dp.satori
{
    /// <summary>
    ///   Split data into training data, develop data and test data.
    /// </summary>
    class DataSpliter
    {
  
        //  refined satori directory. Files seperated by type
        readonly string sourceDir;
        // file storing the number by type information
        readonly string sourceFileInfoFile;
        // directory to store the training data by type
        readonly string trainDir;
        // directory to store the develop data by type
        readonly string developDir;
        // directory to store the test data by type
        readonly string testDir;
        // file to store the training, develop and test data information
        string statisticInfoFile = "./statisticInfo.txt"; // The information should record mention and unique mention number in train data by type

        public DataSpliter(string sourceDir, string sourceFileInfoFile, string trainDir, string developDir, string testDir, string statisticInfoFile = null)
        {
            this.sourceDir = sourceDir;
            this.sourceFileInfoFile = sourceFileInfoFile;
            this.trainDir = trainDir;
            if(!Directory.Exists(trainDir))
            {
                Directory.CreateDirectory(trainDir);
            }
            this.developDir = developDir;
            if (!Directory.Exists(developDir))
            {
                Directory.CreateDirectory(developDir);
            }
            this.testDir = testDir;
            if (!Directory.Exists(testDir))
            {
                Directory.CreateDirectory(testDir);
            }
            if (statisticInfoFile != null)
            {
                this.statisticInfoFile = statisticInfoFile;
            }
        }

        public void SplitData()
        {
           var  sourceDic = LoadTotalNumByType();
            var mentionNumDic = new Dictionary<string, int>();
            var uniqueMentionNumDic = new Dictionary<string, HashSet<string>>();
            // create reader by file
            var files = Directory.GetFiles(this.sourceDir);
           var reader = new LargeFileReader();
            // create file path to store train, develop and test data
            var trainFiles = new List<string>();
            var devFiles = new List<string>();
            var testFiles = new List<string>();
            foreach (var file in files)
            {
                trainFiles.Add(Path.Combine(trainDir, Path.GetFileName(file)));
                devFiles.Add(Path.Combine(developDir, Path.GetFileName(file)));
                testFiles.Add(Path.Combine(testDir, Path.GetFileName(file)));
            }
            var writers = new List<FileWriter>();
            // random value generator to seperate develop and test data
            var random = new Random();
            string line;
            string[] array;
            int num = 0;
            int trainNumLimit = 500000;
            HashSet<string> set = null;
            int count = 0;
            int limitMentionNumPerEntity = 10;
            int numByEntity = 0;
            int devNumLimit = 4000;
            int i = 0;
            files = new string[] { @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\refined-satori\time_event.txt" };
            trainFiles.Clear();
            trainFiles.Add(@"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\train\time_event.txt");
            devFiles.Clear();
            devFiles.Add(@"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\develop\time_event.txt");
            testFiles.Clear();
            testFiles.Add(@"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\test\time_event.txt" );

            foreach (var file in files)
            {
                reader.Open(file);
                string lastEntity = "";
                writers.Clear();
                writers.Add(new LargeFileWriter(devFiles[i], FileMode.Create));
                writers.Add(new LargeFileWriter(testFiles[i], FileMode.Create));
                var trainWriter = new LargeFileWriter(trainFiles[i], FileMode.Create);
                i++;
                int devOrTestNum = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    if (++count % 10000 == 0)
                    {
                        Console.WriteLine(count);
                    }
                    array = line.Split('\t');
                    if (array[1].Equals(lastEntity))
                    {
                        numByEntity++;
                    }
                    else
                    {
                        numByEntity = 1;
                        lastEntity = array[1];
                    }
                    if (numByEntity > limitMentionNumPerEntity)
                    {
                        continue;
                    }
                    mentionNumDic.TryGetValue(array[2], out num);
                    if (num < trainNumLimit && num < 0.8 * sourceDic[array[2]]/ limitMentionNumPerEntity)
                    {
                        SaveForTrain(trainWriter, line);
                        mentionNumDic[array[2]] = num + 1;
                        uniqueMentionNumDic.TryGetValue(array[2], out set);
                        if (set == null)
                        {
                            set = uniqueMentionNumDic[array[2]] = new HashSet<string>();
                        }
                        if (!set.Contains(array[0]))
                        {
                            set.Add(array[0]);
                        }
                    }
                    else   if(devOrTestNum < devNumLimit*2)
                    {
                        devOrTestNum++;
                        SaveForDevOrTest(writers[random.Next(0,2)], line);
                    }
                }
                reader.Close();
                trainWriter.Close();
                writers[0].Close();
                writers[1].Close();
            }
            var writer = new LargeFileWriter(statisticInfoFile, FileMode.Create);
            foreach(var key in mentionNumDic.Keys)
            {
                writer.WriteLine(key + "\t" + mentionNumDic[key]);
                writer.WriteLine(key + "\t" + uniqueMentionNumDic[key].Count);
            }
            writer.Close();
            foreach(var file in trainFiles)
            {
                File.SetAttributes(file, FileAttributes.ReadOnly);
            }
            foreach (var file in devFiles)
            {
                File.SetAttributes(file, FileAttributes.ReadOnly);
            }
            foreach (var file in testFiles )
            {
                File.SetAttributes(file, FileAttributes.ReadOnly);
            }
        }

       


        string line;
        string[] array;
        FileReader reader = null;
        private void ReadOneItem()
        {
            //if (reader == null)
            //{
            //    reader = new LargeFileReader();
            //}
            //this.line = reader.ReadLine();
            //array = line.Split('\t');
        }

        private Dictionary<string, int> LoadTotalNumByType()
        {
            var dic = new Dictionary<string, int>();
            var reader = new LargeFileReader(this.sourceFileInfoFile);
            string line;

            while ((line =reader.ReadLine())!=null)
            {
                var array = line.Split('\t');
                dic[array[0]] = int.Parse(array[1]);
            }
            reader.Close();
            return dic;
        }

        private void SaveForTrain(FileWriter trainWriter, string line)
        {
            trainWriter.WriteLine(line);
        }

        private void SaveForDevOrTest(FileWriter devWriter, string line)
        {
            devWriter.WriteLine(line);
        }


        ~DataSpliter()
        {
        }

        public static void Mains(string[] args)
        {
            DataSpliter spliter = new DataSpliter(
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\refined-satori\",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\refined-satori\statisticInfo.txt",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\train\",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\develop\",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\test\",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\train\train-data-info.txt");
            spliter.SplitData();
        }

    }
}
