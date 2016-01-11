using pml.file.reader;
using pml.file.writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr.dp
{
    class DataSpliter
    {
        readonly string sourceFile;
        readonly string sourceFileInfoFile;
        readonly string trainDataFile;
        readonly string devAndTestDataFile; // reserve the data for future useage
        string statisticInfoFile = "./statisticInfo.txt"; // The information should record mention and unique mention number in train data by type

        public DataSpliter(string sourceFile, string sourceFileInfoFile, string trainDataFile, string devAndTestDataFile, string statisticInfoFile = null)
        {
            this.sourceFile = sourceFile;
            this.sourceFileInfoFile = sourceFileInfoFile;
            this.trainDataFile = trainDataFile;
            this.devAndTestDataFile = devAndTestDataFile;
            if (statisticInfoFile != null)
            {
                this.statisticInfoFile = statisticInfoFile;
            }
        }

        public void SplitData()
        {
            var sourceDic = LoadTotalNumByType();
            var mentionNumDic = new Dictionary<string, int>();
            var uniqueMentionNumDic = new Dictionary<string, HashSet<string>>();
            string line;
            string[] array;
            reader = new LargeFileReader(sourceFile);
            int num = 0;
            int trainNumLimit = 500000;
            HashSet<string> set = null;
            int count = 0;
            string lastEntity = "";
            int limitMentionNumPerEntity = 10;
            int numByEntity = 0;

            while ((line = reader.ReadLine())!=null)
            {
                if(++count%10000==0)
                {
                    Console.WriteLine(count);
                }
                array = line.Split('\t');
                if(array[1].Equals(lastEntity))
                {
                    numByEntity++;
                }
                else
                {
                    numByEntity = 1;
                    lastEntity = array[1];
                }
                if(numByEntity > limitMentionNumPerEntity)
                {
                    continue;
                }
                mentionNumDic.TryGetValue(array[2], out num);
                if(num< trainNumLimit && num < 0.8*sourceDic[array[2]])
                {
                    SaveForTrain(line);
                    mentionNumDic[array[2]] = num+1;
                    uniqueMentionNumDic.TryGetValue(array[2], out set);
                    if(set == null)
                    {
                        set = uniqueMentionNumDic[array[2]] = new HashSet<string>();
                    }
                    if(!set.Contains(array[0]))
                    {
                        set.Add(array[0]);
                    }
                }
                else
                {
                    SaveForDevOrTest(line);
                }
            }
            reader.Close();
            var writer = new LargeFileWriter(statisticInfoFile, FileMode.Create);
            foreach(var key in mentionNumDic.Keys)
            {
                writer.WriteLine(key + "\t" + mentionNumDic[key]);
                writer.WriteLine(key + "\t" + uniqueMentionNumDic[key].Count);
            }
            writer.Close();
        }

        string line;
        string[] array;
        FileReader reader = null;
        private void ReadOneItem()
        {
            if (reader == null)
            {
                reader = new LargeFileReader(sourceFile);
            }
            this.line = reader.ReadLine();
            array = line.Split('\t');
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

        FileWriter trainWriter = null;
        private void SaveForTrain(string line)
        {
            if (trainWriter == null)
            {
                trainWriter = new LargeFileWriter(this.trainDataFile, FileMode.Create);
            }
            trainWriter.WriteLine(line);
        }

        FileWriter devWriter = null;
        private void SaveForDevOrTest(string line)
        {
            if (devWriter == null)
            {
                devWriter = new LargeFileWriter(this.devAndTestDataFile, FileMode.Create);
            }
            devWriter.WriteLine(line);
        }


        ~DataSpliter()
        {
            trainWriter.Close();
            devWriter.Close();
        }

        public static void Main(string[] args)
        {
            DataSpliter spliter = new DataSpliter(
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\refined-satori.txt",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\statisticInfo.txt",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\train.txt",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\devOrTest.txt",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\trainDataInfo.txt");
            spliter.SplitData();
        }

    }
}
