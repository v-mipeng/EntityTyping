using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using ikvm.extensions;
using pml.file.reader;
using pml.file.writer;
using pml.type;

namespace msra.nlp.tr
{
    class BayesTest
    {
        Dictionary<string, Dictionary<string, Dictionary<string, int>>> model = null;
        List<string> labels = null;
        List<Pair<string, double>> scores = null;
        private readonly string modelFile = null;
        readonly string sourceFile = null;
        readonly string resultFile = null;
        Dictionary<string, double> w = null;
 
        public BayesTest(string modelFile, string sourceFile, string resultFile)
        {
            this.modelFile = modelFile;
            this.sourceFile = sourceFile;
            this.resultFile = resultFile;
        }

        private void Initial()
        {
            this.model = LoadModel(this.modelFile);
            this.labels = GetLabels();
        }


        public void Test()
        {
            if (model == null)
            {
                Initial();
            }
            var fields = BayesModel.GetFields(sourceFile);
            FileReader reader = new LargeFileReader(sourceFile);
            FileWriter writer = new LargeFileWriter(resultFile, FileMode.Create);
            // actual label-->(prediced label-->times)
            var detailDic = new Dictionary<string, Dictionary<string, int>>();
            var positiveNums = new Dictionary<string, int>(); // positive number by type
            var predictedNums = new Dictionary<string, int>(); // predicted number by type
            var actualNums = new Dictionary<string, int>(); //  actual number by type
            Dictionary<string, int> dic = null;
            Pair<string, Dictionary<string, object>> feature = null;
            var i = 0;

            while ((feature = BayesModel.GetFeatureItem(reader, fields)) != null)
            {
                i++;
                var label = feature.first;
                string predictedLabel = null;
                try
                {
                    predictedLabel = Predict(feature.second);
                }
                catch (Exception)
                {
                    Console.WriteLine("Wrong!");
                    writer.WriteLine(i + "\t" + label + "\tNULL");
                    continue;
                }
                writer.Write(string.Format("{0}\t{1, -30}", i, label));
                foreach (var score in this.scores)
                {
                    writer.Write(string.Format("{0,30}:{1,-10:F2}", score.first, score.second));
                }
                writer.Write("\r");

                if (label.Equals(predictedLabel))
                {
                    try
                    {
                        positiveNums[label] += 1;
                    }
                    catch (Exception)
                    {
                        positiveNums[label] = 1;
                    }
                }
                try
                {        // update predicted number
                    predictedNums[predictedLabel] += 1;
                }
                catch (Exception)
                {
                    predictedNums[predictedLabel] = 1;
                }
                try
                {    // update actually number
                    actualNums[label] += 1;
                }
                catch (Exception)
                {
                    actualNums[label] = 1;
                }
                // update detail dictionary
                try
                {
                    dic = detailDic[label];
                }
                catch (Exception)
                {
                    dic = new Dictionary<string, int>();
                    detailDic[label] = dic;
                }
                try
                {
                    dic[predictedLabel] += 1;
                }
                catch (Exception)
                {
                    dic[predictedLabel] = 1;
                }
            }
            var buffer = new StringBuilder();
            buffer.Append(string.Format("{0,-30}", "actual label |predicted type"));
            foreach (var key in this.labels)
            {
                buffer.Append(string.Format("{0,-30}", key));
            }
            buffer.Append(string.Format("{0,-30}\r", "recall"));
            foreach (var key in this.labels)
            {
                buffer.Append(string.Format("{0,-30}", key));
                dic = detailDic[key];
                foreach (var k in this.labels)
                {
                    buffer.Append(string.Format("{0,-30}", dic[k]));
                }
                // recall
                buffer.Append(string.Format("{0,-30}\r", 1.0 * positiveNums[key] / actualNums[key]));
            }
            buffer.Append(string.Format("{0,-30}", "precision"));
            foreach (var key in this.labels)
            {
                buffer.Append(string.Format("{0,-30:f5}", 1.0 * positiveNums[key] / predictedNums[key]));
            }
            buffer.Append("\r");
            writer.WriteLine(buffer.ToString());
            writer.Close();
        }

