using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.type;

namespace msra.nlp.tr
{
    interface Ner
    {

        List<Pair<string, string>> FindNer(string context);

        string GetNerType(string mention);

    }
}
