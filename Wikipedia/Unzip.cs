using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Core;


namespace Wikipedia
{
    /// <summary>
    /// Uncompress compressed files with SharpZipLib.
    /// Download the package with PM> Install-package SharpZipLib in visualstudio and source codes from https://github.com/icsharpcode/SharpZipLib
    /// For usage, please refer https://github.com/icsharpcode/SharpZipLib/wiki/
    /// </summary>
    public class Unzip
    {
        /// <summary>
        /// Uncompress source file to destination.
        /// Recognize file type with source file extension
        /// </summary>
        /// <param name="sourceFile">
        /// Compressed file
        /// </param>
        /// <param name="desDirectory">
        /// Destination file.
        /// </param>
        public static void Uncompress(string sourceFile, string desDirectory = null)
        {
            // check the format of the compressed file
            var fileExtension = Path.GetExtension(sourceFile).ToLower();
            switch (fileExtension)
            {
                case ".zip":
                    UncompressZipFile(sourceFile, desDirectory);
                    break;
                case ".gz":
                    UncompressGZipFile(sourceFile, desDirectory);
                    break;
                case ".tar":
                    UncompressTarFile(sourceFile, desDirectory);
                    break;
                case ".bz2":
                    UncompressBZip2File(sourceFile, desDirectory);
                    break;
                default:
                    Console.Error.WriteLine(string.Format("Sorry, {0} file format is not support!", fileExtension));
                    break;
            }
        }

        public static void UncompressZipFile(string sourceFile, string desDirectory = null)
        {
            if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
            {
                throw new FileNotFoundException(string.Format("{0} does not exist!", sourceFile));
            }
            string directoryName = Path.GetDirectoryName(sourceFile);
            // create directory
            if (string.IsNullOrEmpty(desDirectory))
            {
                desDirectory = directoryName;   // store the uncompressed file in current directory
            }
            else if (!Directory.Exists(desDirectory))
            {
                Directory.CreateDirectory(desDirectory);
            }
            Console.WriteLine(string.Format("Uncompress {0}...", Path.GetFileName(sourceFile)));
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(sourceFile)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (fileName != String.Empty)
                    {
                        directoryName = Path.Combine(desDirectory,Path.GetDirectoryName(theEntry.Name));
                        if(!Directory.Exists(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                        }
                        using (FileStream streamWriter = File.Create(Path.Combine(desDirectory,theEntry.Name)))
                        {
                            int size = 4096;   // buffer size
                            byte[] data = new byte[4096];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Done!");
        }

        public static void UncompressGZipFile(string sourceFile, string desDirectory = null)
        {
            if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
            {
                throw new FileNotFoundException(string.Format("{0} does not exist!", sourceFile));
            }
            string directoryName = Path.GetDirectoryName(sourceFile);
            // create directory
            if (string.IsNullOrEmpty(desDirectory))
            {
                desDirectory = directoryName;
            }
            else if (!Directory.Exists(desDirectory))
            {
                Directory.CreateDirectory(desDirectory);
            }
            byte[] dataBuffer = new byte[4096];
            Console.WriteLine(string.Format("Uncompress {0}...", Path.GetFileName(sourceFile)));
            using (Stream s = new GZipInputStream(File.OpenRead(sourceFile)))
            using (FileStream fs = File.Create(Path.Combine(desDirectory, Path.GetFileNameWithoutExtension(sourceFile))))
            {
                StreamUtils.Copy(s, fs, dataBuffer);
            }
            Console.WriteLine("Done!");

        }

        public static void UncompressTarFile(string sourceFile, string desDirectory = null)
        {
            if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
            {
                throw new FileNotFoundException(string.Format("{0} does not exist!", sourceFile));
            }
            string directoryName = Path.GetDirectoryName(sourceFile);
            // create directory
            if (string.IsNullOrEmpty(desDirectory))
            {
                desDirectory = directoryName;
            }
            else if (!Directory.Exists(desDirectory))
            {
                Directory.CreateDirectory(desDirectory);
            }
            Stream inStream = File.OpenRead(sourceFile);
            Console.WriteLine(string.Format("Uncompress {0}...", Path.GetFileName(sourceFile)));
            TarArchive tarArchive = TarArchive.CreateInputTarArchive(inStream);
            tarArchive.ExtractContents(desDirectory);
            tarArchive.Close();
            inStream.Close();
            Console.WriteLine("Done!");

        }

        public static void UncompressBZip2File(string sourceFile, string desDirectory = null)
        {
            if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
            {
                throw new FileNotFoundException(string.Format("{0} does not exist!", sourceFile));
            }
            string directoryName = Path.GetDirectoryName(sourceFile);
            // create directory
            if (string.IsNullOrEmpty(desDirectory))
            {
                desDirectory = directoryName;
            }
            else if (!Directory.Exists(desDirectory))
            {
                Directory.CreateDirectory(desDirectory);
            }
            Console.WriteLine(string.Format("Uncompress {0}...", Path.GetFileName(sourceFile)));
            BZip2.Decompress(File.OpenRead(sourceFile), File.Create(Path.Combine(desDirectory, Path.GetFileNameWithoutExtension(sourceFile))), false);
            Console.WriteLine("Done!");

        }
    }
}
