using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace msra.nlp.tr
{
    class IndividualFeatureExtractor : FeatureExtractor
    {
        IndividualFeature extractor = null;
        string source = null;
        string des = null;

        public IndividualFeatureExtractor(string sourceFilePath, string desFilePath)
        {
            source = sourceFilePath;
            des = desFilePath;
            extractor = new IndividualFeature();
        }

        public override void ExtractFeature()
        {
            int count = 0;
            var reader = new InstanceReaderByLine(source);
            var writer = new EventWriterByLine(des);
            while (reader.HasNext())
            {
                if (++count % 1000 == 0)
                {
                    Console.Clear();
                    Console.WriteLine("{0} has processed {1}", Thread.CurrentThread.Name, count);
                }
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
                    //Console.WriteLine(e.StackTrace);
                    Console.WriteLine(instance);
                }
            }
            reader.Close();
            writer.Close();
        }

        public void AddFeature()
        {
            var reader = new EventReaderByLine(source);
            var writer = new EventWriterByLine(des);
            int count = 0;
            var dic = new Dictionary<string, int>();

            while (reader.HasNext())
            {
                if (++count % 1000 == 0)
                {
                    Console.Clear();
                    Console.WriteLine("{0} has processed {1}", Thread.CurrentThread.Name, count);
                }
                if(count > 100000)
                {
                    break;
                }
                var e = reader.GetNextEvent();
                try
                {
                    var feature = extractor.AddFeature(e);
                    e = new Event(e.Label, feature);
                    try
                    {
                        dic[feature[feature.Count - 2]] += 1;
                    }
                    catch (Exception)
                    {
                        dic[feature[feature.Count - 2]] = 0;

                    }
                    writer.WriteEvent(e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //Console.WriteLine(e.StackTrace);
                    Console.WriteLine(e);
                }
            }
            Console.WriteLine("Effect for file {0}", Path.GetFileName(source));
            foreach (var item in dic)
            {
                Console.WriteLine(item.Key + ":" + item.Value);
            }
            Console.WriteLine();
            Console.ReadKey();
            reader.Close();
            writer.Close();
        }


    }
}
