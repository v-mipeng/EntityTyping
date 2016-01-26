using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.writer;
using System.IO;

namespace msra.nlp.tr
{
    class EventWriterByLine : EventWriter
    {

        FileWriter writer = null;

        public EventWriterByLine(string filePath)
        {
            writer = new LargeFileWriter(filePath, FileMode.Create);
        }

        public void WriteEvent(Event e)
        {
            writer.Write(e.Label.StringLabel);
            foreach(var str in (IEnumerable<string>)e.Feature)
            {
                writer.Write("\t"+str);
            }
            writer.Write("\r");
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
