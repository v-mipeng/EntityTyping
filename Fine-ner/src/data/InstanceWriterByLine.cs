using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.writer;
using System.IO;

namespace msra.nlp.tr
{
    /// <summary>
    /// Write instance by line
    /// </summary>
    public class InstanceWriterByLine : InstanceWriter
    {

        FileWriter writer = null;

        public InstanceWriterByLine(string filePath)
        {
            writer = new LargeFileWriter(filePath, FileMode.Create);
        }

        /// <summary>
        /// Write instance by line
        /// </summary>
        /// <param name="instance"></param>
        public void WriteInstance(Instance instance)
        {
            writer.WriteLine(instance.Mention+"\t"+instance.Label.StringLabel+"\t"+instance.Context);
        }

        public bool Close()
        {
            try
            {
                writer.Close();
                return true;
            }
            catch(Exception)
            {
                return false;
            }

        }
    }
}
