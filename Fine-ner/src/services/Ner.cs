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

        void FindNer(string context);

        List<Pair<string, string>>  GetEntities();

        string GetNerType(string mention);

    }
}
