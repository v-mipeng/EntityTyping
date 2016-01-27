using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;


namespace msra.nlp.tr
{
    interface EventReader
    {
        void Open(string filePath, bool HasHead = false);

        Event GetNextEvent();

        bool HasNext();


        /// <summary>
        /// Check if event file contains head information
        /// </summary>
        /// <returns></returns>
        bool ContainHeads();


        /// <summary>
        /// Get head index in event feature starting from 0.
        /// If head not exists, return -1
        /// </summary>
        /// <param name="head"></param>
        /// <returns>
        /// 
        /// </returns>
        int GetFeatureHeadIndex(string head);


        bool Close();
    }
}
