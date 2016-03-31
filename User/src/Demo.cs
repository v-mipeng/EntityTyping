using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.type;
using msra.nlp.tr;
using System.Threading;
using msra.nlp.tr.predict;
using System.Xml;
using System.IO;

namespace User.src
{
    class Demo
    {

        public Demo() { }

        /// <summary>
        /// Predict the type of mention given its context
        /// </summary>
        /// <param name="mention"></param>
        /// <param name="context"></param>
        /// <returns>
        /// A string represent the type of the mention or UNKNOWN if some exception threw
        /// </returns>
        public string Predict(string mention, string context)
        {
            FullFeaturePredictor predictor = null;
            try
            {
                predictor = FullFeaturePredictorPool.GetFullFeaturePredictor();   // Get Predictor from predictor pool
                var type = predictor.Predict(mention, context);
                FullFeaturePredictorPool.ReturnFullFeaturePredictor(predictor);       // Rember return the predictor back to the pool
                return type;
            }
            catch (Exception ex)
            {
                FullFeaturePredictorPool.ReturnFullFeaturePredictor(predictor);       // Rember return the predictor back to the pool
                Console.WriteLine(string.Format("{0} for query: {1}!\nReturn UNKNOWN label.", ex.Message, mention));
                return "UNKNOWN";
            }
        }

        public string FourClassPredict(string mention, string context)
        {
            FourClassPredictor predictor = null;
            try
            {
                predictor = FourClassPredictorPool.GetFourClassPredictor();   // Get Predictor from predictor pool
                var type = predictor.Predict(mention, context);
                FourClassPredictorPool.ReturnFourClassPredictor(predictor);       // Rember return the predictor back to the pool
                return type;
            }
            catch (Exception ex)
            {
                FourClassPredictorPool.ReturnFourClassPredictor(predictor);       // Rember return the predictor back to the pool
                Console.WriteLine(string.Format("{0} for query: {1}!\nReturn UNKNOWN label.", ex.Message, mention));
                return "UNKNOWN";
            }
        }

        /// <summary>
        /// Make prediction for a list of queries.
        /// </summary>
        /// <param name="queries">
        /// A list of pairs with pair.first the mention and pair.second the context
        /// </param>
        /// <param name="numPerThread">
        /// Number of query dealt by each thread. Default is 1000
        /// </param>
        /// <returns>
        /// Types corresponding to the queries.
        /// </returns>
        public List<string> Predict(List<Pair<string, string>> queries, int numPerThread = 4000)
        {
            List<string> types = new List<string>();
            if (queries.Count <= numPerThread)
            {
                for (var i = 0; i < queries.Count; i++)
                {
                    types.Add(Predict(queries[i].first, queries[i].second));
                }
            }
            else
            {
                for (var i = 0; i < queries.Count; i++)
                {
                    types.Add(null);
                }
                var threadNum = (int)Math.Ceiling(1.0 * queries.Count / numPerThread);
                var ThreadClasses = new List<DemoThread>(threadNum);
                var threads = new List<Thread>(threadNum);

                for (var i = 0; i < threadNum; i++)
                {
                    var threadClass = new DemoThread(queries, i * numPerThread, (i + 1) * numPerThread - 1, types);
                    var thread = new Thread(threadClass.Run);
                    thread.Name = "Thread " + i;
                    threads.Add(thread);
                    thread.Start();
                    Console.Clear();
                    Console.WriteLine("Thread {0} start.", i);
                }
                // Wait until all the threads complete work
                for (var i = 0; i < threads.Count; i++)
                {
                    threads[i].Join();
                }
            }
            return types;

        }


        public List<string> FourClassPredict(List<Pair<string, string>> queries, int numPerThread = 4000)
        {
            List<string> types = new List<string>();
            if (queries.Count <= numPerThread)
            {
                for (var i = 0; i < queries.Count; i++)
                {
                    types.Add(FourClassPredict(queries[i].first, queries[i].second));
                }
            }
            else
            {
                for (var i = 0; i < queries.Count; i++)
                {
                    types.Add(null);
                }
                var threadNum = (int)Math.Ceiling(1.0 * queries.Count / numPerThread);
                var ThreadClasses = new List<DemoThread>(threadNum);
                var threads = new List<Thread>(threadNum);

                for (var i = 0; i < threadNum; i++)
                {
                    var threadClass = new FourClassPredictThread(queries, i * numPerThread, (i + 1) * numPerThread - 1, types);
                    var thread = new Thread(threadClass.Run);
                    thread.Name = "Thread " + i;
                    threads.Add(thread);
                    thread.Start();
                    Console.Clear();
                    Console.WriteLine("Thread {0} start.", i);
                }
                // Wait until all the threads complete work
                for (var i = 0; i < threads.Count; i++)
                {
                    threads[i].Join();
                }
            }
            return types;

        }


