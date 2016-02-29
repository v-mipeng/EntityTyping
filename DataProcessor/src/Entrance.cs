using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using msra.nlp.tr;

namespace msra.nlp.tr.dp
{
    class Entrance
    {
        public static void Main(string[] args)
        {
            if (false)
            {
                var refiner = new DataRefiner(@"E:\Users\v-mipeng\Data\Satori\Raw\Interlink.stype.tsv",
              @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori ontology\satori-hierarchy.txt",
              @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\refined-satori\",
              @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\refined-satori\statisticInfo.txt");
                refiner.Refine();
            }
            if (false)
            {
                DataSpliter spliter = new DataSpliter(
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\refined-satori\",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\statisticInfo.txt",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\train\",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\develop\",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\test\",
                @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\train\train-data-info.txt");
                spliter.SplitData();
            }
            if (false)
            {
                var sourceDir = @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\train\";
                var desDir = @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\tmp\";
                var selector = new KeyWordSelector(sourceDir, desDir);
                selector.GetKeyWords();
            }
            if(true)
            {
                var source = @"D:\Data\DBpedia\disambiguations_en.nt";
                var des = @"D:\Data\DBpedia\disambiguation mapping.txt";
                DBpediaProcessor.RefineAmbiguousItem(source, des);
            }
            if(true)
            {
                TfIdf tfidf = new TfIdf(
                @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract.txt",
                @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract vector.txt",
                @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract df.txt",
                @"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract word table.txt");
                tfidf.GetVectorCorpus();
            }
        }
    }
}
