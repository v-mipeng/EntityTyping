using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pml.ml.cluster
{
    public class ParallelKMeans
    {
        // define distance function options
        public static enum DistanceFunction { Euclid };
        private class DataPool
        {
            // cluster number
            internal static int k = -1;
            //
            internal static int dimension = -1;
            // points[i] is the ith point and point[i][j] is the jth value of point[i]
            internal static double[][] points;
            // centroids[i][j] is the jth value of centroid[i]
            internal static double[][] centroids;
            // distances[i][j] is the distance between point[i] and centroid[j].
            internal static double[][] distances;
            // clusters[i] is the cluster id of point[i]
            internal static int[] clusters;
            // pointsByCluster[i] store point ids which belong to the cluster centred on centroid[i]
            internal static HashSet<int>[] pointsByCluster;
            //  distance function
            internal static DistanceFunction disFun = DistanceFunction.Euclid;
        }

        class KMeansThread
        {
            int threadID = -1;

            public KMeansThread(int threadID)
            {
                this.threadID = threadID;
            }

            public void DistanceCalculator()
            {
                var centorid = DataPool.centroids[threadID];
                int i = 0;
                foreach (var point in DataPool.points)
                {
                    DataPool.distances[i][threadID] = GetDistance(point, centorid);
                    i++;
                }
            }

            /// <summary>
            /// Find cluster ids of points calculated by this thread
            /// </summary>
            public void PointCluster()
            {
                int average = DataPool.points.Length / DataPool.k;
                int start = average * threadID;
                int end = start + average;
                if (end > DataPool.points.Length)
                {
                    end = DataPool.points.Length;
                }
                double max = -1;
                int maxIndex = -1;
                for (var i = start; i < end; i++)
                {
                    max = -1;
                    for (var j = 0; j < DataPool.k; j++)
                    {
                        if (DataPool.distances[i][j] > max)
                        {
                            max = DataPool.distances[i][j];
                            maxIndex = j;
                        }
                    }
                    DataPool.clusters[i] = maxIndex;
                    UpdateClusterSet(maxIndex, i);
                }

            }

            /// <summary>
            /// Updata DataPool.centroids[threadID]
            /// </summary>
            public void CentroidCalculator()
            {
                double[] centroid = new double[DataPool.points[0].Length];
                foreach (var pointID in DataPool.pointsByCluster[threadID])
                {
                    var point = DataPool.points[pointID];
                    for (var i = 0; i < centroid.Length; i++)
                    {
                        centroid[i] += point[i];
                    }
                }
                var count = DataPool.pointsByCluster[threadID].Count;
                for (var i = 0; i < centroid.Length; i++)
                {
                    centroid[i] /= count;
                }
                DataPool.centroids[threadID] = centroid;
            }
        }


        public ParallelKMeans(int k, DistanceFunction disFun = DistanceFunction.Euclid)
        {
            DataPool.k = k;
            DataPool.disFun = disFun;
        }

        public int[] ComputeCluster(double[][] points)
        {
            DataPool.points = points;
            Initial();
            KMeansThread[] threadEntrance = new KMeansThread[DataPool.k];
            Thread[] threads = new Thread[DataPool.k];
            for (int i = 0; i < DataPool.k; i++)
            {
                threadEntrance[i] = new KMeansThread(i);
            }

            double lastTotalDistance = 0;
            double totalDistance = 0;

            while (true)
            {
                // create k threads and calculate the centroids(the order is decided by the initial method)
                for (int i = 0; i < DataPool.k; i++)
                {
                    threads[i] = new Thread(new ThreadStart(threadEntrance[i].CentroidCalculator));
                    threads[i].Start();
                }
                for (int i = 0; i < DataPool.k; i++)
                {
                    threads[i].Join();
                }
                for (int i = 0; i < DataPool.k; i++)
                {
                    threads[i] = new Thread(new ThreadStart(threadEntrance[i].DistanceCalculator));
                    threads[i].Start();
                }
                for (int i = 0; i < DataPool.k; i++)
                {
                    threads[i].Join();
                }
                for (int i = 0; i < DataPool.k; i++)
                {
                    threads[i] = new Thread(new ThreadStart(threadEntrance[i].DistanceCalculator));
                    threads[i].Start();
                }
                for (int i = 0; i < DataPool.k; i++)
                {
                    threads[i].Join();
                }
                totalDistance = 0;
                for(int i = 0;i<DataPool.points.Length;i++)
                {
                    totalDistance += DataPool.distances[i][DataPool.clusters[i]];
                }
                // check if stop
                if((totalDistance-lastTotalDistance)/totalDistance < 0.05)
                {
                    break;
                }
                else
                {
                    lastTotalDistance = totalDistance;
                }
            }
            return DataPool.clusters.ToArray();
        }


        public double[][] Centroids
        {
            get
            {
                var centroids = new double[DataPool.k][];

                for (int i = 0; i < DataPool.k; i++)
                {
                    centroids[i] = new double[DataPool.dimension];
                    for (int j = 0; j < DataPool.dimension; j++)
                    {
                        centroids[i][j] = DataPool.centroids[i][j];
                    }
                }
                return centroids;
            }
            private set;
        }

        public int[] Clusters
        {
            get
            {
                return DataPool.clusters.ToArray();
            }
            private set;
        }

        private void Initial()
        {
            DataPool.dimension = DataPool.points[0].Length;
            DataPool.centroids = new double[DataPool.k][];
            DataPool.pointsByCluster = new HashSet<int>[DataPool.k];
            DataPool.clusters = new int[DataPool.points.Length];
            for (int i = 0; i < DataPool.k; i++)
            {
                DataPool.centroids[i] = new double[DataPool.dimension];
            }
            for (int i = 0; i < DataPool.points.Length; i++)
            {
                DataPool.distances[i] = new double[DataPool.k];
            }
            for (int i = 0; i < DataPool.k; i++)
            {
                DataPool.pointsByCluster[i] = new HashSet<int>();
            }
            InitialCentroids();
        }


        /// <summary>
        /// Initial centroids with random assign method: randomly assign point to cluster
        /// </summary>
        private void InitialCentroids()
        {
            Random random = new Random();
            for (int i = 0; i < DataPool.points.Length; i++)
            {
                var cluster = random.Next(0, DataPool.k);
                DataPool.pointsByCluster[cluster].Add(i);
            }
        }

        private int[] InitClustering(int numTuples, int numClusters, int seed)
        {
            Random random = new Random(seed);
            int[] clustering = new int[numTuples];
            for (int i = 0; i < numClusters; ++i)
                clustering[i] = i;
            for (int i = numClusters; i < clustering.Length; ++i)
                clustering[i] = random.Next(0, numClusters);
            return clustering;
        }

        // Normalize the vector
        private static double[][] Normalized(double[][] rawData)
        {
            double[][] result = new double[rawData.Length][];
            for (int i = 0; i < rawData.Length; ++i)
            {
                result[i] = new double[rawData[i].Length];
                Array.Copy(rawData[i], result[i], rawData[i].Length);
            }

            for (int j = 0; j < result[0].Length; ++j)
            {
                double colSum = 0.0;
                for (int i = 0; i < result.Length; ++i)
                    colSum += result[i][j];
                double mean = colSum / result.Length;
                double sum = 0.0;
                for (int i = 0; i < result.Length; ++i)
                    sum += (result[i][j] - mean) * (result[i][j] - mean);
                double sd = sum / result.Length;
                for (int i = 0; i < result.Length; ++i)
                    result[i][j] = (result[i][j] - mean) / sd;
            }
            return result;
        }

        static void UpdateClusterSet(int setID, int pointID)
        {
           lock(DataPool.pointsByCluster[setID])
           {
               DataPool.pointsByCluster[setID].Add(pointID); //TODO: clear set befor each loop
           }
        }
       
        /// <summary>
        /// Get distance between double vector a and b.
        /// The distance measuer is decided by the disFun option.
        /// Supported Measures contains:
        ///     1. Euclid distance
        ///     2. ...
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>

        static double  GetDistance(double[] a,double[] b)
        {
            if (DataPool.disFun == DistanceFunction.Euclid)
            {
                return GetEuclidDistance(a, b);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        ///   Calculate the distance between vector a and b using Euclid Distance
        /// </summary>
        /// <param name="a">a double vector</param>
        /// <param name="b">a double vector</param>
        /// <returns></returns>
        static double GetEuclidDistance(double[] a, double[] b)
        {
            double sumSquaredDiffs = 0.0;
            for (int j = 0; j < a.Length; ++j)
                sumSquaredDiffs += Math.Pow((a[j] - b[j]), 2);
            return Math.Sqrt(sumSquaredDiffs);
        }

    }
}

