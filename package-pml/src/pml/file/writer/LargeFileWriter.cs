using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace pml.file.writer
{
   public  class LargeFileWriter   : FileWriter
    {

        /*The path of the file to read
       */
        string filePath = null;
        FileStream fileStream = null;
        StreamWriter streamWriter = null;
        /*File encode type(default UTF8)
         */
        Encoding encoding = Encoding.UTF8;
        /*The buffer size to be used for the streamReader
         */
        int bufferSize = 4096;
        /*Is file end reached
         */

    
        FileMode fileMode = FileMode.Open;
        FileShare fileShare = FileShare.ReadWrite;



        public LargeFileWriter() { }

        public LargeFileWriter(string filePath)
        {
            this.filePath = filePath;
        }

        public LargeFileWriter(string filePath, FileMode fileMode)
        {
            this.filePath = filePath;
            this.fileMode = fileMode;
        }

        public LargeFileWriter(string filePath, FileMode fileMode, FileShare fileShare)
        {
            this.filePath = filePath;
            this.fileMode = fileMode;
            this.fileShare = fileShare;
        }
        public LargeFileWriter(string filePath,FileMode fileMode,FileShare fileShare,Encoding encoding)
        {
            this.filePath = filePath;
            this.fileMode = fileMode;
            this.fileShare = fileShare;
            this.encoding = encoding;
        }
     
       ~LargeFileWriter()
        {
            Close();
        }

       /*Throw Exception if file cannot open!
          */ 
        public void Open(string filePath)
        {
            this.filePath = filePath;
            Open();
        }
        /*Throw Exception if file cannot open!
        */ 
        public void Open(string filePath, FileMode fileMode)
        {
            this.filePath = filePath;
            this.fileMode = fileMode;
            Open();
        }


        /*Throw Exception if file cannot open!
        */ 
        public void Open(string filePath, FileMode fileMode, FileShare fileShare)
        {
            this.filePath = filePath;
            this.fileMode = fileMode;
            this.fileShare = fileShare;
            Open();
        }

        /*Throw Exception if file cannot open!
        */ 
        public void Open(string filePath, FileMode fileMode, FileShare fileShare, Encoding encoding)
        {
            this.filePath = filePath;
            this.fileMode = fileMode;
            this.fileShare = fileShare;
            this.encoding = encoding;
            Open();
        }

        /*Throw Exception if file cannot open!
         */ 
        private void Open()
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
                    this.fileStream = new FileStream(this.filePath, this.fileMode, FileAccess.Write, this.fileShare, bufferSize * 4);
                    this.streamWriter = new StreamWriter(this.fileStream, this.encoding, this.bufferSize);
                }
                catch (Exception e)
                {
                    this.fileStream = null;
                    this.streamWriter = null;
                    throw new FileException("Cannot open file: "+filePath+" because "+e.Message);
                }
            }
        }

        public void Close()
        {
            if(streamWriter != null)
            {
                streamWriter.Close();
                streamWriter = null;
                fileStream.Close();
                fileStream = null;
            }
        }

        public void Write(object output)
        {
            if(fileStream == null)
            {
                Open();
            }
            this.streamWriter.Write(output);
        }

        public void WriteLine(object output)
        {
            if (fileStream == null)
            {
                Open();
            }
            Write(output + "\r\n");
        }

        StringBuilder buffer = null;

        /* Cache the output. 
         * You can obsolete cached object by invoking ClearCache()
         * or you can write it to the file with WriteCache()
         */
        public void Cache(object output)
        {
            if(buffer == null)
            {
                buffer = new StringBuilder();
            }
            buffer.Append(output.ToString());
        }

        /* Cache the output and append a default line terminal to it. 
         * You can obsolete cached object by invoking ClearCache()
         * or you can write it to the file with WriteCache()
         */
        public void CacheLine(object output)
        {
            if (buffer == null)
            {
                buffer = new StringBuilder();
            }
            buffer.Append(output.ToString()+"\r");
        }

        /* Clear the Cached object
         */ 
        public void ClearCache()
        {
            buffer.Clear();
        }

       /* Write the cached object to the file stream
         */ 
        public void WriteCache()
        {
            if (buffer != null)
            {
                Write(buffer.ToString());
                buffer.Clear();
            }
        }
    }
}
