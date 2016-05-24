using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using pml.file.reader;
using pml.file.writer;
using System.IO;

namespace Wikipedia
{
    public class Resolver
    {
        #region Wikipedia Page Links

        public static void ExtractPageLink(string pageLinksFile, string resolvedPageLinksFile)
        {
            var source = pageLinksFile;
            var reader = new LargeFileReader(source);
            var writer = new LargeFileWriter(resolvedPageLinksFile, FileMode.Create);
            var linkFromRegex = new Regex(@"/([^>/]+)> "); // match the link from page
            var linkToRegex = new Regex(@"/([^>/]+)> .$"); // match the link to page
            var line = "";
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if (++count % 10000 == 0)
                {
                    Console.WriteLine(string.Format("Resolved {0} lines for page links...", count));
                }
                var linkFrom = linkFromRegex.Match(line).Groups[1].Value;
                var linkTo = linkToRegex.Match(line).Groups[1].Value;
                writer.WriteLine(linkFrom + "\t" + linkTo);
            }
            reader.Close();
            writer.Close();
        }

        public static void ResolvePageIndegree(string pagelinksFile, string pageIndegreeFile, bool raw = true)
        {
            var source = pagelinksFile;
            var reader = new LargeFileReader(source);
            var indegreeWriter = new LargeFileWriter(pageIndegreeFile, FileMode.Create);
            string line;
            string linkTo = null;
            Regex linkToRegex = null;
            var indegreeDic = new Dictionary<string, int>();
            reader.ReadLine();// skip the descreption line.
            int count = 0;

            if (raw)
            {
                linkToRegex = new Regex(@"/([^>/]+)> .$"); // match the link to page
            }
            while ((line = reader.ReadLine()) != null)
            {
                if (++count % 10000 == 0)
                {
                    Console.WriteLine(string.Format("Resolved {0} lines for page indegree...", count));
                }
                if (raw)
                {
                    linkTo = linkToRegex.Match(line).Groups[1].Value;
                }
                else
                {
                    var array = line.Split('\t');
                    linkTo = array[1];
                }
                try
                {
                    indegreeDic[linkTo] += 1;
                }
                catch (Exception)
                {
                    indegreeDic[linkTo] = 1;
                }

            }
            reader.Close();
            foreach (var item in indegreeDic)
            {
                indegreeWriter.WriteLine(item.Key + "\t" + item.Value);
            }
            indegreeWriter.Close();
        }

        public static void ResolvePageOutdegree(string pagelinksFile, string pageOutdegreeFile, bool raw = true)
        {
            var source = pagelinksFile;
            var reader = new LargeFileReader(source);
            var outdegreeWriter = new LargeFileWriter(pageOutdegreeFile, FileMode.Create);
            Regex linkFromRegex = null;
            string linkFrom = null;
            var outdegreeDic = new Dictionary<string, int>();
            reader.ReadLine();// skip the descreption line.
            int count = 0;
            string line;

            if (raw)
            {
                linkFromRegex = new Regex(@"/([^>/]+)> "); // match the link from page
            }
            while ((line = reader.ReadLine()) != null)
            {
                if (++count % 10000 == 0)
                {
                    Console.WriteLine(string.Format("Resolved {0} lines for page outdegree...", count));
                }
                if (raw)
                {
                    linkFrom = linkFromRegex.Match(line).Groups[1].Value;
                }
                else
                {
                    var array = line.Split('\t');
                    linkFrom = array[0];
                }
                try
                {
                    outdegreeDic[linkFrom] += 1;
                }
                catch (Exception)
                {
                    outdegreeDic[linkFrom] = 1;
                }
            }
            reader.Close();
            foreach (var item in outdegreeDic)
            {
                outdegreeWriter.WriteLine(item.Key + "\t" + item.Value);
            }
            outdegreeWriter.Close();
        }

