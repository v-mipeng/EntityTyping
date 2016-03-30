using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pml.ml.optimizer
{
    class Viterbi
    {

        protected class ViterbiNode
        {
            List<string> path = null;
            double prob = 0.0;

            public ViterbiNode(int pathLength)
            {
                path = new List<string>(pathLength);
            }

            public void AddPath(string state)
            {
                path.Add(state);
            }


            public List<string> GetPath()
            {
                return path;
            }

            public double Prob
            {
                get
                {
                    return prob;
                }
                set
                {
                    prob = value;
                }
            }

            public string LastPath
            {
                get
                {
                    return path[path.Count - 1];
                }
            }

        }

        private Viterbi()
        {

        }

        //ForwardViterbi(observations, states, start_probability, transition_probability, emission_probability)
        public static List<string> ForwardViterbi(IEnumerable<string> inputs,
            IEnumerable<string> states,
            Dictionary<string, double> sp,
            Dictionary<string, Dictionary<string, double>> tp,
            Dictionary<string, Dictionary<string, double>> ep)
        {
            var stepLength = inputs.Count();
            var stateNum = states.Count();
            var VNodes = new List<ViterbiNode>(stateNum);
            for (var i = 0; i < stateNum; i++)
            {
                VNodes.Add(new ViterbiNode(stepLength));
                VNodes[i].AddPath(states.ElementAt(i));
                VNodes[i].Prob = sp[states.ElementAt(i)] * ep[states.ElementAt(i)][inputs.ElementAt(0)];
            }

            var maxPro = 0.0;
            var maxIndex = 0;
            for (var i = 1; i < stepLength; i++)   // foreach step
            {
                var maxPros = new List<double>();
                var maxIndexes = new List<int>();

                foreach (var state in states)
                {
                    maxPro = 0.0;
                    maxIndex = 0;
                    for (var j = 0; j < stateNum; j++)
                    {
                        var prob = tp[VNodes[j].LastPath][state] * ep[state][inputs.ElementAt(i)] * VNodes[j].Prob;
                        if (prob > maxPro)
                        {
                            maxPro = prob;
                            maxIndex = j;
                        }
                    }
                    maxPros.Add(maxPro);
                    maxIndexes.Add(maxIndex);
                }
                // update nodes
                for (var j = 0; j < stateNum; j++)
                {
                    VNodes[j].AddPath(states.ElementAt(maxIndexes[j]));
                    VNodes[j].Prob = maxPros[j];
                }
            }
            // output the most likely path
            maxPro = 0.0;
            maxIndex = 0;
            for (var i = 0; i < stateNum; i++)
            {
                if (VNodes[i].Prob > maxPro)
                {
                    maxPro = VNodes[i].Prob;
                    maxIndex = i;
                }
            }
            return VNodes[maxIndex].GetPath();
        }

        //Weather states
        static String HEALTHY = "Healthy";
        static String FEVER = "Fever";
        //Dependable actions (observations)
        static String DIZZY = "dizzy";
        static String COLD = "cold";
        static String NORMAL = "normal";


        static void Main(string[] args)
        {
            //initialize our arrays of states and observations
            String[] states = { HEALTHY, FEVER };
            String[] observations = { DIZZY, COLD};

            var start_probability = new Dictionary<String, double>();
            start_probability.Add(HEALTHY, 0.6f);
            start_probability.Add(FEVER, 0.4f);
            //Transition probability
            var transition_probability = new Dictionary<String, Dictionary<String, double>>();
            var t1 = new Dictionary<String, double>();
            t1.Add(HEALTHY, 0.7f);
            t1.Add(FEVER, 0.3f);
            Dictionary<String, double> t2 = new Dictionary<String, double>();
            t2.Add(HEALTHY, 0.4f);
            t2.Add(FEVER, 0.6f);
            transition_probability.Add(HEALTHY, t1);
            transition_probability.Add(FEVER, t2);

            //emission_probability
            var emission_probability = new Dictionary<String, Dictionary<String, double>>();
            var e1 = new Dictionary<String, double>();
            e1.Add(DIZZY, 0.1f);
            e1.Add(COLD, 0.4f);
            e1.Add(NORMAL, 0.5f);

            Dictionary<String, double> e2 = new Dictionary<String, double>();
            e2.Add(DIZZY, 0.6f);
            e2.Add(COLD, 0.3f);
            e2.Add(NORMAL, 0.1f);

            emission_probability.Add(HEALTHY, e1);
            emission_probability.Add(FEVER, e2);

            var ret = ForwardViterbi(observations, states, start_probability, transition_probability, emission_probability);
            Console.WriteLine(string.Join(",",ret));
            Console.ReadLine();

        }

    }
}
