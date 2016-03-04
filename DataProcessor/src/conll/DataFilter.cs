using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pml.file.reader;
using pml.file.writer;
using System.IO;
namespace msra.nlp.dp.conll
{
    public class DataFilter
    {
        // original satori file
        private string originalFileDic = " ";
        // directory to store the satori data by type
        private string refinedFileDic = " ";
        // file to store  the number by type information
        private string statisticInfoFile = "./item-num-by-type.txt";

        public DataFilter(string originalFileDic, string refinedFileDic)
        {
            if (!Directory.Exists(originalFileDic))
            {
                throw new Exception(string.Format("Director:{0} does not exist!", originalFileDic));
            }
            this.originalFileDic = originalFileDic;
            this.refinedFileDic = refinedFileDic;
            if (!Directory.Exists(refinedFileDic))
            {
                Directory.CreateDirectory(refinedFileDic);
            }
        }

        public DataFilter(string originalFileDic, string refinedFileDic, string statisticInfoFile)
            : this(originalFileDic, refinedFileDic)
        {
            this.statisticInfoFile = statisticInfoFile;
        }

        public void Refine()
        {
            var reader = new LargeFileReader();
            var writer = new LargeFileWriter();
            // create reader and writer
            var sourceFiles = Directory.GetFiles(originalFileDic);

            string line;
            int count = 0;
            foreach (var file in sourceFiles)
            {
                reader.Open(file);
                writer.Open(Path.Combine(refinedFileDic, Path.GetFileName(file)), FileMode.Create);
                var numByType = new Dictionary<string, int>();
                while ((line = reader.ReadLine()) != null)
                {
                    if (++count % 1000 == 0)
                    {
                        Console.WriteLine(count);
                    }
                    var array = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (IsValidItem(array))
                    {
                        writer.WriteLine(string.Format("{0}\t{1}\t{2}", array[0], array[1], array[2]));    // mentinon   type    context
                        try
                        {
                            numByType[array[1]] += 1;
                        }
                        catch (Exception)
                        {
                            numByType[array[1]] = 1;
                        }
                    }
                }
                reader.Close();
                writer.Close();
                writer.Open(statisticInfoFile, FileMode.Append);
                writer.WriteLine(Path.GetFileNameWithoutExtension(file));
                foreach (var type in numByType.Keys)
                {
                    writer.WriteLine(type + "\t" + numByType[type]);
                }
                writer.WriteLine("");
                writer.Close();
            }
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
            // Remove items whose context shorter than 3 * Len(mention)
            if (array[2].Split(' ').Length < 4 * array[0].Split(' ').Length)
            {
                return false;
            }
            return true;
        }

    }
}
