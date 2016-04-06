using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using msra.nlp.tr;
using msra.nlp.dp.conll;
using msra.nlp.tr.dp.satori;

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
                //var sourceDir = @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\train\";
                //var desDir = @"E:\Users\v-mipeng\Codes\Projects\EntityTyping\Fine-ner\input\satori\tmp\";
                //var selector = new KeyWordSelector(sourceDir, desDir);
                //selector.GetKeyWords();
            }
            if(false)
            {
                var source = @"D:\Data\DBpedia\disambiguations_en.nt";
                var des = @"D:\Data\DBpedia\disambiguation mapping.txt";
                DBpediaProcessor.RefineAmbiguousItem(source, des);
            }
            if(false)
            {
                //TfIdf tfidf = new TfIdf(
                //@"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract.txt",
                //@"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract vector.txt",
                //@"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract df.txt",
                //@"D:\Codes\Project\EntityTyping\Fine-ner\input\dictionaries\dbpedia\abstract word table.txt");
                //tfidf.GetVectorCorpus();
            }
            if(false)
            {
                // filter conll data
                var filter = new DataFilter(@"D:\Codes\Project\EntityTyping\Fine-ner\input\conll",
                     @"D:\Codes\Project\EntityTyping\Fine-ner\input\conll trimed\",
                     @"D:\Codes\Project\EntityTyping\Fine-ner\input\conll trimed\conll info.txt");
                filter.Refine();
            }
             if(true)
             {
                Script.AddProductData(@"D:\Codes\Project\EntityTyping\Fine-ner\input\datasets\satori\computer_software.txt",
                     @"D:\Codes\Project\EntityTyping\Fine-ner\input\datasets\satori\train\computer_software.txt");
                Script.AddProductData(@"D:\Codes\Project\EntityTyping\Fine-ner\input\datasets\satori\commerce_electronics_product.txt",
                   @"D:\Codes\Project\EntityTyping\Fine-ner\input\datasets\satori\train\commerce_electronics_product.txt");
                Script.AddProductData(@"D:\Codes\Project\EntityTyping\Fine-ner\input\datasets\satori\commerce_consumer_product.txt",
                                    @"D:\Codes\Project\EntityTyping\Fine-ner\input\datasets\satori\train\commerce_consumer_product.txt");

             }
        }
    }
}
