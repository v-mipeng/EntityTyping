using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using pml.type;

namespace msra.nlp.tr
{
    abstract class ParallelFeatureExtractor
    {

        public abstract void ExtractFeature();

        public void AddFeature() { }

        public Pair<List<string>, List<string>> SplitData(string source, string des, int numPerThread)
        {
            var reader = new InstanceReaderByLine(source);
            var writer = new InstanceWriterByLine(des);

            var directory =Path.GetDirectoryName(source);
            var name = Path.GetFileNameWithoutExtension(source);
            var ext = Path.GetExtension(source);
            // seperate source file into parts
            int part = 0;
            var partFile = Path.Combine(directory, name + "-part" + part + ext);
            var sourceFiles = new List<string>();
            sourceFiles.Add(partFile);
            // Create corresponding des files
            string desPartFile = null;
            var desFiles = new List<string>();
            writer = new InstanceWriterByLine(partFile);

            Instance instance = null;
            int count = 0;

            while (reader.HasNext())
            {
                try
                {
                    instance = reader.GetNextInstance();
                }
                catch(Exception)
                {
                    continue;
                }
                if (++count < numPerThread)
                {
                    writer.WriteInstance(instance);
                }
                else
                {
                    writer.Close();
                    // add des path to desfiles
                    desPartFile = Path.Combine(directory, name + "-feature-part" + part + ext);
                    desFiles.Add(desPartFile);
                    // create another part file
                    part++;
                    partFile = Path.Combine(directory, name + "-part" + part + ext);
                    writer = new InstanceWriterByLine(partFile);
                    count = 0;
                    sourceFiles.Add(partFile);
                }
            }
            if (count > 0)
            {
                writer.Close();
                desPartFile = Path.Combine(directory, name + "-feature-part" + part + ext);
                desFiles.Add(desPartFile);
            }
            else
            {
                writer.Close();
                sourceFiles.Remove(partFile);
            }
            reader.Close();
            return new Pair<List<string>, List<string>>(sourceFiles, desFiles);
        }
 

    }
}
