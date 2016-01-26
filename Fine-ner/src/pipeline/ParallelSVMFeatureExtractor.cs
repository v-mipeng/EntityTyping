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


        public ParallelSVMFeatureExtractor(string sourceFilePath, string desFilePath)
        {
            this.source = sourceFilePath;
            this.des = desFilePath;
        }


        public override void ExtractFeature()
        {
            var pair = SplitData(source, des, numPerThread);
            sourceFiles = pair.first;
            desFiles = pair.second;
            var ThreadClasses = new List<SVMFeatureExtractor>(sourceFiles.Count);
            var threads = new List<Thread>(sourceFiles.Count);

            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var threadClass = new SVMFeatureExtractor(sourceFiles[i], desFiles[i]);
                var thread = new Thread(threadClass.ExtractFeature);
                threads.Add(thread);
                thread.Start();
            }
            // Wait until all the threads complete work
            for (var i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }
            // combine features extracted by different threads
            var writer = new LargeFileWriter(this.des, FileMode.Create);
            foreach (var f in this.desFiles)
            {
                string text = File.ReadAllText(f);
                writer.Write(text);
                File.Delete(f);
            }
            writer.Close();
            // delete temp part files
            foreach (var f in this.sourceFiles)
            {
                File.Delete(f);
            }
        }

  
    }
}
