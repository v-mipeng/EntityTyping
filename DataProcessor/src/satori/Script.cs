using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using pml.file.reader;
using pml.file.writer;

namespace msra.nlp.tr.dp.satori
{
    class Script
    {
        /// <summary>
        /// Add training data for product type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="file"></param>
        public static void AddProductData(string source, string file)
        {
            var reader = new LargeFileReader(source);
            var writer = new LargeFileWriter(file, FileMode.Create);
            string line;
            int numLimit = 100000;
            int limitMentionNumPerEntity = 30;
            int numByEntity = 0;
            int count = 0;
            string lastEntity = "";
            
            while((line = reader.ReadLine())!=null && count<numLimit)
            {
                var array = line.Split('\t');
                if(IsValidItem(array))
                {
                   if(array[1].Equals(lastEntity))
                   {
                       if (numByEntity <= limitMentionNumPerEntity)
                       {
                           numByEntity += 1;
                           writer.WriteLine(line);
                           count++;
                       }
                   }
                   else
                   {
                       lastEntity = array[1];
                       numByEntity = 1;
                       writer.WriteLine(line);
                       count++;
                   }
                }
            }
            reader.Close();
            writer.Close();
        }

        private static bool IsValidItem(string[] array)
        {
            if(array.Length!=4)
            {
                return false;
            }
            if(array[3].Length<array[0].Length*4 || array[3].Length<50)
            {
                return false;
            }
            return true;
        }
    }
}