        public static void ResolvePageInDegreeAndOutdegree(string pagelinksFile, string pageIndegreeFile, string pageOutdegreeFile, bool raw = true)
        {
            var source = pagelinksFile;
            var reader = new LargeFileReader(source);
            var indegreeWriter = new LargeFileWriter(pageIndegreeFile, FileMode.Create);
            var outdegreeWriter = new LargeFileWriter(pageOutdegreeFile, FileMode.Create);
            string line;
            Regex linkFromRegex = null;
            Regex linkToRegex = null;
            string linkFrom = null, linkTo = null;
            var indegreeDic = new Dictionary<string, int>();
            var outdegreeDic = new Dictionary<string, int>();
            reader.ReadLine();// skip the descreption line.
            int count = 0;

            if (raw)
            {
                linkFromRegex = new Regex(@"/([^>/]+)> "); // match the link from page
                linkToRegex = new Regex(@"/([^>/]+)> .$"); // match the link to page
            }
            while((line = reader.ReadLine())!=null)
            {
                if(++count%10000==0)
                {
                    Console.WriteLine(string.Format("Resolved {0} lines for page indegree and outdegree...",count));
                }
                if (raw)
                {
                    linkFrom = linkFromRegex.Match(line).Groups[1].Value;
                    linkTo = linkToRegex.Match(line).Groups[1].Value;
                }
                else
                {

                }
                try
                {
                    indegreeDic[linkTo] += 1;
                }
                catch (Exception)
                {
                    indegreeDic[linkTo] = 1;
                }
                try
                {
                    outdegreeDic[linkFrom] += 1;
                }
                catch (Exception)
                {
                    outdegreeDic[linkFrom] = 1;
                }
            }
            reader.Close();
            foreach(var item in indegreeDic)
            {
                indegreeWriter.WriteLine(item.Key + "\t" + item.Value);
            }
            indegreeWriter.Close();
            foreach (var item in outdegreeDic)
            {
                outdegreeWriter.WriteLine(item.Key + "\t" + item.Value);
            }
            outdegreeWriter.Close();
        }

