using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;


namespace msra.nlp.tr
{
    class EventReader
    {
        private FileReader eventReader = null;

        public EventReader(string filePath)
        {
            eventReader = new LargeFileReader(filePath);
        }

        public Event GetNextEvent()
        {
            
        }

        public bool HasNext()
        {
            return false;
        }
    }
}
