using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wikipedia
{
    public class Config
    {
        public static string dbpediaUrl = "http://wiki.dbpedia.org/Downloads2015-04/";
        public static string dbpediaPageLinksUrl = "http://downloads.dbpedia.org/2015-04/core-i18n/en/page-links_en.nt.bz2";
        public static string dbpediaRedirectsUrl = "http://downloads.dbpedia.org/2015-04/core-i18n/en/redirects_en.nt.bz2";
        public static string dbpediaEntityTypesUrl = " http://downloads.dbpedia.org/preview.php?file=2015-04_sl_core-i18n_sl_en_sl_instance-types_en.nt.bz2";

        public static string dbpediaPageLinksBZip2      = @"D:\Data\DBpedia\";      // directory to store the compressed page link bz file download from internet
        public static string dbpediaRedirectsBZip2      = @"D:\Data\DBpedia\";      // the uncompressed  file will be also stored there
        public static string dbpediaEntityTypesBZip2    = @"D:\Data\DBpedia\";
        public static string dbpediaAmbiguousBZip2      = @"D:\Data\DBpedia\";
        public static string dbpediaAbstractBZip2       = @"D:\Data\DBpedia\";

        public static string dbpediaPageLinksResolvedDir = @"D:\Data\DBpedia\Resolved\page links\";
        public static string dbpediaRedirectsResolvedDir = @"D:\Data\DBpedia\Resolved\redirects\";
        public static string dbpediaEntityTypesResolvedDir = @"D:\Data\DBpedia\Resolved\entity types\";
        public static string dbpediaAmbiguousResolvedDir = @"D:\Data\DBpedia\Resolved\disambiguous\";
        public static string dbpediaAbstractResolvedDir = @"D:\Data\DBpedia\Resolved\abstracts\";

    }
}
