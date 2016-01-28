using pml.file.reader;
using pml.file.writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pml.file.util
{
    public class Util
    {
       /// <summary>
       /// Combine files given by sourceFiles into one file given by desFile
       /// </summary>
       /// <param name="sourceFiles">
       /// Source file pathes to be combined
       /// </param>
       /// <param name="desFile">
       /// The file path to store the combined file
       /// </param>
       public static void CombineFiles(IEnumerable<string> sourceFiles, string desFile)
        {
            var reader = new LargeFileReader();
            var writer = new LargeFileWriter(desFile, FileMode.Create);
            string line;

            foreach (var file in sourceFiles)
            {
                reader.Open(file);
                while ((line = reader.ReadLine()) != null)
                {
                    writer.WriteLine(line);
                }
            }
            reader.Close();
            writer.Close();
        }

        /// <summary>
        /// Combine files given by sourceFiles into one file given by desFile
        /// </summary>
        /// <param name="sourceDir">
        /// Folder path storing the files to be combined
        /// </param>
        /// <param name="desFile">
        /// File path to store the file combined
        /// </param>
        /// <param name="searchPattern">
        /// Patttern to search the file in the given source folder
        /// </param>
        public static void CombineFiles(string sourceDir, string desFile, string searchPattern = null)
        {
            string[] files = null;

            if (Directory.Exists(sourceDir))
            {
                if (searchPattern != null)
                {
                   files  = Directory.GetFiles(sourceDir, searchPattern);
                }
                else
                {
                    files = Directory.GetFiles(sourceDir);
                }
            }
            else
            {
                throw new FileException(string.Format("Does not exist directory {0}.", sourceDir));
            }
            CombineFiles(files, desFile);
        }
    }
}
