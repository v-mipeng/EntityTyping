using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    /// <summary>
    /// Extract SVM feature for instances in source file and store the result into desfile
    /// </summary>
    class SVMFeatureExtractor  : FeatureExtractor
    {
        InstanceReader reader = null;
        EventWriter writer = null;
        SVMFeature extractor = null;

        public SVMFeatureExtractor(string sourceFilePath, string desFilePath)
        {
            reader = new InstanceReaderByLine(sourceFilePath);
            writer = new EventWriterByLine(desFilePath);
            extractor = new SVMFeature();
        }

        public override void ExtractFeature()
        {
             while(reader.HasNext())
             {
                 var instance = reader.GetNextInstance();
                 try
                 {
                     var feature = extractor.ExtractFeature(instance);
                     var e = new Event(instance.Label, feature);
                     writer.WriteEvent(e);
                 }
                 catch(Exception e)
                 {
                     Console.WriteLine(e.Message);
                 }
             }
             reader.Close();
             writer.Close();
        }



    }
}
