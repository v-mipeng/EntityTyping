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
    class Debug
    {
        static void Mains(string[] args)
        {
            var sspliter = new SentenceSpliter(@"D:\Codes\Project\EntityTyping\Fine-ner\input\stanford models");
            var sentences = sspliter.SplitSequence("I like Beijing(China). J. Smith went with me.");
            Run();
        }

        public static void Run()
        {
            var props = new Property();
            Pipeline pipeline = null;
            /************************************************************************/
            /* Feature extractor  for satori                                                                   */
            /************************************************************************/
            if (true)
            {
                var currentFolderPath = Environment.CurrentDirectory;
                var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
                var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
                basedir = Path.Combine(basedir, "Fine-ner/");
                var configFile = Path.Combine(basedir, "config.xml");
                props.SetProperty("method", @"/ef -raw -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"input\datasets\satori\train\award_award.txt"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"input\features\satori\temp train\award_award.txt"));
                props.SetProperty("develop_data_file", Path.Combine(basedir, @"input\features\satori\develop\"));
                props.SetProperty("develop_feature_file", Path.Combine(basedir, @"input\features\satori\temp develop\"));
                props.SetProperty("test_data_file", Path.Combine(basedir, @"input\features\satori\test\"));
                props.SetProperty("test_feature_file", Path.Combine(basedir, @"input\features\satori\temp test\"));
                pipeline = new Pipeline(configFile, props);
                pipeline.Execute();

                #region
                //props.SetProperty("method", @"/ef -svm -all");
                //props.SetProperty("train_data_file", Path.Combine(basedir, @"input\features\satori\temp train\"));
                //props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\features\satori\train\"));
                //props.SetProperty("develop_data_file", Path.Combine(basedir, @"input\features\satori\temp develop\"));
                //props.SetProperty("develop_feature_file", Path.Combine(basedir, @"output\features\satori\develop\"));
                //props.SetProperty("test_data_file", Path.Combine(basedir, @"input\features\satori\temp test\"));
                //props.SetProperty("test_feature_file", Path.Combine(basedir, @"output\satori feature\test\"));
                ////props.SetProperty("method", @"/ef -raw -add -test");
                ////props.SetProperty("train_data_file", Path.Combine(basedir, @"input\feature\train\"));
                ////props.SetProperty("train_feature_file", Path.Combine(basedir, @"input\feature\temp train\"));
                ////props.SetProperty("develop_data_file", Path.Combine(basedir, @"input\feature\develop\"));
                ////props.SetProperty("develop_feature_file", Path.Combine(basedir, @"input\feature\temp develop\"));
                ////props.SetProperty("test_data_file", Path.Combine(basedir, @"input\feature\test\"));
                ////props.SetProperty("test_feature_file", Path.Combine(basedir, @"input\feature\temp test\"));
                ////pipeline = new Pipeline(props);
                ////pipeline.Execute();
                //props.SetProperty("method", @"/ef -svm -train");
                //props.SetProperty("train_data_file", Path.Combine(basedir, @"input\satori+conll\"));
                //props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\satori+conll\"));
                //props.SetProperty("develop_data_file", Path.Combine(basedir, @"input\feature\temp develop\"));
                //props.SetProperty("develop_feature_file", Path.Combine(basedir, @"output\svm\develop\"));
                //props.SetProperty("test_data_file", Path.Combine(basedir, @"input\feature\temp test\"));
                //props.SetProperty("test_feature_file", Path.Combine(basedir, @"output\svm\test\"));
                //props.SetProperty("test_feature_file", Path.Combine(basedir, @"output\features\satori\test\"));
                ////props.Set("activateMIKeyword", false);
                ////props.Set("activateDbpedia", false);
                //props.Set("activateNer", false);
                //props.Set("activateParser", false);
                //pipeline = new Pipeline(props);
                //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\satori feature\train\"), Path.Combine(basedir, @"output\satori feature\train.txt"));
                //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\satori feature\develop\"), Path.Combine(basedir, @"output\satori feature\develop.txt"));
                //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\satori feature\test\"), Path.Combine(basedir, @"output\satori feature\test.txt"));
                //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\features\satori\train\"), Path.Combine(basedir, @"output\satori feature\train.txt"));
                //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\features\satori\develop\"), Path.Combine(basedir, @"output\satori feature\develop.txt"));
                //pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\features\satori\test\"), Path.Combine(basedir, @"output\satori feature\test.txt"));
                #endregion
            }
            /************************************************************************/
            /* Feature extractor  for conll                                                                   */
            /************************************************************************/
            if (false)
            {
                var currentFolderPath = Environment.CurrentDirectory;
                var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
                var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
                basedir = Path.Combine(basedir, "Fine-ner/");
                props.SetProperty("method", @"/ef -raw -add -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"input\features\conll\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"input\features\temp\"));
                pipeline = new Pipeline(props);
                pipeline.Execute();
                props.SetProperty("method", @"/ef -svm -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"input\features\temp\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\features\conll\"));
                //props.Set("activateMIKeyword", false);
                props.Set("activateNer", false);
                props.Set("activateParser", false);
                pipeline = new Pipeline(props);
                pipeline.Execute();
            }
            /************************************************************************/
            /* Feature extractor  for bbn                                                                */
            /************************************************************************/
            if (false)
            {
                var currentFolderPath = Environment.CurrentDirectory;
                var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
                var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
                basedir = Path.Combine(basedir, "Fine-ner/");
                props.SetProperty("method", @"/ef -raw -add -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"input\bbn\individual\backup\with ners dbpedia-abstrtact-indegree keyword\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"input\bbn\individual\"));
                pipeline = new Pipeline(props);
                pipeline.Execute();
                props.SetProperty("method", @"/ef -svm -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"input\bbn\individual\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\bbn\backup\with ners dbpedia-abstract-indegree keyword\"));
                pipeline = new Pipeline(props);
                pipeline.Execute();
                props.SetProperty("method", @"/ef -svm -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"input\bbn\individual\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\bbn\backup\with ners keyword\"));
                props.Set("activateDbpedia", false);
                pipeline = new Pipeline(props);
                pipeline.Execute();
                props.SetProperty("method", @"/ef -svm -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"input\bbn\individual\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\bbn\backup\with ners dbpedia-abstract-indegree\"));
                props.Set("activateMIKeyword", false);
                pipeline = new Pipeline(props);
                pipeline.Execute();
                props.SetProperty("method", @"/ef -svm -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"input\bbn\individual\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\bbn\backup\base-parser ners dbpedia-abstract-indegree keyword\"));
                props.Set("activateParser", false);
                pipeline = new Pipeline(props);
                pipeline.Execute();
                props.SetProperty("method", @"/ef -svm -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"input\bbn\individual\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\bbn\backup\base-parser with dbpedia-abstract-indegree keyword\"));
                props.Set("activateNer", false);
                props.Set("activateParser", false);
                pipeline = new Pipeline(props);
                pipeline.Execute();
            }
            /************************************************************************/
            /* Feature extractor  for satori and conll                                                                   */
            /************************************************************************/
            if(false)
            {
                var currentFolderPath = Environment.CurrentDirectory;
                var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
                var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
                basedir = Path.Combine(basedir, "Fine-ner/");
                var satoriTrainDir = Path.Combine(basedir, "output/features/satori/train/");
                var satoriTestDir = Path.Combine(basedir, "output/features/satori/develop/");
                var satoriDevDir = Path.Combine(basedir, "output/features/satori/test/");
                var conllTrainFile = Path.Combine(basedir, "output/features/conll/train.txt");
                var conllTestFile = Path.Combine(basedir, "output/features/conll/develop.txt");
                var conllDevFile = Path.Combine(basedir, "output/features/conll/test.txt");
                var desTrainFile = Path.Combine(basedir, "output/features/satori+conll/train.txt");
                var desTestFile = Path.Combine(basedir, "output/features/satori+conll/develop.txt");
                var desDevFile = Path.Combine(basedir, "output/features/satori+conll/test.txt");
                var reader = new LargeFileReader();
                var writer = new LargeFileWriter();

                #region Satori+Conll Train
                {
                    var files = Directory.GetFiles(satoriTrainDir);
                    writer.Open(desTrainFile, FileMode.Create);
                    foreach (var file in files)
                    {
                        var text = File.ReadAllText(file);
                        writer.Write(text);
                        if (Path.GetFileNameWithoutExtension(file).Contains("electronic"))
                        {
                            for (var i = 0; i < 4; i++)      // copy 4 times
                            {
                                writer.Write(text);
                            }
                        }
                    }
                    {
                        var text = File.ReadAllText(conllTrainFile);
                        writer.Write(text);
                        writer.Close();
                    }
                }
                #endregion

                #region Satori+Conll Develop
                {
                    var files = Directory.GetFiles(satoriDevDir);
                    writer.Open(desDevFile, FileMode.Create);
                    foreach (var file in files)
                    {
                        var text = File.ReadAllText(file);
                        writer.Write(text);
                    }
                    {
                        var text = File.ReadAllText(conllDevFile);
                        writer.Write(text);
                        writer.Close();
                    }
                }
                #endregion

                #region Satori+Conll Test
                {
                    var files = Directory.GetFiles(satoriTestDir);
                    writer.Open(desTestFile, FileMode.Create);
                    foreach (var file in files)
                    {
                        var text = File.ReadAllText(file);
                        writer.Write(text);
                    }
                    {
                        var text = File.ReadAllText(conllTestFile);
                        writer.Write(text);
                        writer.Close();
                    }
                }
                #endregion

            }
            /************************************************************************/
            /* Feature extractor  for 5 classes: product and other                                                                   */
            /************************************************************************/
            if (false)
            {
                var currentFolderPath = Environment.CurrentDirectory;
                var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
                var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
                basedir = Path.Combine(basedir, "Fine-ner/");
                var configFile = Path.Combine(basedir,"config.xml");
                props.SetProperty("method", @"/ef -svm -train");
                props.SetProperty("train_data_file", Path.Combine(basedir, @"input\features\5 class\train\"));
                props.SetProperty("train_feature_file", Path.Combine(basedir, @"output\features\5 class\train\"));
                props.Set("activateNer", false);
                props.Set("activateParser", false);
                pipeline = new Pipeline(configFile, props);
                pipeline.Execute();
                pml.file.util.Util.CombineFiles(Path.Combine(basedir, @"output\features\5 class\train\"), Path.Combine(basedir, @"output\features\5 class\train.txt"));

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
        }
    }
}
