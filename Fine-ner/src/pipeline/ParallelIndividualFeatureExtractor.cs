using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using pml.file.writer;

namespace msra.nlp.tr
{
    class ParallelIndividualFeatureExtractor : ParallelFeatureExtractor
    {
        string source = null;
        string des = null;
        List<string> sourceFiles = null;
        List<string> desFiles = null;
        // define number each thread to deal with
        static int numPerThread = 50000;


        public ParallelIndividualFeatureExtractor(string source, string des)
        {
            this.source = source;
            this.des = des;
            var attr = File.GetAttributes(source);
            if(attr.HasFlag(FileAttributes.Directory))
            {
                this.sourceFiles = Directory.GetFiles(source, "*.txt").ToList();
                this.desFiles = new List<string>();
                foreach (var file in sourceFiles)
                {
                    this.desFiles.Add(Path.Combine(des, Path.GetFileName(file)));
                }
            }
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
