using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    interface InstanceReader
    {

        void Open(string filePath);

        Instance GetNextInstance();

        bool HasNext();

        bool Close();
    }
}
