using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using pml.file.writer;
using pml.type;

namespace msra.nlp.tr
{
    class ParallelSVMFeatureExtractor : ParallelFeatureExtractor
    {
        string source = null;
        string des = null;
        List<string> sourceFiles = null;
        List<string> desFiles = null;
        // define number each thread to deal with
        static int numPerThread = 10000;


        public ParallelSVMFeatureExtractor(string source, string des)
        {
            this.source = source;
            this.des = des;
            var attr = File.GetAttributes(source);
            if (attr.HasFlag(FileAttributes.Directory))
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
            var ThreadClasses = new List<SVMFeatureExtractor>(sourceFiles.Count);
            var threads = new List<Thread>(sourceFiles.Count);

            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var threadClass = new SVMFeatureExtractor(sourceFiles[i], desFiles[i]);
                var thread = new Thread(threadClass.ExtractFeature);
                threads.Add(thread);
                thread.Name = "Thread " + i;
                thread.Start();
            }
            // Wait until all the threads complete work
            for (var i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }
        }
    }
}
