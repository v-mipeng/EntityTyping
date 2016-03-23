using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using pml.file.writer;
using pml.type;

namespace msra.nlp.tr
{
    class ParallelIndividualFeatureExtractor : ParallelFeatureExtractor
    {
        string source = null;
        string des = null;
        List<string> sourceFiles = null;
        List<string> desFiles = null;
        int numPerThread = 0;

        public ParallelIndividualFeatureExtractor(string source, string des, int numPerThread = 2000)
        {
            this.source = source;
            this.des = des;
            var attr = File.GetAttributes(source);
            if(attr.HasFlag(FileAttributes.Directory))
            {
                if(!Directory.Exists(des))
                {
                    Directory.CreateDirectory(des);
                }
                this.sourceFiles = Directory.GetFiles(source, "*.txt").ToList();
                this.desFiles = new List<string>();
                foreach (var file in sourceFiles)
                {
                    this.desFiles.Add(Path.Combine(des, Path.GetFileName(file)));
                }
            }
            this.numPerThread = numPerThread;
        }

        public override void ExtractFeature()
        {
            if (this.sourceFiles == null)
            {
                var pair = SplitData(source, des, numPerThread);
                sourceFiles = pair.first;
                desFiles = pair.second;
            }
            var ThreadClasses = new List<IndividualFeature>(sourceFiles.Count);
            var threads = new List<Thread>(sourceFiles.Count);

            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var threadClass = new IndividualFeatureExtractor(sourceFiles[i], desFiles[i]);
                var thread = new Thread(threadClass.ExtractFeature);
                thread.Name = "Thread " + i;
                threads.Add(thread);
                thread.Start();
                Console.Clear();
                Console.WriteLine("Thread {0} start.", i);
            }
            // Wait until all the threads complete work
            for (var i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }
        }

        public List<string> ExtractFeatureForQuery(List<Pair<string, string>> queries, int numPerThread = 2000)
        {
            List<string> events = new List<string>();
            for(var i = 0;i<queries.Count;i++)
            {
                events.Add(null);
            }
            var ThreadClasses = new List<IndividualFeature>((int)Math.Ceiling(1.0*queries.Count/numPerThread));
            var threads = new List<Thread>(sourceFiles.Count);

            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var threadClass = new IndividualFeatureExtractor(queries, i*numPerThread, (i+1)*numPerThread-1, events);
                var thread = new Thread(threadClass.ExtractFeatureForQuery);
                thread.Name = "Thread " + i;
                threads.Add(thread);
                thread.Start();
                Console.Clear();
                Console.WriteLine("Thread {0} start.", i);
            }
            // Wait until all the threads complete work
            for (var i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }
            return events;
        }

        public void AddFeature()
        {
            if (this.sourceFiles == null)
            {
                var pair = SplitData(source, des, numPerThread);
                sourceFiles = pair.first;
                desFiles = pair.second;
            }
            var ThreadClasses = new List<IndividualFeature>(sourceFiles.Count);
            var threads = new List<Thread>(sourceFiles.Count);

            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var threadClass = new IndividualFeatureExtractor(sourceFiles[i], desFiles[i]);
                var thread = new Thread(threadClass.AddFeature);
                thread.Name = "Thread " + i;
                threads.Add(thread);
                thread.Start();
                Console.Clear();
                Console.WriteLine("Thread {0} start.", i);
            }
            // Wait until all the threads complete work
            for (var i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }
        }

    }
}
