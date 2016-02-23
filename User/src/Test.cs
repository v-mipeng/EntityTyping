
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.MachineLearning;
using Microsoft.MachineLearning.CommandLine;
using Microsoft.MachineLearning.Learners;
using Microsoft.MachineLearning.Model;
using Microsoft.TMSN.TMSNlearn;
using Microsoft.MachineLearning.Data;
using msra.nlp.tr.predict;
using pml.type;
using System.IO;

namespace msra.nlp.tr
{
    public class Test
    {
       
        public static void Main(string[] args)
        {
            // Set resource file pathes
            Property props = new Property();
            var currentFolderPath = Environment.CurrentDirectory;
            var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
            var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
            basedir = Path.Combine(basedir, "Fine-ner");
            props.Set("tanford_model_dir", Path.Combine(basedir, @"input\stanford models"));   // set stanford model files path
            props.Set("opennlp_model_dir",Path.Combine(basedir, @"input\opennlp models"));     // set opennlp model files path
            props.Set("word_table_file", Path.Combine(basedir, @"input\tables\wordTable.txt"));// set word table file path
            props.Set("Field.stem_map",Path.Combine(basedir, @"input\tables\stem-word-table.txt"));
            props.Set("word_shape_table_file", Path.Combine(basedir, @"input\tables\shape-table.txt"));
            props.Set("posTag_table_file", Path.Combine(basedir, @"input\tables\pos-tag-table.txt"));
            props.Set("word_id_file", Path.Combine(basedir, @"input\tables\wordID.txt"));
            props.Set("mention_id_file", Path.Combine(basedir, @"input\tables\mentionID.txt"));
            props.Set("model_file", Path.Combine(basedir, @"output\model\logistic model.zip"));
            props.Set("dbpedia_dic_file", Path.Combine(basedir, @"input\dictionaries\dbpedia\dbpedia entity type.txt"));
            props.Set("dbpedia_redirect_file", Path.Combine(basedir, @"input\dictionaries\dbpedia\redirects.txt"));
            props.Set("disambiguous_file", Path.Combine(basedir, @"input\dictionaries\disambiguation mapping.txt"));
            props.Set("page_anchor_file", Path.Combine(basedir, @"input\dictionaries\page anchor.txt"));
            props.Set("keyword_file",  Path.Combine(basedir, @"input\tables\keywords.txt"));
            var pipeline = new Pipeline(props);
            // Make prediction
            var mention = "Tony Award for Best Lighting Design";
            var context = "Gallo has received eight nominations for the Tony Award for Best Lighting Design and ten nominations for the Drama Desk Award for Outstanding Lighting Design, which he won for the 1992 revival of Guys and Dolls. He won the Henry Hewes Design Award, Collaborative Design Achievement-Lighting Design for the Public Theater production of Vienna: Lusthaus in 1986, and was nominated for Hewes Design Award, Lighting Design, for The Crucible (2002).";
            var predictor = new FullFeaturePredictor();
            var label = predictor.Predict(mention, context);
            Console.WriteLine(label);
        }
    }
}

