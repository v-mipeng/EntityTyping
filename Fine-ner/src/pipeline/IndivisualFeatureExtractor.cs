using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using pml.type;

namespace msra.nlp.tr
{
    /// <summary>
    /// Extract features of queries stored in file and store the features into given file
    /// </summary>
    class IndividualFeatureExtractor : FeatureExtractor
    {
        IndividualFeature extractor = new IndividualFeature();
        string source = null;
        string des = null;
        List<Pair<string, string>> queries = null;
        int begin=0, end = 0;
        List<string> events;

        public IndividualFeatureExtractor(List<Pair<string, string>> queries, List<string> events)
            :this(queries,0,queries.Count-1,events)
        {

        }

        public IndividualFeatureExtractor(List<Pair<string, string>> queries, int begin, int end, List<string> events) 
        {
            this.queries = queries;
            this.begin = begin;
            this.end = end;
            this.events = events;
        }

        public IndividualFeatureExtractor(string sourceFilePath, string desFilePath)
        {
            source = sourceFilePath;
            des = desFilePath;
        }

        public override void ExtractFeature()
        {
            var reader = new InstanceReaderByLine(source);
            var writer = new EventWriterByLine(des);
            int index = 0;
            int count = 0;

            while (reader.HasNext())
            {
                if(++count%1000 == 0)
                {
                    Console.WriteLine(Thread.CurrentThread.Name + " has processed " + count + " items.");
                }
                var instance = reader.GetNextInstance();
                try
                {
                    List<string> feature = null;
                    feature = extractor.ExtractFeature(instance);
                    var e = new Event(instance.Label, feature);
                    writer.WriteEvent(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("error happened in file {0} item {1}", source, index));
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                index++;
            }
            reader.Close();
            writer.Close();
        }

        public void ExtractFeatureForQuery()
        {
            var events = new List<Event>();
            for (var i = this.begin; i <= this.end && i<queries.Count; i++)
            {
                var instance = new Instance(this.queries[i].first, this.queries[i].second);
                try
                {
                    List<string> feature = null;
                    try
                    {
                        feature = extractor.ExtractFeature(instance);
                    }
                    catch (Exception)
                    {
                        feature = extractor.ExtractFeature(instance);
                    }
                    var e = new Event(instance.Label, feature);
                    events[i] = e;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message+"for instance\n"+instance);
                    Console.WriteLine("Skip this query!");
                    events.Add(null);
                }
            }
        }

        public void AddFeature()
        {
            var reader = new EventReaderByLine(source);
            var writer = new EventWriterByLine(des);
            int count = 0;

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
                    //var feature = extractor.AddFeature(e);
                    //e = new Event(e.Label, feature);
                    //writer.WriteEvent(e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(e);
                }
            }
            reader.Close();
            writer.Close();
        }

    }
}
