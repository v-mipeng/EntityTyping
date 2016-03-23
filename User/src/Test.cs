//#define debug

using pml.file.reader;
using pml.file.writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using msra.nlp.tr.eval;
using User.src;
using pml.type;


namespace msra.nlp.tr
{
    public class OpenNer
    {
        public static void Mains(string[] args)
        {
            var demo = new Demo();
            Console.WriteLine(demo.Predict("House Ways and Means Committee","Influential members of the House Ways and Means Committee introduced legislation that would restrict how the new savings-and-loan bailout agency can raise capital , creating another potential obstacle to the government 's sale of sick thrifts ."));
            Console.ReadKey();

        }
    }
}
