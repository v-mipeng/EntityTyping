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
    }
}