        /// <summary>
        /// Predict the type of mention given its context with probability
        /// </summary>
        /// <param name="mention"></param>
        /// <param name="context"></param>
        /// <returns>
        /// A list of pairs, with pair.first the type and pair.second the score of corresponding type.
        /// </returns>
        public List<Pair<string, float>> PredictWithProbability(string mention, string context)
        {
            try
            {
                var predictor = FullFeaturePredictorPool.GetFullFeaturePredictor();
                var typeWithProbability = predictor.PredictWithProbability(mention, context);
                FullFeaturePredictorPool.ReturnFullFeaturePredictor(predictor);
                return typeWithProbability;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("{0} for query: {1}!\nReturn null.", ex.Message, mention));
                return null;
            }
        }

        class DemoThread
        {
            protected List<Pair<string, string>> queries = null;
            protected int begin = 0, end = 0;
            protected List<string> types;

            public DemoThread(List<Pair<string, string>> queries, List<string> types)
                : this(queries, 0, queries.Count - 1, types)
            {

            }

            public DemoThread(List<Pair<string, string>> queries, int begin, int end, List<string> types)
            {
                this.queries = queries;
                this.begin = begin;
                this.end = end;
                this.types = types;
            }

            public virtual void Run()
            {
                int count = 0;
                for (var i = this.begin; i <= this.end && i < queries.Count; i++)
                {
                    if (++count % ((end - begin) / 10) == 0)
                    {
                        Console.Clear();
                        Console.WriteLine(string.Format("{0} has proccessed {1} items", Thread.CurrentThread.Name, count));
                    }
                    FullFeaturePredictor predictor = null;
                    try
                    {
                        predictor = FullFeaturePredictorPool.GetFullFeaturePredictor();
                        types[i] = predictor.Predict(queries[i].first, queries[i].second);
                        FullFeaturePredictorPool.ReturnFullFeaturePredictor(predictor);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                        FullFeaturePredictorPool.ReturnFullFeaturePredictor(predictor);
                        Console.WriteLine(string.Format("{0} for query {1}!\nSkip this query and label it as UNKNOWN", ex.Message, i));
                        types[i] = "UNKNOWN";
                    }
                }
            }
        }

        class FourClassPredictThread : DemoThread
        {
           
            public FourClassPredictThread(List<Pair<string, string>> queries, List<string> types)
                : base(queries, types)
            {
            }

            public FourClassPredictThread(List<Pair<string, string>> queries, int begin, int end, List<string> types)
                : base(queries, begin, end, types)
            {
            }

            public override void Run()
            {
                int count = 0;
                for (var i = this.begin; i <= this.end && i < queries.Count; i++)
                {
                    if (++count % ((end - begin) / 10) == 0)
                    {
                        Console.Clear();
                        Console.WriteLine(string.Format("{0} has proccessed {1} items", Thread.CurrentThread.Name, count));
                    }
                    FourClassPredictor predictor = null;
                    try
                    {
                        predictor = FourClassPredictorPool.GetFourClassPredictor();
                        types[i] = predictor.Predict(queries[i].first, queries[i].second);
                        FourClassPredictorPool.ReturnFourClassPredictor(predictor);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                        FourClassPredictorPool.ReturnFourClassPredictor(predictor);
                        Console.WriteLine(string.Format("{0} for query {1}!\nSkip this query and label it as UNKNOWN", ex.Message, i));
                        types[i] = "UNKNOWN";
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            var pipeline = new Pipeline(@"D:\Codes\Project\package\config.xml");
            var demo = new Demo();
            //Console.WriteLine(demo.Predict("House Ways and Means Committee", "Influential members of the House Ways and Means Committee introduced legislation that would restrict how the new savings-and-loan bailout agency can raise capital , creating another potential obstacle to the government 's sale of sick thrifts ."));
            //Console.ReadKey();
            var source = @"D:\Codes\Project\EntityTyping\Fine-ner\unit test\input.txt";
            var reader = new pml.file.reader.LargeFileReader(source);
            var des = @"D:\Codes\Project\EntityTyping\Fine-ner\unit test\output.txt";
            var writer = new pml.file.writer.LargeFileWriter(des, System.IO.FileMode.Create);
            string line;
            var queries = new List<Pair<string, string>>();

            while ((line = reader.ReadLine()) != null)
            {
                var array = line.Split('\t');
                queries.Add(new pml.type.Pair<string, string>(array[0], array[1]));
            }
            reader.Close();
            var types = demo.Predict(queries);
            for (var i = 0; i < queries.Count; i++)
            {
                writer.WriteLine(string.Format("{0}\t{1}\t{2}", queries[i].first, types[i], queries[i].second));
            }
            types = demo.FourClassPredict(queries);
            writer.WriteLine("");
            writer.WriteLine("Predict by predictor trained on 4 classes:");
            writer.WriteLine("");
            for (var i = 0; i < queries.Count; i++)
            {
                writer.WriteLine(string.Format("{0}\t\t{1}\t\t{2}", queries[i].first, types[i], queries[i].second));
            }
            writer.Close();
        }

        public static void Test()
        {
            var configFile = @"D:\Codes\Project\EntityTyping\Fine-ner\config.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(configFile);
            var nodes = doc.DocumentElement.SelectNodes("/config/parameters/parameter");
            foreach(XmlNode node in nodes)
            {
                var name = node.Attributes["name"].Value;
                var type = node.Attributes["type"].Value;
                var value = node.Attributes["value"].Value;
            }

        }
    }
}
