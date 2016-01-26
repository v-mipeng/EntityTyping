using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace msra.nlp.tr
{
    class IndividualFeatureExtractor : FeatureExtractor
    {
        InstanceReader reader = null;
        EventWriter writer = null;
        IndividualFeature extractor = null;

        public IndividualFeatureExtractor(string sourceFilePath, string desFilePath)
        {
            reader = new InstanceReaderByLine(sourceFilePath);
            writer = new EventWriterByLine(desFilePath);
            extractor = new IndividualFeature();
        }

        public override void ExtractFeature()
        {
            int count = 0;
            while (reader.HasNext())
            {
                //if (++count % 100 == 0)
                //{
                    count++;
                    Console.Clear();
                    Console.WriteLine("{0} has processed {1}", Thread.CurrentThread.Name, count);
                //}
                var instance = reader.GetNextInstance();
                try
                {
                    var feature = extractor.ExtractFeature(instance);
                    var e = new Event(instance.Label, feature);
                    writer.WriteEvent(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            reader.Close();
            writer.Close();
        }


    }
}
