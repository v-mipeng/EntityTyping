﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Accord.MachineLearning;
using pml.file.reader;
using pml.file.writer;
using pml.ml.cluster;

namespace pml.ml.cluster
{
    /// <summary>
    /// Cluster word vectors.
    /// This program will read the vector of words from the given file you appointed in the constructor.
    /// The vector file format should be: word [seperator] value1 [seperator] value2...
    /// The seperator should be either TAB or Space
    /// The program will parse the file to decide the seperator and the dimension of vector.
    /// Another thing you should know is that if will be slightly faster if you set the number of the words by Size.
    /// </summary>
    class KmeansCluster
    {
        // file storing the word vectors
        string vectorFile;
        // file to store the centroids of clusters
        string centroidInfoFile;
        // file to store the word cluster id pairs
        string clusterIDFile;
        // word number
        int size = 0;
        // vector dimension for every word
        int dimension = 0;
        // the vectors of words
        double[][] vectors = null;
        // the words
        List<string> words;
        // labels of words according
        int[] labels;
        // kmeans object
        //KMeans kmeans = null;
        ParallelKMeans kmeans = null;

        // cluster word vectors
        public KmeansCluster(string vectorFile, string centroidInfoFile, string clusterIDFile)
        {
            this.vectorFile = vectorFile;
            this.centroidInfoFile = centroidInfoFile;
            this.clusterIDFile = clusterIDFile;
        }

        public string VectorFile
        {
            get
            {
                return this.vectorFile;
            }
            set
            {
                this.vectorFile = value;
                this.vectors = null;
            }
        }

        public string CentroidInfoFile
        {
            get
            {
                return this.centroidInfoFile;
            }
            set
            {
                this.centroidInfoFile = value;
            }
        }

        public string ClusterIDFile
        {
            get
            {
                return this.clusterIDFile;
            }
            set
            {
                this.clusterIDFile = value;
            }
        }

        /// <summary>
        /// Cluster vectors into k groups
        /// </summary>
        /// <param name="k">
        /// Group number to be cluster
        /// </param>
        public void Cluster(int k)
        {
            this.kmeans = new ParallelKMeans();
            if (vectors == null)
            {
                LoadVectors();
            }
            Console.WriteLine("Clustering...");
            Stopwatch watcher = new Stopwatch();
            watcher.Start();
            this.labels = kmeans.Compute(k, vectors);
            //this.labels = kmeans.Compute(vectors);
            watcher.Stop();
            int seconds = (int)watcher.ElapsedMilliseconds / 1000;
            int mins = seconds / 60;
            seconds = seconds - mins * 60;
            Console.WriteLine(string.Format("Done!\r Time Consumed:{0}m{1}s ", mins, seconds));
            Console.WriteLine("Done!");
            SaveCentroids();
            SaveClusterId();
        }

        // read word vectors from file
        private void LoadVectors()
        {
            if (this.dimension == 0)
            {
                this.dimension = GetVectorDimension();
            }
            if (this.size == 0)
            {
                FileReader reader = new LargeFileReader(vectorFile);
                this.words = new List<string>();
                var vectors = new List<double[]>();
                string line;
                int index = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    var array = line.Split(' ');
                    if (array.Length != dimension + 1)
                    {
                        continue;
                    }
                    words.Add(array[0]);
                    var vector = new double[dimension];
                    for (int i = 1; i < array.Length; i++)
                    {
                        vector[i - 1] = double.Parse(array[i]);
                    }
                    vectors.Add(vector);
                    index++;
                }
                reader.Close();
                this.size = vectors.Count;
                this.vectors = new double[this.size][];
                for (int i = 0; i < this.size; i++)
                {
                    this.vectors[i] = vectors[i];
                }
            }
            else
            {
                LoadVectors(this.size, this.dimension);
            }
        }

        char seperator = (char)0;

        private int GetVectorDimension()
        {
            FileReader reader = new LargeFileReader(vectorFile);
            string line;
            char[] seperators = new char[] { '\t', ' ' };
            string[] array;
            line = reader.ReadLine().Trim();
            double d;
            foreach (var c in seperators)
            {
                array = line.Split(c);
                if (array.Length > 1 && double.TryParse(array[1], out d))
                {
                    seperator = c;
                    break;
                }
            }
            if (seperator == (char)0)
            {
                throw new Exception("Cannot parse word vector file with default seperators:TAB and Space!\r" +
                                    "Please check your file format!");
            }
            array = line.Split(seperator);
            return array.Length - 1;
        }

        private void LoadVectors(int size, int dimension)
        {
            FileReader reader = new LargeFileReader(vectorFile);
            this.vectors = new double[size][];
            this.words = new List<string>();

            string line;
            int index = 0;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                var array = line.Split(this.seperator);
                if (array.Length != dimension + 1)
                {
                    continue;
                }
                words.Add(array[0]);
                var vector = new double[dimension];
                for (int i = 1; i < array.Length; i++)
                {
                    vector[i - 1] = double.Parse(array[i]);
                }
                this.vectors[index] = vector;
                index++;
            }
            reader.Close();
        }

        // save the centroid of clusters
        private void SaveCentroids()
        {
            var writer = new LargeFileWriter(centroidInfoFile, FileMode.Create);

            //foreach (var centroid in kmeans.Clusters.Centroids)
            foreach (var centroid in kmeans.Centroids)
            {
                foreach (var value in centroid)
                {
                    writer.Write(string.Format("{0}\t", value));
                }
                writer.WriteLine("");
            }
            writer.Close();

        }

        // save word and cluster id pairs
        private void SaveClusterId()
        {
            var writer = new LargeFileWriter(clusterIDFile, FileMode.Create);
            var dic = new Dictionary<int, List<string>>();

            for (int i = 0; i < words.Count; i++)
            {
                try
                {
                    var list = dic[labels[i]];
                    list.Add(words[i]);
                }
                catch (Exception)
                {
                    var list = new List<string>();
                    list.Add(words[i]);
                    dic[labels[i]] = list;
                }
            }
            var clusters = dic.Keys.ToList();
            clusters.Sort();
            foreach (var label in clusters)
            {
                foreach (var word in dic[label])
                {
                    writer.WriteLine(word + "\t" + label);
                }
            }
            writer.Close();
        }

        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                this.size = value;
            }
        }

        public int Dimension
        {
            get
            {
                return dimension;
            }
            private set
            {
            }
        }
    }
}
