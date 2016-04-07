using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using pml.file.reader;
using pml.file.writer;


namespace msra.nlp.tr.dp.satori
{
    /// <summary>
    /// Extract Satori data which is of interest types.
    /// </summary>
    class DataRefiner
    {
        // original satori file
        private string originalFile = " ";
        // hierarchy of interest types
        private string hierarchyFile = " ";
        // directory to store the satori data by type
        private string refinedDir = " ";
        // file to store  the number by type information
        private string statisticInfoFile = "./item-num-by-type.txt";

        Dictionary<string, string> low2top = null;

        public DataRefiner(string originalFile, string hierarchyFile, string refinedDir)
        {
            this.originalFile = originalFile;
            this.hierarchyFile = hierarchyFile;
            this.refinedDir = refinedDir;
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
            // create writers
            HashSet<string> topTypes = new HashSet<string>(low2top.Values); 
            var writers = new Dictionary<string, FileWriter>();
            var paths = new List<string>();
            foreach(var type in topTypes)
            {
                var path = Path.Combine(this.refinedDir, type.Replace('.','_') + ".txt");
                 writers[type] = new LargeFileWriter(path, FileMode.Create);
                paths.Add(path);
            }
            FileWriter writer = null;
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
                    try {
                        writer = writers[this.type];
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
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
            foreach (var w in writers.Values)
            {
                w.Close();
            }
            foreach(var p in paths)
            {
                File.SetAttributes(p, FileAttributes.ReadOnly);
            }
            writer = new LargeFileWriter(statisticInfoFile, FileMode.Create);
            foreach(var type in numByType.Keys)
            {
                writer.WriteLine(type + "\t" + numByType[type]);
            }
            writer.Close();
        }

        /// <summary>
        ///  Filter items with some criterions
        /// </summary>
        /// <param name="array">
        ///     mention  entity  types   context
        /// </param>
        /// <returns></returns>
        private bool IsValidItem(string[] array)
        {
            if (array.Length != 4)
            {
                return false;
            }
            // Remove items whose context shorter than 3 * Len(mention)
            if (array[3].Length < 4 * array[0].Length && array[3].Length < 50)
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


       /// <summary>
       /// Load the hierarchy of interest types.
       /// </summary>
        private void LoadHierarchy()
        {
            FileReader reader = new LargeFileReader(hierarchyFile);
            this.low2top = new Dictionary<string, string>();
            string line;
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                count++;
                if(count == 120)
                {
                    Console.Write("debug!");
                }
                var array = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if(line.Equals("medicine.drug"))
                {
                    Console.Write(line);
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
          
        }
    }
}
