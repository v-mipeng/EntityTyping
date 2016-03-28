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
            while (reader.HasNext())
            {
                var instance = reader.GetNextInstance();
                try
                {
                    List<string> feature = null;
                    try
                    {
                        feature = extractor.ExtractFeature(instance);
                    }
                    catch (Exception)
                    {
                        feature = extractor.ExtractFeature(instance, false);
                    }
                    var e = new Event(instance.Label, feature);
                    writer.WriteEvent(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(instance);
                }
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
                        feature = extractor.ExtractFeature(instance, false);
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
        }


    }
}
