using pml.file.reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wikipedia.API 
{
    public class WikiResource : Wiki
    {
        public string GetEntityType(string entity)
        {
            if (dbpediaEntity2Type == null)
            {
                LoadDBpediaType();
            }
            string type = null;
            if(dbpediaEntity2Type.TryGetValue(entity, out type))
            {
                return type;
            }
            return null;
        }

        public HashSet<string> GetRedirects(string query)
        {
            if (redirects == null)
            {
                LoadRedirect();
            }
            try
            {
                return redirects[query];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public int GetPageIndegree(string title)
        {
            if (pageIndegree == null)
            {
                LoadPageIndegree();
            }
            int indegree = 0;
            pageIndegree.TryGetValue(title, out indegree);
            return indegree;
        }

        public int GetPageOutdegree(string title)
        {
            if (pageOutdegree == null)
            {
                LoadPageOutdegree();
            }
            int outdegree = 0;
            pageOutdegree.TryGetValue(title, out outdegree);
            return outdegree;
        }

        public List<string> GetPageAbstract(string title)
        {
            List<int> tokenIndexes = null;
            if (pageAbstract.TryGetValue(title, out tokenIndexes))
            {
                return null;
            }
            else
            {
                var list = new List<string>();
                foreach(var tokenIndex in tokenIndexes)
                {
                    list.Add(abstractIndex2Token[tokenIndex]);
                }
                return list;
            }

        }

        public List<string> GetLinkInPages(string title)
        {
            if (linkInPageDic == null)
            {
                LoadLinkInPages();
            }
            int pageIndex = 0;
            if(!linkInPage2Index.TryGetValue(title, out pageIndex))
            {
                return null;
            }
            else
            {
                var list = new List<string>();
                var linkInPageIndexes =  linkInPageDic[pageIndex];
                foreach(var linkInPageIndex in linkInPageIndexes)
                {
                    list.Add(linkInIndex2Page[linkInPageIndex]);
                }
                return list;
            }
        }

        public List<string> GetLinkedOutPages(string title)
        {
            if (linkInPageDic == null)
            {
                LoadLinkOutPages();
            }
            int pageIndex = 0;
            if (!linkOutPage2Index.TryGetValue(title, out pageIndex))
            {
                return null;
            }
            else
            {
                var list = new List<string>();
                var linkOutPageIndexes = linkOutPageDic[pageIndex];
                foreach (var linkOutPageIndex in linkOutPageIndexes)
                {
                    list.Add(linkOutIndex2Page[linkOutPageIndex]);
                }
                return list;
            }
        }

        // TODO: complete the next two API

        public List<string> GetDisambiguations(string title)
        {
            throw new NotImplementedException();
        }

        public string GetPageCategory(string title)
        {
            throw new NotImplementedException();
        }


        #region Wikipedia

        static Dictionary<string, string> dbpediaEntity2Type = null;                   // entity type in dbpedia  (serve for indivisual feature); trimed entity -->(type<-->untrimed entity)
        static Dictionary<string, List<string>> disambiguousDic = null;                                         // 
        static Dictionary<string, List<string>> pageAnchorsDic = null;                                          // recoding page anchors of articles
        // page redirects
        static Dictionary<string, HashSet<string>> redirects = null;
        // page abstract
        static Dictionary<string, List<int>> pageAbstract = null;                                 // page absctract: sparse vector
        static Dictionary<int, string> abstractIndex2Token = null;
        // page indegree and outdegree
        static Dictionary<string, int> pageIndegree = null;
        static Dictionary<string, int> pageOutdegree = null;
        // page linked in pages
        static Dictionary<int, List<int>> linkInPageDic = null;
        static Dictionary<int, string> linkInIndex2Page = null;
        static Dictionary<string, int> linkInPage2Index = null;
        // page linked out pages
        static Dictionary<int, List<int>> linkOutPageDic = null;
        static Dictionary<int, string> linkOutIndex2Page = null;
        static Dictionary<string, int> linkOutPage2Index = null;     


        static object dbpediaDicLocker = new object();
        static object dbpediaType2IndexLocker = new object();
        static object disambiguousLocker = new object();
        static object pageAnchorLocker = new object();
        static object pageAbstractLocker = new object();
        static object pageIndegreeLocker = new object();
        static object pageOutdegreeLocker = new object();
        static object redirectLocker = new object();
        static object linkInPageLocker = new object();
        static object linkOutPageLocker = new object();

        
        public static void LoadRedirect()
        {
            lock (redirectLocker)
            {
                if (redirects == null)
                {
                    var dic = new Dictionary<string, HashSet<string>>();
                    var set = new HashSet<string>();
                    var reader = new LargeFileReader(Path.Combine(Config.dbpediaRedirectsResolvedDir, "redirect.txt"));
                    var line = "";
                    System.Text.RegularExpressions.Regex braceRegex = new System.Text.RegularExpressions.Regex(@"\(\w+\)");

                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.ToLower();
                        var array = line.Split('\t');

                        if (dic.TryGetValue(array[0], out set))
                        {
                            set.Add(array[1]);
                        }
                        else
                        {
                            set = new HashSet<string>();
                            set.Add(array[1]);
                            dic[array[0]] = set;
                        }
                    }
                    reader.Close();
                    redirects = dic;
                }
            }
        }

        public static void LoadDBpediaType()
        {
            lock (dbpediaDicLocker)
            {
                if (dbpediaEntity2Type == null)
                {
                    var dic = new Dictionary<string, string>();
                    var reader = new LargeFileReader(Path.Combine(Config.dbpediaEntityTypesResolvedDir,"entity type pair.txt"));
                    var line = "";

                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.ToLower();
                        var array = line.Split('\t');                       // array[0]: entity array[1]:type              
                        dic[array[0]] = array[1];
                    }
                    reader.Close();
                    dbpediaEntity2Type = dic;
                }
            }
        }

        private static void LoadPageAnchors()
        {
            lock (pageAnchorLocker)
            {
                if (pageAnchorsDic == null)
                {
                    var reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.page_anchor_file));
                    var line = "";
                    var dic = new Dictionary<string, List<string>>();
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"_+");

                    while ((line = reader.ReadLine()) != null)
                    {
                        var array = line.Split('\t');
                        var list = array.ToList();   // TODO: delete space(_)
                        dic[array[0]] = list;       // Depend on file format
                        list.RemoveAt(0);
                    }
                    reader.Close();
                    pageAnchorsDic = dic;
                }
            }
        }

        private static void LoadDisambiguous()
        {
            lock (disambiguousLocker)
            {
                if (disambiguousDic == null)
                {
                    var dic = new Dictionary<string, List<string>>();
                    var reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.disambiguous_file));
                    var line = "";
                    System.Text.RegularExpressions.Regex deleteUnderline = new System.Text.RegularExpressions.Regex(@"_+");

                    while ((line = reader.ReadLine()) != null)
                    {
                        var l = deleteUnderline.Replace(line, "");
                        var array = line.Split('\t').ToList();
                        dic[array[0]] = array;
                        array.RemoveAt(0);
                    }
                    reader.Close();
                    disambiguousDic = dic;
                }
            }
        }

        private static void LoadPageAbstract()
        {
            lock (pageAbstractLocker)
            {
                if (pageAbstract == null)
                {
                    var tokenTableReader = new LargeFileReader(Path.Combine(Config.dbpediaAbstractResolvedDir, "token table.txt"));
                    var pageAbstractReader = new LargeFileReader(Path.Combine(Config.dbpediaAbstractResolvedDir, "abstract.txt"));
                    var dic1 = new Dictionary<int, string>();
                    var dic2 = new Dictionary<string, List<int>>();

                    string line;

                    while ((line = tokenTableReader.ReadLine()) != null)  // index to page title
                    {
                        var array = line.Split('\t');
                        var index =   int.Parse(array[1]);
                        dic1[index] = array[0];
                    }
                    tokenTableReader.Close();
                    while ((line = pageAbstractReader.ReadLine()) != null)
                    {
                        var array = line.Split('\t');
                        var list = new List<int>();
                        for (var i = 1; i < array.Length; i++)
                        {
                            list.Add(int.Parse(array[i]));
                        }
                        dic2[array[0]] = list;
                    }
                    pageAbstractReader.Close();
                    abstractIndex2Token = dic1;
                    pageAbstract = dic2;
                }
            }
        }

        private static void LoadPageIndegree()
        {
            lock (pageIndegreeLocker)
            {
                if (pageIndegree == null)
                {
                    var source = Path.Combine(Config.dbpediaPageLinksResolvedDir, "page indegree.txt");
                    var reader = new LargeFileReader(source);
                    var dic = new Dictionary<string, int>();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.ToLower();
                        var array = line.Split('\t');                       // array[0]: entity array[1]:type 
                        int indegree = int.Parse(array[1]);
                        if (dic.ContainsKey(array[0]))
                        {
                            if (dic[array[0]] < indegree)
                            {
                                dic[array[0]] = indegree;
                            }
                        }
                        else
                        {
                            dic[array[0]] = indegree;
                        }
                    }
                    reader.Close();
                    pageIndegree = dic;
                }
            }
        }

        private static void LoadPageOutdegree()
        {
            lock (pageOutdegreeLocker)
            {
                if (pageOutdegree == null)
                {
                    var source = Path.Combine(Config.dbpediaPageLinksResolvedDir, "page outdegree.txt");
                    var reader = new LargeFileReader(source);
                    var dic = new Dictionary<string, int>();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.ToLower();
                        var array = line.Split('\t');                       // array[0]: entity array[1]:type 
                        int outdegree = int.Parse(array[1]);
                        if (dic.ContainsKey(array[0]))
                        {
                            if (dic[array[0]] < outdegree)
                            {
                                dic[array[0]] = outdegree;
                            }
                        }
                        else
                        {
                            dic[array[0]] = outdegree;
                        }
                    }
                    reader.Close();
                    pageOutdegree = dic;
                }
            }
        }

        private static void LoadLinkInPages()
        {
            lock (linkInPageLocker)
            {
                if (linkInPageDic == null)
                {
                    var pageIndexReader = new LargeFileReader(Path.Combine(Config.dbpediaPageLinksResolvedDir, @"page link ins\page index.txt"));
                    var linkInPageReader = new LargeFileReader(Path.Combine(Config.dbpediaPageLinksResolvedDir, @"page link ins\page link in.txt"));
                    var dic1 = new Dictionary<string, int>();
                    var dic2 = new Dictionary<int, string>();
                    var dic3 = new Dictionary<int, List<int>>();

                    string line;

                    while ((line = pageIndexReader.ReadLine()) != null)  // index to page title
                    {
                        var array = line.Split('\t');
                        var index = int.Parse(array[1]);
                        dic1[array[0]] = index;
                        dic2[index] = array[0];
                    }
                    pageIndexReader.Close();
                    while ((line = linkInPageReader.ReadLine()) != null)
                    {
                        var array = line.Split('\t');
                        var list = new List<int>();
                        for (var i = 1; i < array.Length; i++)
                        {
                            list.Add(int.Parse(array[i]));
                        }
                        dic3[int.Parse(array[0])] = list;
                    }
                    linkInPageReader.Close();
                    linkInPage2Index = dic1;
                    linkInIndex2Page = dic2;
                    linkInPageDic = dic3;
                }
            }
        }

        private static void LoadLinkOutPages()
        {
            lock (linkOutPageLocker)
            {
                if (linkOutPageDic == null)
                {
                    var pageIndexReader = new LargeFileReader(Path.Combine(Config.dbpediaPageLinksResolvedDir, @"page link outs\page index.txt"));
                    var linkOutPageReader = new LargeFileReader(Path.Combine(Config.dbpediaPageLinksResolvedDir, @"page link outs\page link in.txt"));
                    var dic1 = new Dictionary<string, int>();
                    var dic2 = new Dictionary<int, string>();
                    var dic3 = new Dictionary<int, List<int>>();

                    string line;

                    while ((line = pageIndexReader.ReadLine()) != null)  // index to page title
                    {
                        var array = line.Split('\t');
                        var index = int.Parse(array[1]);
                        dic1[array[0]] = index;
                        dic2[index] = array[0];
                    }
                    pageIndexReader.Close();
                    while ((line = linkOutPageReader.ReadLine()) != null)
                    {
                        var array = line.Split('\t');
                        var list = new List<int>();
                        for (var i = 1; i < array.Length; i++)
                        {
                            list.Add(int.Parse(array[i]));
                        }
                        dic3[int.Parse(array[0])] = list;
                    }
                    linkOutPageReader.Close();
                    linkOutPage2Index = dic1;
                    linkOutIndex2Page = dic2;
                    linkOutPageDic = dic3;
                }
            }
        }


        #endregion



    }
}
