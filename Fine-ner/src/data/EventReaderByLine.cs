using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;

namespace msra.nlp.tr
{
    class EventReaderByLine : EventReader
    {

        FileReader reader = null;
        Event nextEvent = null;
        Exception exception = null;
        List<string> featureHeads = null;
        Dictionary<string, int> featureHeadIndex = null;

        public EventReaderByLine(string filePath, bool HasHead = false)
        {
            Open(filePath, HasHead);
        }

        public void Open(string filePath, bool HasHead = false)
        {
            reader = new LargeFileReader(filePath);
            if(HasHead)
            {
                this.featureHeads = ReadFeatureHeads();
                featureHeadIndex = new Dictionary<string,int>();
                for (var i = 0; i < this.featureHeads.Count; i++)
                {
                    featureHeadIndex[featureHeads[i]] = i;
                }
            }
            try
            {
                nextEvent = ReadEvent();
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        /// <summary>
        ///  Read Event from file by line. So file format should be:
        ///    Label   TAB   feature1    TAB    feature2   TAB...
        /// </summary>
        /// <returns></returns>
        public Event GetNextEvent()
        {
            if (exception != null)
            {
                var e = exception;
                exception = null;
                throw e;
            }
            if (this.nextEvent == null)
            {
                throw new Exception("There is no more event in the file!");
            }
            var env = nextEvent;
            try
            {
                nextEvent = ReadEvent();
            }
            catch (Exception e)
            {
                exception = e;
            }
            return env;
        }


        /// <summary>
        /// Return if there is event having not been read in the file
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            if (nextEvent != null)
            {
                return true;
            }
            return false;
        }

        public bool Close()
        {
            try
            {
                reader.Close();
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if event file contains head information
        /// </summary>
        /// <returns></returns>
        public bool ContainHeads()
        {
            if(featureHeads != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
       
        /// <summary>
        /// Get event heads
        /// </summary>
        public List<string> FeatureHeads
        {
            get
            {
                return this.featureHeads;
            }
            private set{}
        }

        /// <summary>
        /// Get head index in event feature starting from 0.
        /// </summary>
        /// <param name="head"></param>
        /// <returns>
        /// 
        /// </returns>
        public int GetFeatureHeadIndex(string head)
        {
            if(featureHeadIndex == null)
            {
                throw new Exception("Event file does not contain heads!");
            }
            int index = 0;
            if(featureHeadIndex.TryGetValue(head, out index))
            {
                return index;
            }
            return -1;
        }

        

        /// <summary>
        /// Read a event by line.
        /// The line should seperated by TAB with the first item being label and others being features
        /// </summary>
        /// <returns></returns>
        private Event ReadEvent()
        {
            var line = reader.ReadLine();
            if (line == null)
            {
                return null;
            }
            var array = line.Split('\t');
            if (array.Length < 2)
            {
                throw new Exception("Line format is wrong! Line cannot be seperated by TAB");
            }
            var feature = new List<string>();
            for (var i = 1; i < array.Length; i++)
            {
                feature.Add(array[i]);
            }
            return new Event(new Label(array[0]), feature);
        }

        private List<string> ReadFeatureHeads()
        {
            var line = reader.ReadLine();
            var array = line.Split('\t');
            var list = new List<string>();
            for (var i = 1; i < array.Length;i++ )
            {
                list.Add(array[i]);
            }
            return list;
        }

    }
}
