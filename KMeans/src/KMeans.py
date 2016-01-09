# -*- coding: utf-8 -*-
from numpy import array
from scipy.cluster.vq import vq, kmeans, whiten, kmeans2
import io

class KMeans(object):

    def __init__(self, sourceFile, desFile):
        self.wordList = [];
        self.vectors = [];
        self.sourceFile = sourceFile;
        self.desFile = desFile;

    def loadfeatures(self):

        reader = open(self.sourceFile,"r")
        line = reader.readline();
        array = line.split(" ");
        dimension = int(array[1]);
        line = reader.readline();
        while line is not None:
            array = line.split(" ");
            if len(array) != dimension+2:
                print(len(array));
                continue;
            self.wordList.append(array[0]);
            vector = [];
            for feature in array[1:len(array)-1]:
                vector.append(float(feature));
            self.vectors.append(vector);
            line = reader.readline();
        reader.close();


    def group(self):
        self.loadfeatures();
        centroids,null = kmeans(features,200, 10, 0.01);
        y = vq(vectors,centroids);
        writer = open(desFile,"wo");
        for i in range(0,len(centroids)):
            writer.write(centroids[i]+"\r");
        for i in range(0,len(y)):
            writer.write(wordList[i]+"\t"+y[i]+"\r");
        writer.close();

kmean = KMeans(r"D:\Codes\C#\EntityTyping\word2vec\test\vectors.txt", r"D:\Data\Google-word2vec\wordCluster.txt")
kmean.group();