        /// <summary>
        /// Resolve pages link given page.
        /// Store the results with two files:
        /// file 1 stores the page index with: page title TAB integer
        /// file 2 stores the page link information: integer(request page index) TAB  intergers(page indexes link to the request page)
        /// </summary>
        /// <param name="pagelinksFile"></param>
        /// <param name="pageLinkInDir"></param>
        /// <param name="raw"></param>
        public static void ResolveLinkedInPage(string pagelinksFile, string pageLinkInDir, bool raw = true)
        {
            var source = pagelinksFile;
            var reader = new LargeFileReader(source);
            var pageLinkInWriter = new LargeFileWriter(Path.Combine(pageLinkInDir,"link in pages.txt"), FileMode.Create);
            var pageIndexWriter = new LargeFileWriter(Path.Combine(pageLinkInDir, "page index.txt"), FileMode.Create);
            Regex linkFromRegex = null;
            Regex linkToRegex = null;
            string linkFrom = null, linkTo = null;
            var pageIndexDic = new Dictionary<string, int>(5000000);
            var linkInPageDic = new Dictionary<int, List<int>>(5000000);
            int count = 0;
            string line;

            if (raw)
            {
                linkFromRegex = new Regex(@"/([^>/]+)> "); // match the link from page
                linkToRegex = new Regex(@"/([^>/]+)> .$"); // match the link to page
            }
            reader.ReadLine();// skip the descreption line.
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    if (++count % 10000 == 0)
                    {
                        Console.WriteLine(string.Format("Resolved {0} lines for link in pages...", count));
                    }
                    if (raw)
                    {
                        linkFrom = linkFromRegex.Match(line).Groups[1].Value;
                        linkTo = linkToRegex.Match(line).Groups[1].Value;
                    }
                    var linkFromIndex = 0;
                    var linkToIndex = 0;
                    if (!pageIndexDic.TryGetValue(linkFrom, out linkFromIndex))
                    {
                        linkFromIndex = pageIndexDic.Count;
                        pageIndexDic[linkFrom] = pageIndexDic.Count;
                    }
                    if (!pageIndexDic.TryGetValue(linkTo, out linkToIndex))
                    {
                        linkToIndex = pageIndexDic.Count;
                        pageIndexDic[linkTo] = linkToIndex;
                    }
                    List<int> list = null;
                    if (!linkInPageDic.TryGetValue(linkToIndex, out list))
                    {
                        list = new List<int>();
                        list.Add(linkFromIndex);
                        linkInPageDic[linkToIndex] = list;
                    }
                    else
                    {
                        list.Add(linkFromIndex);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
            reader.Close();
            foreach (var item in pageIndexDic)
            {
                pageIndexWriter.WriteLine(item.Key + "\t" + item.Value); // output page index
            }
            pageIndexWriter.Close();
            foreach (var item in linkInPageDic)
            {
                pageLinkInWriter.Write("\t" + item.Key);
                foreach (var linkInPageIndex in item.Value)
                {
                    pageLinkInWriter.Write("\t" + linkInPageIndex);
                }
                pageLinkInWriter.WriteLine("");
            }
            pageLinkInWriter.Close();
        }

        public static void ResolveLinkOutPage(string pagelinksFile, string pageLinkOutDir, bool raw = true)
        {
            var source = pagelinksFile;
            var reader = new LargeFileReader(source);
            var pageLinkOutWriter = new LargeFileWriter(Path.Combine(pageLinkOutDir, "link out pages.txt"), FileMode.Create);
            var pageIndexWriter = new LargeFileWriter(Path.Combine(pageLinkOutDir, "page index.txt"), FileMode.Create);
            Regex linkFromRegex = null;
            Regex linkToRegex = null;
            string linkFrom = null, linkTo = null;
            var pageIndexDic = new Dictionary<string, int>(5000000);
            var linkOutPageDic = new Dictionary<int, List<int>>(5000000);
            int count = 0;
            string line;

            if (raw)
            {
                linkFromRegex = new Regex(@"/([^>/]+)> "); // match the link from page
                linkToRegex = new Regex(@"/([^>/]+)> .$"); // match the link to page
            }
            reader.ReadLine();// skip the descreption line.
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    if (++count % 10000 == 0)
                    {
                        Console.WriteLine(string.Format("Resolved {0} lines for link out pages...", count));
                    }
                    if (raw)
                    {
                        linkFrom = linkFromRegex.Match(line).Groups[1].Value;
                        linkTo = linkToRegex.Match(line).Groups[1].Value;
                    }
                    var linkFromIndex = 0;
                    var linkToIndex = 0;
                    if (!pageIndexDic.TryGetValue(linkFrom, out linkFromIndex))
                    {
                        linkFromIndex = pageIndexDic.Count;
                        pageIndexDic[linkFrom] = pageIndexDic.Count;
                    }
                    if (!pageIndexDic.TryGetValue(linkTo, out linkToIndex))
                    {
                        linkToIndex = pageIndexDic.Count;
                        pageIndexDic[linkTo] = linkToIndex;
                    }
                    List<int> list = null;
                    if (!linkOutPageDic.TryGetValue(linkFromIndex, out list))
                    {
                        list = new List<int>();
                        list.Add(linkToIndex);
                        linkOutPageDic[linkFromIndex] = list;
                    }
                    else
                    {
                        list.Add(linkToIndex);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
            reader.Close();
            foreach (var item in pageIndexDic)
            {
                pageIndexWriter.WriteLine(item.Key + "\t" + item.Value); // output page index
            }
            pageIndexWriter.Close();
            foreach (var item in linkOutPageDic)
            {
                pageLinkOutWriter.Write("\t" + item.Key);
                foreach (var linkOutPageIndex in item.Value)
                {
                    pageLinkOutWriter.Write("\t" + linkOutPageIndex);
                }
                pageLinkOutWriter.WriteLine("");
            }
            pageLinkOutWriter.Close();
        }

        public static void ResolveLinkInAndOutPage(string pagelinksFile, string pageLinkInDic, string pageLinkOutDic, bool raw = true)
        {
            var source = pagelinksFile;
            var reader = new LargeFileReader(source);
            var pageLinkInWriter = new LargeFileWriter(Path.Combine(pageLinkInDic, "link in pages.txt"), FileMode.Create);
            var pageLinkOutWriter = new LargeFileWriter(Path.Combine(pageLinkOutDic, "link out pages.txt"), FileMode.Create);
            var pageIndexWriter = new LargeFileWriter(Path.Combine(pageLinkInDic, "page index.txt"), FileMode.Create);
            Regex linkFromRegex = null;
            Regex linkToRegex = null;
            string linkFrom = null, linkTo = null;
            var pageIndexDic = new Dictionary<string, int>(5000000);
            var linkInPageDic = new Dictionary<int, List<int>>(5000000);
            var linkOutPageDic = new Dictionary<int, List<int>>(5000000);
            int count = 0;
            string line;

            if (raw)
            {
                linkFromRegex = new Regex(@"/([^>/]+)> "); // match the link from page
                linkToRegex = new Regex(@"/([^>/]+)> .$"); // match the link to page
            }
            reader.ReadLine();// skip the descreption line.
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    if (++count % 10000 == 0)
                    {
                        Console.WriteLine(string.Format("Resolved {0} lines for link out pages...", count));
                    }
                    if (raw)
                    {
                        linkFrom = linkFromRegex.Match(line).Groups[1].Value;
                        linkTo = linkToRegex.Match(line).Groups[1].Value;
                    }
                    var linkFromIndex = 0;
                    var linkToIndex = 0;
                    if (!pageIndexDic.TryGetValue(linkFrom, out linkFromIndex))
                    {
                        linkFromIndex = pageIndexDic.Count;
                        pageIndexDic[linkFrom] = pageIndexDic.Count;
                    }
                    if (!pageIndexDic.TryGetValue(linkTo, out linkToIndex))
                    {
                        linkToIndex = pageIndexDic.Count;
                        pageIndexDic[linkTo] = linkToIndex;
                    }
                    List<int> list = null;
                    if (!linkInPageDic.TryGetValue(linkToIndex, out list))
                    {
                        list = new List<int>();
                        list.Add(linkFromIndex);
                        linkInPageDic[linkToIndex] = list;
                    }
                    else
                    {
                        list.Add(linkFromIndex);
                    }
                    if (!linkOutPageDic.TryGetValue(linkFromIndex, out list))
                    {
                        list = new List<int>();
                        list.Add(linkToIndex);
                        linkOutPageDic[linkFromIndex] = list;
                    }
                    else
                    {
                        list.Add(linkToIndex);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
            reader.Close();
            foreach (var item in pageIndexDic)
            {
                pageIndexWriter.WriteLine(item.Key + "\t" + item.Value); // output page index
            }
            pageIndexWriter.Close();
            File.Copy(Path.Combine(pageLinkInDic, "page index.txt"), Path.Combine(pageLinkOutDic, "page index.txt"), false);
            foreach (var item in linkInPageDic)     // output link in pages
            {
                pageLinkInWriter.Write("\t" + item.Key);
                foreach (var linkInPageIndex in item.Value)
                {
                    pageLinkInWriter.Write("\t" + linkInPageIndex);
                }
                pageLinkInWriter.WriteLine("");
            }
            pageLinkInWriter.Close();
            foreach (var item in linkOutPageDic)        // output link out pages
            {
                pageLinkOutWriter.Write("\t" + item.Key);
                foreach (var linkOutPageIndex in item.Value)
                {
                    pageLinkOutWriter.Write("\t" + linkOutPageIndex);
                }
                pageLinkOutWriter.WriteLine("");
            }
            pageLinkOutWriter.Close();
        }

        #endregion

        #region Wikipedia Entity Type
        public static void ResolveEntityTypes(string dbpediaEntityTypeFile, string extractedEntityTypeFile)
        {
            var reader = new LargeFileReader(dbpediaEntityTypeFile);
            var writer = new LargeFileWriter(extractedEntityTypeFile, FileMode.Create);
            var entityRegex = new Regex(@"/([^>/]+)> ");
            var typeRegex = new Regex(@"ontology/(\w+)>\s\.$");
            int count = 0;
            string line;

            reader.ReadLine(); // skip the descreption line
            while((line = reader.ReadLine())!=null)
            {
                if(++count==10000)
                {
                    Console.WriteLine(string.Format("Resoled {0} entity types...",count));
                }
                try
                {
                    var entity = entityRegex.Match(line).Groups[1].Value;
                    var type = typeRegex.Match(line).Groups[1].Value;
                    writer.WriteLine(entity + "\t" + type);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    continue;
                }
            }
            reader.Close();
            writer.Close();
        }

        #endregion

        #region Wikipedia Abstract

        public static void ExtractAbstract(string dbpediaAbstractFile, string extractedAbstractFile)
        {
            var reader = new LargeFileReader(dbpediaAbstractFile);
            var writer = new LargeFileWriter(extractedAbstractFile, FileMode.Create);
            var titleRegex = new Regex(@"([^/]+)>");
            var abstractRegex = new Regex("\"(.+)\"@en .");
            var unicodeRegex = new Regex(@"(\\u\w{4})+");
            int count = 0;
            string line;

            reader.ReadLine(); // skip the descreption line
            while ((line = reader.ReadLine()) != null)
            {
                if (count++ % 10000 == 0)
                {
                    Console.WriteLine(string.Format("Resoled {0} abstracts...", count));
                }
                try
                {
                    var title = titleRegex.Match(line).Groups[1].Value;
                    var abs = abstractRegex.Match(line).Groups[1].Value;
                    abs = unicodeRegex.Replace(abs, " ");
                    writer.WriteLine(title + "\t" + abs);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    continue;
                }
            }
            reader.Close();
            writer.Close();

        }
        public static void ResolveAbstracts(string dbpediaAbstractFile, string extractedAbstractFile, bool raw = true)
        {
            var reader = new LargeFileReader(dbpediaAbstractFile);
            var writer = new LargeFileWriter(extractedAbstractFile, FileMode.Create);
            Regex titleRegex = null;
            Regex abstractRegex = null;
            Regex unicodeRegex = null;
            string title = null;
            string abs = null;
            var tokenTable = new Dictionary<string, int>();
            var pageAbstract = new Dictionary<string, List<int>>();
            int count = 0;
            string line;

            if (raw)
            {
                titleRegex = new Regex(@"([^/]+)>");
                abstractRegex = new Regex("\"(.+)\"@en .");
                unicodeRegex = new Regex(@"(\\u\w{4})+");
            }
            reader.ReadLine(); // skip the descreption line
            while ((line = reader.ReadLine()) != null)
            {
                if (count++ % 10000 == 0)
                {
                    Console.WriteLine(string.Format("Resoled {0} abstracts...", count));
                }
                try
                {
                    if (raw)
                    {
                        title = titleRegex.Match(line).Groups[1].Value;
                        abs = abstractRegex.Match(line).Groups[1].Value;
                        abs = unicodeRegex.Replace(abs, " ");
                    }
                    else
                    {
                        var array = line.Split('\t');
                        title = array[0];
                        abs = array[1];
                    }
                    writer.WriteLine(title + "\t" + abs);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    continue;
                }
            }
            reader.Close();
            writer.Close();
        }

        #endregion

        #region   Wikepedia Redirect

        public static void ResolveRedirects(string dbpediaRedirectFile, string extractedRedirectFile)
        {
            var reader = new LargeFileReader(dbpediaRedirectFile);
            var writer = new LargeFileWriter(extractedRedirectFile, FileMode.Create);
            System.Text.RegularExpressions.Regex fromRegex = new System.Text.RegularExpressions.Regex(@"/([^>/]+)>\s<");
            System.Text.RegularExpressions.Regex toRegex = new System.Text.RegularExpressions.Regex(@"/(\w+)>\s\.$");
            int count = 0;
            string line;

            reader.ReadLine(); // skip the descreption line
            while ((line = reader.ReadLine()) != null)
            {
                if (++count % 10000 == 0)
                {
                    Console.WriteLine(string.Format("Resoled {0} redirects...",count));
                }
                var from = fromRegex.Match(line).Groups[1].Value;
                var to = toRegex.Match(line).Groups[1].Value;
                writer.WriteLine(from + "\t" + to);
            }
            reader.Close();
            writer.Close();
        }

        #endregion

        #region Wikipedia Anchor

        public static void ResolvePageAnchors(string dbpediaPageLinkFile, string extractedPageAnchorFile)
        {

        }
        #endregion

        public static void Main(string[] args)
        {
            ResolvePageLink(@"D:\Data\DBpedia\page-links_en.nt",
                @"D:\Data\DBpedia\page links.txt");
            //Unzip.Uncompress(@"D:\Data\CrossWiki\dictionary.bz2");
        }
    }
}
