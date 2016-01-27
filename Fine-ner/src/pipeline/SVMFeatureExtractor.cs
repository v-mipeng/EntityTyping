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
        EventReader reader = null;
        EventWriter writer = null;
        SVMFeature extractor = null;

        public SVMFeatureExtractor(string sourceFilePath, string desFilePath)
        {
            reader = new EventReaderByLine(sourceFilePath, true);
            writer = new EventWriterByLine(desFilePath);
            extractor = new SVMFeature();
        }

        public override void ExtractFeature()
        {
             while(reader.HasNext())
             {
                 var e = reader.GetNextEvent();
                 try
                 {
                     var feature = extractor.ExtractFeature(e);
                     writer.WriteEvent(new Event(e.Label, feature));
                 }
                 catch(Exception ex)
                 {
                     Console.WriteLine(ex.Message);
                 }
             }
             reader.Close();
             writer.Close();
        }

    }
}
