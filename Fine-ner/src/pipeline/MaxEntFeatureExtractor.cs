using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace msra.nlp.tr
{
    class MaxEntFeatureExtractor : FeatureExtractor
    {
        EventReader reader = null;
        EventWriter writer = null;
        MaxEntFeature extractor = null;

        public MaxEntFeatureExtractor(string sourceFilePath, string desFilePath)
        {
            reader = new EventReaderByLine(sourceFilePath);
            writer = new EventWriterByLine(desFilePath);
            extractor = new MaxEntFeature();
        }

        public override void ExtractFeature()
        {
            int count = 0;
            while (reader.HasNext())
            {
                if (++count % 10000 == 0)
                {
                    Console.Clear();
                    Console.WriteLine("{0} has processed {1}", Thread.CurrentThread.Name, count);
                }
                var e = reader.GetNextEvent();
                try
                {
                    var feature = extractor.ExtractFeature(e);
                    writer.WriteEvent(new Event(e.Label, feature));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //Console.WriteLine(e.StackTrace);
                    Console.WriteLine(e);
                }
            }
            reader.Close();
            writer.Close();
        }



    }
}