        /// <summary>
        ///     Predict the label of a test data given its feature.
        ///     The score is calculated by field.
        /// </summary>
        /// <param name="feature">
        ///     field-->values
        /// </param>
        /// <returns></returns>
        public string Predict(Dictionary<string, object> feature)
        {
            if(this.w == null)
            {
                LoadWeight(@"D:\Codes\C#\EntityTyping\Fine-ner\unit test\output\weight.txt");
            }
            var fields = feature.Keys.ToList();
            this.scores = new List<Pair<string, double>>(this.labels.Count);
            // Initial
            foreach (var label in labels)
            {
                this.scores.Add(new Pair<string, double>(label, 0));
            }
            // Calculate score  for each label by field
            var times = new int[this.labels.Count];
            var i = 0;
            var total = 0;
            const int minLimit = 10;
            var min = -1;
            var max = -1;
            var valueNum = 0;     // count the element number of a field
            var scoresByField = new double[this.labels.Count];      // store the score of each label got in a field
            for (int k = 0;k<fields.Count-2;k++)
            {
                var field = fields[k];
                valueNum = 0;
                for (var j = 0; j < this.labels.Count; j++)
                {
                    scoresByField[j] = 0;
                }
                var values = (IEnumerable<string>)feature[field];
                foreach (var value in values)
                {
                    min = -1;
                    max = -1;
                    i = 0;
                    foreach (var label in this.labels)
                    {
                        try
                        {
                            times[i] = this.model[label][field][value];
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
                if(valueNum >0)
                {
                    for (var j = 0; j < this.labels.Count; j++)
                    {
                        scores[j].second += scoresByField[j] / valueNum;
                    }
                }
            }
            var comparer = scores[0].GetBySecondReverseComparer();
            for (i = 0; i < scores.Count;i++ )
            {
                scores[i].second *= this.w[scores[i].first];
            }
            scores.Sort(comparer);
            return scores[0].first;
        }

        private void LoadWeight(string weightFilePath)
        {
            w = new Dictionary<string, double>();
            var reader = new LargeFileReader(weightFilePath);
            string line;

            while((line = reader.ReadLine())!=null)
            {
                var array = line.Split('\t');
                w[array[0]] = double.Parse(array[1]);
            }
            reader.Close();
        }

        /// <summary>
        /// Get type labels from the model information
        /// </summary>
        /// <returns></returns>
        private List<string> GetLabels()
        {
            return ((Dictionary<string, Dictionary<string, Dictionary<string, int>>>)this.model).Keys.ToList();
        }

        /// <summary>
        /// Load Bayes Statistic Model
        /// </summary>
        /// <param name="modelFile"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, Dictionary<string, int>>> LoadModel(string modelFile)
        {
            var model = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
            FileReader reader = new LargeFileReader(modelFile);
            string line;
            var count = 0;
            var regex = new Regex(@"^\w");

            Dictionary<string, Dictionary<string, int>> dicByField = null;
            var dicByValue = new Dictionary<string, int>();

            while ((line = reader.ReadLine()) != null)
            {
                count++;
                if (regex.IsMatch(line))
                {
                    // get new label or feild
                    var label = line;
                    try
                    {
                        dicByField = model[label];
                    }
                    catch (Exception)
                    {
                        dicByField = new Dictionary<string, Dictionary<string, int>>();
                        model[label] = dicByField;
                    }
                    var field = reader.ReadLine();
                    try
                    {
                        dicByValue = dicByField[field];
                    }
                    catch (Exception)
                    {
                        dicByValue = new Dictionary<string, int>();
                        dicByField[field] = dicByValue;
                    }
                }
                else
                {
                    line = line.Trim();
                    var array = line.Split('\t');
                    if (array.Length != 2)
                    {
                        Console.WriteLine("Wrong Format in line" + count);
                        continue;
                    }
                    dicByValue[array[0]] = int.Parse(array[1]);
                }
            }
            reader.Close();
            return model;
        }

        public static int GetDimension(string modelFilePath)
        {
            int count = 0;
            FileReader reader = new LargeFileReader(modelFilePath);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(BayesModel.END))
                {
                    count++;
                }
            }
            reader.Close();
            return count;
        }

        /// <summary>
        /// Compare the result of old model and the newest model
        /// </summary>
        /// <param name="resultFile1"></param>
        /// File path storing the old result
        /// <param name="resultFile2"></param>
        /// File path storing the new result
        public static string CompareResult(string resultFile1, string resultFile2)
        {
            var itemLabels = new HashSet<string>();
            var positiveItemsInResultOne = new HashSet<string>();
            var positiveItemsInResultTwo = new HashSet<string>();
            var negtiveItemsInResultOne = new HashSet<string>();
            var negtiveItemsInResultTwo = new HashSet<string>();

            string line;
            FileReader reader = new LargeFileReader(resultFile1);
            while((line = reader.ReadLine())!=null)
            {
                var array = line.Split('\t');
                itemLabels.Add(array[0]);
                if(array[2].StartsWith(array[1]))
                {
                    positiveItemsInResultOne.Add(array[0]);
                }
                else
                {
                    negtiveItemsInResultOne.Add(array[0]);
                }
            }
            reader.Open(resultFile2);
            while ((line = reader.ReadLine()) != null)
            {
                var array = line.Split('\t');
                if (array[2].StartsWith(array[1]))
                {
                    positiveItemsInResultTwo.Add(array[0]);
                }
                else
                {
                    negtiveItemsInResultTwo.Add(array[0]);
                }
            }
            reader.Close();
            StringBuilder report = null;
            var pp = GetIntersection(positiveItemsInResultOne, positiveItemsInResultTwo).Count;
            var pn = GetIntersection(positiveItemsInResultOne, negtiveItemsInResultTwo).Count;
            var np = GetIntersection(negtiveItemsInResultOne, positiveItemsInResultTwo).Count;
            var nn = GetIntersection(negtiveItemsInResultOne, negtiveItemsInResultTwo).Count;
            report.Append("old|new | right | wrong\r");
            report.Append(string.Format(" right  | {0} | {1}\r"),pp,pn);
            report.Append(string.Format(" wrong  | {0} | {1}\r"),np,nn);
            return report.toString();
        }

        /// <summary>
        /// Get intersection of two groups
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <returns></returns>
        public static HashSet<string> GetIntersection(HashSet<string> set1, HashSet<string> set2)
        {
            var intersection = new HashSet<string>();
            foreach (var item in set1)
            {
                if (set2.Contains(item))
                {
                    intersection.Add(item);
                }
            }
            return intersection;
        }
    }
}
