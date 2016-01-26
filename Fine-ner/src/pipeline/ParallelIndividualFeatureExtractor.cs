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
        static int numPerThread = 500;


        public ParallelIndividualFeatureExtractor(string sourceFilePath, string desFilePath)
        {
            this.source = sourceFilePath;
            this.des = desFilePath;
        }


        public override void ExtractFeature()
        {
            var pair = SplitData(source, des, numPerThread);
            sourceFiles = pair.first;
            desFiles = pair.second;
            var ThreadClasses = new List<IndividualFeature>(sourceFiles.Count);
            var threads = new List<Thread>(sourceFiles.Count);

            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var threadClass = new IndividualFeatureExtractor(sourceFiles[i], desFiles[i]);
                var thread = new Thread(threadClass.ExtractFeature);
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
