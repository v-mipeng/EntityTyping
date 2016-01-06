using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace pml.file.reader
{
    public class LargeFileReader : FileReader
    {
        /*The path of the file to read
         */
        String filePath = null;
        FileStream fileStream = null;
        StreamReader streamReader = null;
        /*File encode type(default UTF8)
         */
        Encoding encoding = Encoding.UTF8;
        /*The buffer size to be used for the streamReader
         */
        int bufferSize = 4096;
        /*Is file end reached
         */
        public bool reachFileEnd = false;

        public LargeFileReader() { }

        ~LargeFileReader()
        {
            Close();
        }
        /*Create an FileReader object and assign the file path
         */
        public  LargeFileReader(String filePath)
        {
            this.filePath = filePath;
        }

        /*Create an FileReader object, assign the file path and encoding type
       */
        public LargeFileReader(String filePath, Encoding encoding)
        {
            this.filePath = filePath;
            this.encoding = encoding;
        }

        /* Open file defined by the given filePath with default encoding
         */
        public bool Open(String filePath)
        {
            this.filePath = filePath;
            return Open();
        }

        /* Open file defined by the given filePath with given encoding
        */
        public bool Open(String filePath, Encoding encoding)
        {
            this.filePath = filePath;
            this.encoding = encoding;
            return Open();
        }

        /*Close file reader object
         */
        public bool Close()
        {
            if (this.fileStream != null)
            {
                streamReader.Close();
                this.streamReader = null;
                fileStream.Close();
                fileStream = null;
                return true;
            }
            return false;
        }


        /* Read a character from the file stream
         */
        public Char Read()
        {
            if (this.streamReader == null)
            {
                Open();
            }
            return (char)streamReader.Read();
        }

        /* Read all the text of the file
         */
        public String ReadAll()
        {
            if (this.streamReader == null)
            {
                Open();
            }
            return streamReader.ReadToEnd();
        }

        /*Read a line of the file
         */
        public String ReadLine()
        {
            if (this.streamReader == null)
            {
                Open();
            }
            return streamReader.ReadLine();

        }

        /**Read all the lines of the file
         * return a string array with every element stores a line.
         */
        public string[] ReadAllLines()
        {

            List<String> lines = new List<string>();
            String line;
            while ((line = ReadLine()) != null)
            {
                lines.Add(line);
            }
            return lines.ToArray();
        }

        /*Skip given number of words
         */ 
        public void Skip(int offset)
        {
            if (streamReader == null)
            {
                Open();
            }
            try
            {
                streamReader.Read(new char[offset], 0, offset);
            }
            catch
            {
                reachFileEnd = true;
            }
        }

        /*Read given number of lines
         */ 
        public String[] ReadLines(int lineNum)
        {
            String[] lines = new string[lineNum];
            String line;
            for (int i = 0; i < lineNum; i++)
            {
                line = ReadLine();
                if (line == null)  // reach the file end
                {
                    reachFileEnd = true;
                    break;
                }
                else
                {
                    lines[i] = line;
                }
            }
            return lines;
        }
        /*Skip given number of lines
         */
        public void SkipByLine(int skipedLines)
        {
            ReadLines(skipedLines);
        }

        static int K1 = 1024;
        static long M1 = K1 * K1;
        static long G1 = M1 * M1;

        /*Set buffer size according to the file size
         */
        private void SetBuffserSize()
        {
            FileInfo fileInfo = new FileInfo(this.filePath);
            long fileSize = fileInfo.Length;
            if (fileSize < 10 * M1)
            {
                this.bufferSize = 512;
            }
            else if (fileSize < 0.5 * G1)
            {
                this.bufferSize = 4096;
            }
            else
            {
                this.bufferSize = 16 * K1;
            }
        }

        /*Open file
         */
        private bool Open()
        {
            if (this.filePath == null)
            {
                throw new FileException("No file assigned to open!");
            }
            else
            {
                Close();
                try
                {
                    SetBuffserSize();
                    this.fileStream = new FileStream(this.filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize * 4, FileOptions.SequentialScan);
                    this.streamReader = new StreamReader(this.fileStream, this.encoding, true, this.bufferSize);
                }
                catch (Exception e)
                {
                    this.fileStream = null;
                    this.streamReader = null;
                    throw new FileException(filePath + " is an invalid file path!");
                }
            }
            return true;
        }

    }
}
