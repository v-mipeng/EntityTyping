using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wikipedia.API
{
    interface Wiki
    {
        /// <summary>
        /// Get the titles of pages which redirect to the given page
        /// </summary>
        /// <param name="input">
        /// The title of the request page
        /// </param>
        /// <returns></returns>
        HashSet<string> GetRedirects(string input);

        /// <summary>
        /// Get possible type of given entity.
        /// </summary>
        /// <param name="entity">
        /// The full name of the entity in wikipedia(some day I should treat wiki as a whole and use unique id of page)
        /// </param>
        /// <returns></returns>
        string GetEntityType(string entity);

        /// <summary>
        /// Get indegree of page with given title
        /// </summary>
        /// <param name="title">
        /// The title of the requested page
        /// </param>
        /// <returns></returns>
        int GetPageIndegree(string title);

        /// <summary>
        /// Get outdegree of page with given title
        /// </summary>
        /// <param name="title">
        /// The title of the requested page
        /// </param>
        /// <returns></returns>
        int GetPageOutdegree(string title);

        /// <summary>
        /// Get abstract of page with given title
        /// </summary>
        /// <param name="title">
        /// The title of the requested page
        /// </param>
        /// <returns></returns>
        List<string> GetPageAbstract(string title);

        /// <summary>
        /// Get the linked pages by the given page
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        List<string> GetLinkedOutPages(string title);

        /// <summary>
        /// Get the pages which link the given page
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        List<string> GetLinkInPages(string title);

        /// <summary>
        /// Get the disambiguations items of given page.
        /// The title should be the ambiguous page in wikipedia
        /// </summary>
        /// <param name="title">
        /// The title of the ambiguous page in wikipedia
        /// </param>
        /// <returns></returns>
        List<string> GetDisambiguations(string title);

        /// <summary>
        /// Get the category of page with given title
        /// </summary>
        /// <param name="title">
        /// The title of the requested page
        /// </param>
        /// <returns></returns>
        string GetPageCategory(string title);

    }
}
