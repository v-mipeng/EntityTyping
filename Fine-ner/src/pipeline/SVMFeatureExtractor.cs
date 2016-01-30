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
        EventWriter writer = null;
        SVMFeature extractor = null;

        public SVMFeatureExtractor(string sourceFilePath, string desFilePath, bool HasHead = false)
        {
            reader = new EventReaderByLine(sourceFilePath, HasHead);
            writer = new EventWriterByLine(desFilePath);
            extractor = new SVMFeature();
        }

        public override void ExtractFeature()
        {
            var count = 0;
            var dic = new Dictionary<string, int>();
            foreach (var type in types)
            {
                dic[type] = dic.Count;
            }

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
                     writer.WriteEvent(new Event(new Label(dic[e.Label.StringLabel].ToString()), feature));
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

        static string[] types = new string[] {
                "music.music",
                "broadcast.content",
                "book.written_work",
                "award.award",
                "body.part",
                "chemicstry.chemistry",
                "time.event",
                "food.food",
                "language.language",
                "location.location",
                "organization.organization",
                "people.person",
                "computer.software",
                "commerce.electronics_product",
                "commerce.consumer_product",
        };

    }
}
