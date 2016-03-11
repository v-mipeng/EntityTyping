using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using pml.file.writer;
using System.IO;
using pml.type;
using msra.nlp.tr;

namespace User
{
    class UnitTest
    {
        static void Main(string[] args)
        {
            //Temp();
            //Analyse();
            Start();
            //Statistic.Refresh();  // test tfs
        }

        public static void Analyse()
        {
            // String basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input";
            String reportFile = @" E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\output\report.txt";
            FileWriter writer = new LargeFileWriter(reportFile, FileMode.Append);
            //// statistic train data number by type and coverage of UIUC by type
            //String dicCovReport = Statistic.StatisticDicCoverage(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\UIUC.txt", basedir + @"\train\train.txt");
            //writer.WriteLine("Coverage of UIUC within train data:\r"+dicCovReport);
            //writer.WriteLine("");
            //basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori";
            //// statistic satori develop data number by type and coverage of UIUC by type
            //dicCovReport = Statistic.StatisticDicCoverage(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\UIUC.txt", basedir + @"\develop.txt");
            //writer.WriteLine("Coverage of UIUC within satori develop data:\r" + dicCovReport);
            //writer.WriteLine("");
            //// statistic satori test data number by type and coverage of UIUC by type
            //dicCovReport = Statistic.StatisticDicCoverage(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\UIUC.txt", basedir + @"\test.txt");
            //writer.WriteLine("Coverage of UIUC within satori test data:\r" + dicCovReport);
            //writer.WriteLine("");
            //basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori_lc";
            //// statistic satori_lc develop data number by type and coverage of UIUC by type
            //dicCovReport = Statistic.StatisticDicCoverage(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\UIUC.txt", basedir + @"\develop.txt");
            //writer.WriteLine("Coverage of UIUC within satori_lc develop data:\r" + dicCovReport);
            //writer.WriteLine("");
            //// statistic satori_lc test data number by type and coverage of UIUC by type
            //dicCovReport = Statistic.StatisticDicCoverage(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\UIUC.txt", basedir + @"\test.txt");
            //writer.WriteLine("Coverage of UIUC within satori_lc test data:\r" + dicCovReport);
            //writer.WriteLine("");

            string basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input";
            // // statistic co-occurrence rate between train and satori develop data
            //string corate = Statistic.StatisticCooccurrence(basedir + @"\train\train.txt", basedir + "/satori/develop.txt");
            //writer.WriteLine("Co-occurrence rate between train and satori develop data is:\r "+corate);
            //writer.WriteLine("");
            //// statistic co-occurrence rate between train and satori test data
            //corate = Statistic.StatisticCooccurrence(basedir + @"\train\train.txt", basedir + "/satori/test.txt");
            //writer.WriteLine("Co-occurrence rate between train and satori test data is:\r " + corate);
            //writer.WriteLine("");
            //// statistic co-occurrence rate between train and satori_lc develop data
            //corate = Statistic.StatisticCooccurrence(basedir + @"\train\train.txt", basedir + "/satori_lc/develop.txt");
            //writer.WriteLine("Co-occurrence rate between train and satori_lc develop data is:\r " + corate);
            //writer.WriteLine("");
            //// statistic co-occurrence rate between train and satori_lc test data
            //corate = Statistic.StatisticCooccurrence(basedir + @"\train\train.txt", basedir + "/satori_lc/test.txt");
            //writer.WriteLine("co-occurrence rate between train and satori_lc test data is:\r " + corate);
            //statistic name list coverage
            //String report = Statistic.StatisticNameListCoverageByType(@"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\dictionary\name-list.txt", @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\train\limited train.txt");
            //writer.WriteLine("Name list coverage by type is :\r " + report);
            // statistic item number by type
            //writer.WriteLine("Item number by type:\r" + Statistic.StatisticItemNumberByType(basedir + @"\train\train.txt"));
            //writer.WriteLine(Statistic.StatisticRoundTokenInformation(basedir + @"\train\train.txt"));
            //writer.WriteLine(Statistic.StatisticWithinTokenInfomation(basedir + @"\train\train.txt"));
            writer.Close();
        }

        public static void Start()
        {
            var props = new Property();
            Pipeline pipeline = null;
            /************************************************************************/
            /* Feature extractor                                                                     */
            /************************************************************************/
            if (true)
            {
                var currentFolderPath = Environment.CurrentDirectory;
                var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
                var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
                basedir = Path.Combine(basedir, "Fine-ner/");
                props.SetProperty("method", @"/ef -raw -add -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"output\conll feature\raw\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\conll feature\temp raw\"));
                props.SetProperty("develop_data_file", Path.Combine(basedir, @"input\feature\develop\"));
                props.SetProperty("develop_feature_file", Path.Combine(basedir, @"input\feature\temp develop\"));
                props.SetProperty("test_data_file", Path.Combine(basedir, @"input\feature\test\"));
                props.SetProperty("test_feature_file", Path.Combine(basedir, @"input\feature\temp test\"));
                pipeline = new Pipeline(props);
                pipeline.Execute();
                props.SetProperty("method", @"/ef -svm -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"output\conll feature\temp raw\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\conll feature\svm\"));
                props.SetProperty("develop_data_file", Path.Combine(basedir, @"input\feature\temp develop\"));
                props.SetProperty("develop_feature_file", Path.Combine(basedir, @"output\svm\develop\"));
                props.SetProperty("test_data_file", Path.Combine(basedir, @"input\feature\temp test\"));
                props.SetProperty("test_feature_file", Path.Combine(basedir, @"output\svm\test\"));
                //props.Set("activateMIKeyword", false);
                pipeline = new Pipeline(props);
                pipeline.Execute();
                //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\svm\train\"), Path.Combine(basedir, @"output\svm\train.txt"));
                //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\svm\develop\"), Path.Combine(basedir, @"output\svm\develop.txt"));
                //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\svm\test\"), Path.Combine(basedir, @"output\svm\test.txt"));
            }
            /************************************************************************/
            /* Bayes train and test                                                                     */
            /************************************************************************/
            if (false)
            {
                props.SetProperty("method", @"/ts -b");
                //props.SetProperty("train_feature_file", @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\unit test\output\trainFeature.txt");
                props.SetProperty("develop_feature_file", @"D:\Codes\C#\EntityTyping\Fine-ner\unit test\output\testFeature.txt");
                props.SetProperty("model_file", @"D:\Codes\C#\EntityTyping\Fine-ner\unit test\output\model.txt");
                props.SetProperty("result_file", @"D:\Codes\C#\EntityTyping\Fine-ner\unit test\output\result.txt");
                
                pipeline = new Pipeline(props);
                pipeline.Execute();
            }
            /************************************************************************/
            /* Extract word table and word shapes                                                                     */
            /************************************************************************/
            if (false)
            {
                props.SetProperty("method", @"/ewt");
                //props.SetProperty("train_data_file", @"");
                //props.SetProperty("word_table_file", @"");
                //props.SetProperty("word_shape_table_file", "");
                pipeline = new Pipeline(props);
                pipeline.Execute();
            }
            /************************************************************************/
            /* Predict with hierarchy model                                                                     */
            /************************************************************************/                                                          
            if (false)
            {
                pipeline = new Pipeline();
                pipeline.Predict(@"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\test.txt", @"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\inst.txt");
                pipeline.Evaluate(@"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\inst.txt", @"D:\Codes\Project\EntityTyping\Fine-ner\input\feature\result.txt");
            }

        }
    }
}
