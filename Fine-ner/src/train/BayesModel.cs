using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using pml.file.writer;
using System.IO;
using System.Text.RegularExpressions;
using pml.type;

namespace msra.nlp.tr
{
    class BayesModel
    {        

        // Parameters for input

        // Parameters for output

        // Parameters for inner operation


        internal const string END = "###END###";
        private readonly string sourceFile = null;
        private readonly string modelFile = null;
        private Dictionary<string, Dictionary<string, Dictionary<string, int>>> statisticModel = null;
        private List<Pair<string, Dictionary<string, List<string>>>> developFeatures = null;

        public BayesModel(string sourceFile, string modelFile)
        {
            this.sourceFile = sourceFile;
            this.modelFile = modelFile;
        }

        /// <summary>
        ///     Train model from the train data
        /// </summary>
        /// <train>
        ///     List<Pair<label,Dictionar<field, values>>>
        /// </train>
        /// <returns>
        ///     A dictionary with format:
        ///     (label-->(field-->(feature values)))
        /// </returns>
        private void Statistic() 
        {
            var fields = GetFields(sourceFile);
            var reader = new LargeFileReader(sourceFile);
            var statisticModel = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
            var count = 0;

            while (true)
            {
                Pair<string, Dictionary<string, object>> trainItem = null;
                try
                {
                    trainItem = GetFeatureItem(reader, fields);
                }
                catch (Exception e)
                {
                    Console.WriteLine("line {0}"+e.Message);
                    continue;
                }
                if (trainItem == null)
                {
                    break;
                }
                if ((++count)%10000 == 0)
                {
                    Console.WriteLine(count);
                }
                var trainDicByField = trainItem.second; // field-->values or value
                Dictionary<string, Dictionary<string, int>> trainedDicByField = null; // a dic for a label
                try
                {
                    trainedDicByField = statisticModel[trainItem.first];
                }
                catch (Exception)
                {
                    trainedDicByField = new Dictionary<string, Dictionary<string, int>>();
                        // field-->(value-->time)           // updata dic
                }
                foreach (var field in fields)
                {
                    Dictionary<string, int> trainedDicByValue = null;
                    try
                    {
                        trainedDicByValue = trainedDicByField[field];
                    }
                    catch (Exception)
                    {
                        trainedDicByValue = new Dictionary<string, int>(); // updata valueDic
                    }
                    try
                    {
                        var values = trainDicByField[field];
                        foreach (string value in (IEnumerable)values)
                        {
                            try
                            {
                                trainedDicByValue[value] += 1;
                            }
                            catch (Exception)
                            {
                                trainedDicByValue[value] = 1;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Console.Error.WriteLine("Wrong format train data!");
                        continue;
                    }
                    if (!trainedDicByField.ContainsKey(field))
                    {
                        trainedDicByField[field] = trainedDicByValue;
                    }
                }
                if (!statisticModel.ContainsKey(trainItem.first))
                {
                    statisticModel[trainItem.first] = trainedDicByField;
                }
            }
            OutputModel(modelFile, statisticModel);
        }

        /// <summary>
        /// Train Bayes Model with some optimization method
        /// </summary>
        public void ReTrain()
        {
            var model = BayesTest.LoadModel("bayesStatisticModelFile");
            var w = new int[GetLabels().ToArray().Length];
            for(int i=0;i<w.Length;i++)  // Initial weight vector
            {
                w[i] = 1;
            }

        }

        List<string> labels = null;
        Dictionary<string, double> w = null;
        int positive = 0;

        public void Train()
        {
            this.statisticModel = BayesTest.LoadModel(@"D:\Codes\C#\EntityTyping\Fine-ner\unit test\output\model.txt");
            this.developFeatures = LoadBayesData(@"D:\Codes\C#\EntityTyping\Fine-ner\unit test\output\developFeature.txt");
            this.labels = GetLabels().ToList();
            this.w = new Dictionary<string, double>(this.labels.Count);
            foreach (var label in labels)  // Initial weight vector
            {
                w[label] = 1;

            }
            var learnSpeed = 0.005;
            var lastPositive = -1;
            this.positive = -1;
            int steps = 0;
            var lastWeight = new Dictionary<string, double>(w);
            while (lastPositive ==-1 || ((this.positive - lastPositive) > 0))
            {
                steps++;
                //learnSpeed /= steps;
                lastPositive = this.positive;
                var diff = GetDiff();
                lastWeight = new Dictionary<string, double>(w);
                foreach (var label in labels)
                {
                    w[label] -= diff[label] * learnSpeed;
                }
                Console.WriteLine("positive: " + positive);
                foreach (var label in labels)
                {
                    Console.Write(label + "\t" + w[label]+"\t");
                }
                Console.WriteLine("");
            }
            FileWriter writer = new LargeFileWriter(@"D:\Codes\C#\EntityTyping\Fine-ner\unit test\output\weight.txt", FileMode.Create);
            foreach(var label in labels)
            {
                writer.WriteLine(label + "\t" + lastWeight[label]);
            }
            writer.Close();
        }

        private Dictionary<string, double> GetDiff()
        {
            var labels = GetLabels().ToList();
            var diff = new Dictionary<string,double>(labels.Count);
            foreach(var label in labels)
            {
                diff[label] = 0;
            }
            this.positive = 0;
            foreach(var feature in this.developFeatures)
            {
                try
                {
                    var label = feature.first;
                    var scores = GetScores(feature.second);
                    var rank = GetRank(scores, label);
                    if (rank > 0)
                    {
                      for(var i=0;i<rank;i++)
                      {
                          diff[scores[i].first] += scores[i].second/(i+1);
                          diff[label] -= scores[rank].second/(i+1);
                      }
                    }
                    else
                    {
                        this.positive++;
                    }
                }
                catch
                {
                    continue;
                }
            }
            double maxDiff = 1;
            var keys = diff.Keys.ToList();
            foreach (var label in keys)
            {
                if (maxDiff < Math.Abs(diff[label]))
                {
                    maxDiff = Math.Abs(diff[label]);
                }
            }
            foreach (var label in keys)
            {
                diff[label] = diff[label] / maxDiff;
                Console.WriteLine(label + "\t" + diff[label]);
            }
            Console.ReadKey();
            return diff;
        }

        private int GetRank(List<Pair<string,double>> scores, string label)
        {
            var temp = new List<Pair<string, double>>(scores);
            for (var i = 0; i < temp.Count; i++)
            {
                temp[i].second *= this.w[temp[i].first];
            }
            var comparer = temp[0].GetBySecondReverseComparer();
            scores.Sort(comparer);
            for(var i=0;i<scores.Count;i++)
            {
               if(scores[i].first.Equals(label))
               {
                   return i;
               }
            }
            throw new Exception("Do not find label "+label+"in scores");
        }

        private List<Pair<string,double>> GetScores(Dictionary<string, List<string>> feature)
        {
            try
            {
                var fields = feature.Keys.ToList();
                var scores = new List<Pair<string, double>>(labels.Count);
                // Initial
                foreach (var label in this.labels)
                {
                    scores.Add(new Pair<string, double>(label, 0));
                }
                // Calculate score  for each label by field
                var times = new int[labels.Count];
                var i = 0;
                var total = 0;
                const int minLimit = 10;
                var min = -1;
                var max = -1;
                var valueNum = 0;     // count the element number of a field
                var scoresByField = new double[labels.Count];      // store the score of each label got in a field
                for (int k = 0; k < fields.Count-2; k++)
                {
                    var field = fields[k];
                    valueNum = 0;
                    for (var j = 0; j < labels.Count; j++)
                    {
                        scoresByField[j] = 0;
                    }
                    var values = (IEnumerable<string>)feature[field];
                    foreach (var value in values)
                    {
                        min = -1;
                        max = -1;
                        i = 0;
                        foreach (var label in labels)
                        {
                            try
                            {
                                times[i] = this.statisticModel[label][field][value];
                            }
                            catch (Exception)
                            {
                                times[i] = 1;
                            }
                            if (min > times[i] || min == -1)
                            {
                                min = times[i];
                            }
                            if (max < times[i] || max == -1)
                            {
                                max = times[i];
                            }
                            total += times[i];
                            i++;
                        }
                        if (total < minLimit && (max - min) < total * 0.5) continue;
                        for (var j = 0; j < times.Length; j++)
                        {
                            scoresByField[j] += Math.Log(1.0 * times[j] / total);
                        }
                        valueNum += 1;
                    }
                    if (valueNum > 0)
                    {
                        for (var j = 0; j < labels.Count; j++)
                        {
                            scores[j].second += scoresByField[j] / valueNum;
                        }
                    }
                }
                //for(i=0;i<scores.Count;i++)
                //{
                //    scores[i].second *= this.w[scores[i].first];
                //}
                var comparer = scores[0].GetBySecondReverseComparer();
                scores.Sort(comparer);
                return scores;
            }
            catch
            {
                throw new Exception("error");
            }
        }

        static readonly Regex labelRegex = new Regex("^[^\t]*");
        static readonly Regex fieldRegex = new Regex(@"\t([^:]*):{([^}]*)}");

        /// <summary>
        ///     It will read a line from the reader and parse the line into feature expression: Pair<string, Dictionary<string, object>>
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        internal static Pair<string, Dictionary<string, object>> GetFeatureItem(FileReader reader,
            ICollection<string> fields)
        {
            if (fields == null || reader == null)
            {
                throw new Exception("Exist null input in GetFeatureItem() function!");
            }
            var line = reader.ReadLine();
            if (line == null)
            {
                return null;
            }
            var pair = new Pair<string, Dictionary<string, object>>();
            var success = true;
            while (pair.second == null)
            {
                var dic = new Dictionary<string, object>(fields.Count);
                pair.first = labelRegex.Match(line).Value;
                var mc = fieldRegex.Matches(line);
                foreach (Match match in mc)
                {
                    if (!fields.Contains(match.Groups[1].Value))
                    {
                        success = false;
                        break;
                    }
                    dic[match.Groups[1].Value] = match.Groups[2].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                if (success)
                {
                    pair.second = dic;
                }
            }
            return pair.second == null ? null : pair;
        }

        /// <summary>
        ///        Store the trained model into file
        /// </summary>
        /// <param name="desPath"></param>
        /// <param name="array"></param>
        /// <format>
        ///     [Label]
        ///     [field name]
        ///     TAB [feature annotation,e.g., last word]    TAB     [times]
        ///     TAB [feature annotation,e.g., last word]    TAB     [times]
        ///     ###END###
        /// </format>
        internal static void OutputModel(string desPath, object model )
        {
            var writer = new LargeFileWriter(desPath, FileMode.Create);
            var dics = (Dictionary<string, Dictionary<string, Dictionary<string, int>>>)model;

            foreach(var label in dics.Keys)      // Check !
            {
                var dic = dics[label];    // fields-->dic<feature value, times>
                foreach(var field in dic.Keys )
                {
                    writer.WriteLine(label);
                    writer.WriteLine(field);     // write field
                    var featureDic = dic[field];
                    var keys = Feature.SortKeysByNum(featureDic);
                    foreach (var featureValue in keys)
                    {
                        writer.WriteLine("\t"+ featureValue +"\t"+ featureDic[featureValue]);
                    }
                }
            }
            writer.Close();
        }

        /// <summary>
        ///       Load train data from file
        /// </summary>
        /// <param name="sourceFile">
        ///       File path of the train data
        /// </param>
        /// <format>
        ///     [Label]   TAB     [FieldName]:{[value1],[value2]...}    TAB     [FieldName]:{[value1],[value2]...}  ...
        /// </format>
        /// <returns>
        ///     List of object(actually a dictionary)
        ///     [class label]-->[field name-->list of values]
        /// </returns>
        internal static List<Pair<string, Dictionary<string, List<string>>>> LoadBayesData(string sourceFile)
        {
            FileReader reader = new LargeFileReader(sourceFile);
            string line;
            var pairs = new List<Pair<string, Dictionary<string, List<string>>>>();
            var labelRegex = new Regex("^[^\t]*");
            var fieldRegex = new Regex(@"\t([^:]*):{([^}]*)}");
            var fields = GetFields(sourceFile);
            var count = 0;

            while ((line = reader.ReadLine())!=null)
            {
                count++;
                var pair = new Pair<string, Dictionary<string, List<string>>>();
                var dic = new Dictionary<string, List<string>>(fields.Count);
                pair.first = labelRegex.Match(line).Value;
                var mc = fieldRegex.Matches(line);
                foreach (Match match in mc)
                {
                    if (!fields.Contains(match.Groups[1].Value))
                    {
                        Console.WriteLine("Invalid format in line{0}", count);
                        continue;
                    }
                    dic[match.Groups[1].Value] = match.Groups[2].Value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                pair.second = dic;
                pairs.Add(pair);
            }
            reader.Close();
            return pairs;
        }

        internal static HashSet<string> GetFields(string sourceFile)
        {
            FileReader reader = new LargeFileReader(sourceFile);
            string line;
            var count = 0;
            var dic = new Dictionary<string, int>();

            while ((line = reader.ReadLine()) != null)
            {
                if (++count > 100)
                {
                    break;
                }
                var mc = fieldRegex.Matches(line);
                foreach (Match match in mc)
                {
                    try
                    {
                        dic[match.Groups[1].Value] += 1;
                    }
                    catch (Exception)
                    {
                        dic[match.Groups[1].Value] = 1;
                    }
                }
            }
            reader.Close();
            var fields = new HashSet<string>();
            foreach (var key in dic.Keys.Where(key => (1.0*dic[key]/count) > 0.95))
            {
                fields.Add(key);
            }
            return fields;
        } 
        
        internal IEnumerable<string> GetLabels()
        {
            return statisticModel.Keys;
        }
    }
}
