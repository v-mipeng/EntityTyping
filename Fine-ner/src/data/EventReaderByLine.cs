﻿using System;
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
        public EventReaderByLine(string filePath)
        {
            reader = new LargeFileReader(filePath);
            try
            {
                nextEvent = ReadEvent();
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
    }
}
