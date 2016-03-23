using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;

namespace msra.nlp.tr
{
    /// <summary>
    /// Read instances by line. File format of an instance is:
    /// Mention     TAB     Context     (for query)    or
    /// Mention     TAB     Label       Context     (for test)
    /// </summary>
    /// <returns></returns>
    public class InstanceReaderByLine : InstanceReader
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
        /// Read a instance by line. File format of an instance is:
        /// Mention     TAB     Context     (for query)    or
        /// Mention     TAB     Label       Context     (for test)
        /// </summary>
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
        /// Read a instance by line. File format of an instance is:
        /// Mention     TAB     Context     (for query)    or
        /// Mention     TAB     Label       Context     (for test)
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
            if(array.Length == 2)
            {
                return new Instance(array[0], array[1]);   // create instance for test
            }
            if (array.Length == 3)
            {
                return new Instance(array[0], array[1], array[2]);
            }
            if (array.Length == 4)
            {
                return new Instance(array[0], array[2], array[3]);
            }
            else
            {
                throw new Exception("Line format is wrong! Line seperated by tab results in" + array.Length + " elements.");
            }
        }

        /// <summary>
        /// Check if reaching file end
        /// </summary>
        /// <returns></returns>
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
