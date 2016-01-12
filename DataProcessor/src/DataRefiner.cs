using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using pml.file.reader;
using pml.file.writer;


namespace msra.nlp.tr.dp
{
    class DataRefiner
    {
       private  string originalFile = " ";
        private string hierarchyFile = " ";
        private string refinedFile = " ";
        private string statisticInfoFile = "./item-num-by-type.txt";

        Dictionary<string, string> low2top = null;

        public DataRefiner(string originalFile, string hierarchyFile, string refinedFile)
        {
            this.originalFile = originalFile;
            this.hierarchyFile = hierarchyFile;
            this.refinedFile = refinedFile;
        }

        public DataRefiner(string originalFile, string hierarchyFile, string refinedFile, string statisticInfoFile) : this(originalFile, hierarchyFile, refinedFile)
        {
            this.statisticInfoFile = statisticInfoFile;
        }

        string type = null;

        public void Refine()
        {
            if(low2top == null)
            {
                LoadHierarchy();
            }
            var reader = new LargeFileReader(originalFile);
            var writer = new LargeFileWriter(refinedFile, FileMode.Create);
            string line;
            var numByType = new Dictionary<string, int>();
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if(++count%10000 == 0)
                {
                    Console.WriteLine(count);
                }
                var array = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (IsValidItem(array))
                {
                    writer.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", array[0], array[1], this.type, array[3].Substring(3)));
                    try
                    {
                        numByType[this.type] += 1;
                    }
                    catch (Exception)
                    {
                        numByType[this.type] = 1;
                    }
                }
            }
            reader.Close();
            writer.Close();
            writer.Open(statisticInfoFile, FileMode.Create);
            foreach(var type in numByType.Keys)
            {
                writer.WriteLine(type + "\t" + numByType[type]);
            }
            writer.Close();
        }

        // mention  entity  types   context
        private bool IsValidItem(string[] array)
        {
            if (array.Length != 4)
            {
                return false;
            }
            // Remove items whose context shorter than 3 * Len(mention)
            if (array[3].Length < 4 * array[0].Length)
            {
                return false;
            }
            // Remove items whose first possible type is not of interest types
            var types = array[2].Split('#');
            string type1;
            if (!low2top.TryGetValue(types[0], out type1))
            {
                return false;
            }
            //  Remove items with multiple top level types within first three possible types
            for (var i = 1; i < types.Length & i < 2; i++)
            {
                var type = types[i];
                string topType;
                if (low2top.TryGetValue(type, out topType))
                {
                    if (!topType.Equals(type1))
                    {
                        return false;
                    }
                }
            }
            this.type = type1;
            return true;
        }



        private void LoadHierarchy()
        {
            FileReader reader = new LargeFileReader(hierarchyFile);
            this.low2top = new Dictionary<string, string>();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                var array = line.Split('\t');
                if (array.Length < 2)
                {
                    continue;
                }
                low2top[array[0]] = array[0];
                for (int i = 1; i < array.Length; i++)
                {
                    low2top[array[i]] = array[0];
                }
            }
            reader.Close();
        }

        static void Mains(string[] args)
        {
           var refiner = new DataRefiner(@"E:\Users\v-mipeng\Data\Satori\Raw\Interlink.stype.tsv", 
               @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori ontology\temp-hierarchy.txt",
               @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\refined-satori.txt",
               @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\statisticInfo.txt");
            refiner.Refine();
           File.SetAttributes(@"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\refined-satori.txt",FileAttributes.ReadOnly);
            File.SetAttributes(@"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\refined-satori.txt", FileAttributes.ReadOnly);
        }
    }
}
