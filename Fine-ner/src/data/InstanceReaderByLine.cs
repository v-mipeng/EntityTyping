using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;

namespace msra.nlp.tr
{
    class InstanceReaderByLine : InstanceReader
    {
        // instance reader        
        FileReader reader = null;
        // next instance
        Instance nextInstance = null;
        // check if there is error in reading instance
        Exception exception = null;

        public InstanceReaderByLine(string filePath)
        {
            reader = new LargeFileReader(filePath);
            try
            {
                nextInstance = ReadInstance();
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        public void Open(string filePath)
        {
            reader = new LargeFileReader(filePath);
            try
            {
                nextInstance = ReadInstance();
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        /// <summary>
        ///  Read Event from file by line. So file format should be:
        ///    Mention [TAB] Type [TAB]  Context
        ///  Check if reaching file end with HashNext() function.
        /// <returns></returns>
        /// <exception>
        ///     If the format of a line is wrong or file reaches end.
        /// </exception>
        /// </summary>
        /// <returns>
        /// An Instance
        /// </returns>
        public Instance GetNextInstance()
        {
            if (exception != null)
            {
                var e = exception;
                exception = null;
                throw e;
            }
            if (this.nextInstance == null)
            {
                throw new Exception("There is no more instance in the file!");
            }
            var ins = nextInstance;
            try
            {
                nextInstance = ReadInstance();
            }
            catch (Exception e)
            {
                exception = e;
            }
            return ins;
        }

        /// <summary>
        /// Read a instance by line
        /// </summary>
        /// <returns></returns>
        private Instance ReadInstance()
        {
            var line = reader.ReadLine();
            if (line == null)
            {
                return null;
            }
            var array = line.Split('\t');
            if (array.Length != 3)
            {
                throw new Exception("Line format is wrong! Line seperated by tab results in" + array.Length + " elements.");
            }
            return new Instance(array[0], array[1], array[2]);
        }

        public bool HasNext()
        {
            if (nextInstance != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Close instance reader
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            try
            {
                reader.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
