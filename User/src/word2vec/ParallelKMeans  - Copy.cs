﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pml.ml.cluster.pool
{
    /// <summary>
    /// Do K-means parallelly.
    /// Time consuming is supported to be: c*n*k*d.  
    /// n:  the number of points to be clustered
    /// k:  the cluster number
    /// d:  the dimension of the representation of a point.
    /// c:  a const, related to iteration times which is usually about 10~20
    /// </summary>
    public class ParallelKMeans2
    {
        // define distance function options
        public enum DistanceFunction { Euclid };
        DataPool dataPool = null;
       

        ManualResetEvent doneEvent = new ManualResetEvent(false);

        private class DataPool
        {
            // cluster number
            public int k = -1;
            // vector dimension
            public int dimension = -1;
            // points[i] is the ith point and point[i][j] is the jth value of point[i]
            public double[][] points;
            // centroids[i][j] is the jth value of centroid[i]
            public double[][] centroids;
            // distances[i][j] is the distance between point[i] and centroid[j].
            public double[][] distances;
            // clusters[i] is the cluster id of point[i]
            public int[] clusters;
            // pointsByCluster[i] store point ids which belong to the cluster centred on centroid[i]
            public HashSet<int>[] pointsByCluster;
            // distance function
            public DistanceFunction disFun = DistanceFunction.Euclid;
            // finished task number
            public int doneTaskNum = 0;
            // notify if all the tasks finished
            public ManualResetEvent finishAllTaskSignal = new ManualResetEvent(false);
        }


        class KMeansThread
        {
            int threadID = -1;
            DataPool dataPool = null;

            public KMeansThread(int threadID, DataPool dataPool)
            {
                this.threadID = threadID;
                this.dataPool = dataPool;
            }

            public void DistanceCalculator(Object threadContext)
            {
                var centorid = this.dataPool.centroids[threadID];
                int i = 0;
                foreach (var point in this.dataPool.points)
                {
                    this.dataPool.distances[i][threadID] = GetDistance(point, centorid);
                    i++;
                }
                FinishTask();
            }

            /// <summary>
            /// Find cluster ids of points calculated by this thread
            /// </summary>
            public void PointCluster(Object threadContext)
            {
                int average = (int)Math.Ceiling((double)this.dataPool.points.Length / this.dataPool.k);
                int start = average * threadID;
                int end = start + average;
                if (end > this.dataPool.points.Length)
                {
                    end = this.dataPool.points.Length;
                }
                for (var i = start; i < end; i++)
                {
                    double min = double.MaxValue;
                    int minIndex = -1;
                    for (var j = 0; j < this.dataPool.k; j++)
                    {
                        if (this.dataPool.distances[i][j] < min)
                        {
                            min = this.dataPool.distances[i][j];
                            minIndex = j;
                        }
                    }
                    this.dataPool.clusters[i] = minIndex;
                    UpdateClusterSet(minIndex, i);
                }
                FinishTask();
            }

            /// <summary>
            /// Updata this.dataPool.centroids[threadID]
            /// </summary>
            public void CentroidCalculator(Object threadContext)
            {
                double[] centroid = new double[this.dataPool.points[0].Length];
                foreach (var pointID in this.dataPool.pointsByCluster[threadID])
                {
                    var point = this.dataPool.points[pointID];
                    for (var i = 0; i < centroid.Length; i++)
                    {
                        centroid[i] += point[i];
                    }
                }
                var count = this.dataPool.pointsByCluster[threadID].Count;
                for (var i = 0; i < centroid.Length; i++)
                {
                    centroid[i] /= count;
                }
                this.dataPool.centroids[threadID] = centroid;
                FinishTask();
            }

            void UpdateClusterSet(int setID, int pointID)
            {
                lock (this.dataPool.pointsByCluster[setID])
                {

                    this.dataPool.pointsByCluster[setID].Add(pointID);
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

            double GetDistance(double[] a, double[] b)
            {
                if (this.dataPool.disFun == DistanceFunction.Euclid)
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
            double GetEuclidDistance(double[] a, double[] b)
            {
                double sumSquaredDiffs = 0.0;
                for (int j = 0; j < a.Length; ++j)
                    sumSquaredDiffs += Math.Pow((a[j] - b[j]), 2);
                return Math.Sqrt(sumSquaredDiffs);
            }

            void FinishTask()
            {
                if(Interlocked.Increment(ref dataPool.doneTaskNum)==dataPool.k)
                {
                    dataPool.finishAllTaskSignal.Set();
                    dataPool.doneTaskNum = 0;
                }
            }

        }

        public ParallelKMeans2(DistanceFunction disFun = DistanceFunction.Euclid)
        {
            this.dataPool = new DataPool();
            this.dataPool.disFun = disFun;
        }

        public int[] Compute(int k, double[][] points)
        {
            this.dataPool.k = k;
            this.dataPool.points = points;
            Initial();
            KMeansThread[] threadEntrance = new KMeansThread[this.dataPool.k];
            for (int i = 0; i < k; i++)
            {
                threadEntrance[i] = new KMeansThread(i, this.dataPool);
            }
            double lastTotalDistance = 0;
            double totalDistance = 0;
            int iterate = 1;

            while (true)
            {
                Console.WriteLine("iteration: " + iterate++);

                // create k threads and calculate the centroids(the order is decided by the initial method)
                for (int i = 0; i < this.dataPool.k; i++)
                {
                    ThreadPool.QueueUserWorkItem(threadEntrance[i].CentroidCalculator, i);
                }

                this.dataPool.finishAllTaskSignal.WaitOne();
                this.dataPool.finishAllTaskSignal.Reset();

                for (int i = 0; i < this.dataPool.k; i++)
                {
                    ThreadPool.QueueUserWorkItem(threadEntrance[i].DistanceCalculator, i);
                }
                this.dataPool.finishAllTaskSignal.WaitOne();
                this.dataPool.finishAllTaskSignal.Reset();


                for (int i = 0; i < this.dataPool.k; i++)
                {
                    this.dataPool.pointsByCluster[i].Clear();
                }
                for (int i = 0; i < this.dataPool.k; i++)
                {
                    ThreadPool.QueueUserWorkItem(threadEntrance[i].PointCluster, i);
                }
                this.dataPool.finishAllTaskSignal.WaitOne();
                this.dataPool.finishAllTaskSignal.Reset();

                totalDistance = 0;
                for (int i = 0; i < this.dataPool.points.Length; i++)
                {
                    totalDistance += this.dataPool.distances[i][this.dataPool.clusters[i]];
                }
                Console.WriteLine("total distance: " + totalDistance);
                // check if stop
                if (Math.Abs(lastTotalDistance - totalDistance) / lastTotalDistance < 0.0001)
                {
                    break;
                }
                else
                {
                    lastTotalDistance = totalDistance;
                }

            }
            return this.dataPool.clusters.ToArray();
        }

        public double[][] Centroids
        {
            get
            {
                var centroids = new double[this.dataPool.k][];

                for (int i = 0; i < this.dataPool.k; i++)
                {
                    centroids[i] = new double[this.dataPool.dimension];
                    for (int j = 0; j < this.dataPool.dimension; j++)
                    {
                        centroids[i][j] = this.dataPool.centroids[i][j];
                    }
                }
                return centroids;
            }
        }

        public int[] Clusters
        {
            get
            {
                return this.dataPool.clusters.ToArray();
            }
        }

        private void Initial()
        {
            this.dataPool.dimension = this.dataPool.points[0].Length;
            this.dataPool.centroids = new double[this.dataPool.k][];
            this.dataPool.distances = new double[this.dataPool.points.Length][];
            this.dataPool.pointsByCluster = new HashSet<int>[this.dataPool.k];
            this.dataPool.clusters = new int[this.dataPool.points.Length];

            for (int i = 0; i < this.dataPool.k; i++)
            {
                this.dataPool.centroids[i] = new double[this.dataPool.dimension];
            }
            for (int i = 0; i < this.dataPool.points.Length; i++)
            {
                this.dataPool.distances[i] = new double[this.dataPool.k];
            }
            for (int i = 0; i < this.dataPool.k; i++)
            {
                this.dataPool.pointsByCluster[i] = new HashSet<int>();
            }
            Initialcentroids();
        }

        /// <summary>
        /// Initial centroids with random assign method: randomly assign point to cluster
        /// </summary>
        private void Initialcentroids()
        {
            Random random = new Random();
            for (int i = 0; i < this.dataPool.points.Length; i++)
            {
                var cluster = random.Next(0, this.dataPool.k);
                this.dataPool.pointsByCluster[cluster].Add(i);
            }
        }

        private int[] InitClustering(int numTuples, int numclusters, int seed)
        {
            Random random = new Random(seed);
            int[] clustering = new int[numTuples];
            for (int i = 0; i < numclusters; ++i)
                clustering[i] = i;
            for (int i = numclusters; i < clustering.Length; ++i)
                clustering[i] = random.Next(0, numclusters);
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

    

    }
}

