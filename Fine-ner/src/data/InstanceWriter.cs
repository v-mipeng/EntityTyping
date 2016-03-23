using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    /// <summary>
    /// Write instance by line into file
    /// </summary>
    interface InstanceWriter
    {
        void WriteInstance(Instance instance);

        bool Close();
    }
}
