using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    /// <summary>
    /// Extract SVM feature for instances in source file and store the result into desfile
    /// </summary>
    class SVMFeatureExtractor  : FeatureExtractor
    {
        EventReader reader = null;
        string source = null;
        EventWriter writer = null;
        SVMFeature extractor = null;

        public SVMFeatureExtractor(string sourceFilePath, string desFilePath, bool HasHead = false)
        {
            source = sourceFilePath;
            reader = new EventReaderByLine(sourceFilePath, HasHead);
            writer = new EventWriterByLine(desFilePath);
            extractor = new SVMFeature();
        }

        public override void ExtractFeature()
        {
            var count = 0;

            while (reader.HasNext())
             {
                if(++count % 1000 ==0)
                {
                    Console.WriteLine("{0} has processed {1}", Thread.CurrentThread.Name, count);
                }
                 var e = reader.GetNextEvent();
                 try
                 {
                     var feature = extractor.ExtractFeature(e);
                     writer.WriteEvent(new Event(new Label(Parameter.GetTypeLabel(e.Label.StringLabel).ToString()), feature));
                 }
                 catch(Exception ex)
                 {
                     Console.WriteLine(ex.Message);
                     Console.WriteLine(ex.StackTrace);
                 }
             }
             reader.Close();
             writer.Close();
        }
       
    }
}
