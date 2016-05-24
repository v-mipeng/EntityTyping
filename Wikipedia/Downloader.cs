using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Wikipedia
{
    class Downloader
    {
        public static void DownloadAll()
        {

        }

        /// <summary>
        /// Download wikipedia page redirects from dbpedia
        /// </summary>
        /// <param name="desDir"></param>
        public static void DownloadPageRedirects(string desDir = null)
        {
            var currentFolderPath = Environment.CurrentDirectory;
            var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
            var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
            if (desDir == null)
            {
                if (!Directory.Exists(Path.Combine(basedir, "Data/Wikipedia/Origin")))
                {
                    Directory.CreateDirectory(Path.Combine(basedir, "Data/Wikipedia/Origin"));
                }
            }
            basedir = Path.Combine(basedir, "Data/Wikipedia/Origin");

            using (WebClient webClient = new WebClient())
            {
                var webUrl = Config.dbpediaPageRedirectsUrl;
                Console.WriteLine("Downloading wikipedia page redirects from dbpedia...");
                webClient.DownloadFile(webUrl, Path.Combine(basedir, Path.GetFileName(Config.dbpediaPageLinksUrl)));
                Console.WriteLine("Wikipedia page redirects downloaded!");
            }
        }

        /// <summary>
        /// Download wikipedia pagelinks information from dbpedia
        /// </summary>
        /// <param name="desDir"></param>
        public static void DownloadPageLinks(string desDir = null)
        {
            var currentFolderPath = Environment.CurrentDirectory;
            var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
            var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
            if (desDir == null)
            {
                if(!Directory.Exists(Path.Combine(basedir, "Data/Wikipedia/Origin")))
                {
                    Directory.CreateDirectory(Path.Combine(basedir, "Data/Wikipedia/Origin"));
                }
            }
            basedir = Path.Combine(basedir, "Data/Wikipedia/Origin");

            using (WebClient webClient = new WebClient())
            {
                var webUrl = Config.dbpediaPageLinksUrl;
                Console.WriteLine("Downloading wikipedia page links from dbpedia...");
                webClient.DownloadFile(webUrl, Path.Combine(basedir, Path.GetFileName(Config.dbpediaPageLinksUrl)));
                Console.WriteLine("Wikipedia page links downloaded!");
            }
        }

        public static void DownloadPageAnchors()
        {

        }


        public static void DownloadPageFullText()
        {

        }

        /// <summary>
        /// Download wikipedia entity type information from dbpedia
        /// </summary>
        /// <param name="desDir"></param>
        public static void DownloadEntityTypeSets(string desDir = null)
        {
            var currentFolderPath = Environment.CurrentDirectory;
            var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
            var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
            if (desDir == null)
            {
                if(!Directory.Exists(Path.Combine(basedir, "Data/Wikipedia/Origin")))
                {
                    Directory.CreateDirectory(Path.Combine(basedir, "Data/Wikipedia/Origin"));
                }
            }
            basedir = Path.Combine(basedir, "Data/Wikipedia/Origin");

            using (WebClient webClient = new WebClient())
            {
                var webUrl = Config.dbpediaEntityTypesUrl;
                Console.WriteLine("Downloading wikipedia entity type information from dbpedia...");
                webClient.DownloadFile(webUrl, Path.Combine(basedir, Path.GetFileName(Config.dbpediaPageLinksUrl)));
                Console.WriteLine("Wikipedia page entity type information downloaded!");
            }
        }

        public static void Mains(string[] args)
        {
            DownloadPageLinks();
        }




    }
}
