using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrNet.Commands.Parameters;
using Carinthia.Domain;

namespace Carinthia.SolrNet
{
    public class Searcher
    {
        public SearchResults DoSearch(string sField, string sQueryTerm, string sExcludeTerm, Dictionary<string, string> FacetFilterCollection, int ResultsPerPage, int PageNumber, string sStartDate, string sEndDate, string Boost, string sAcctId)
        {
            int nAcctId;
            string sIncludeQuery = "", sExcludeQuery = "";

            new SolrBaseRepository.Instance<Contact>().Start();
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Contact>>();

            sStartDate = sStartDate == "" ? "*" : DateTime.Parse(sStartDate).ToString("yyyy-MM-ddTHH:mm:ssZ");
            sEndDate = sEndDate == "" ? "*" : DateTime.Parse(sEndDate).ToString("yyyy-MM-ddTHH:mm:ssZ");
            sQueryTerm = sQueryTerm == "" ? "*" : sQueryTerm;
            int nBoostfactor = 1;
            Int32.TryParse(Boost == "" ? "1" : Boost, out nBoostfactor);

            #region Query Parameters
            HighlightingParameters HighlightParams = new HighlightingParameters
            {
                Fields = new[] {    "cont_first_name_phonetic",
                                    "cont_first_name",
                                    "cont_last_name", 
                                    "email_addr", 
                                    "cont_source_id", 
                                    "cont_note_text", 
                                    "cont_attachment_text" 
                                },
                BeforeTerm = "<span style='background-color:Yellow'>",
                AfterTerm = "</span>"
            };

            SpellCheckingParameters SpellCheckParams = new SpellCheckingParameters
            {
                OnlyMorePopular = true,
                Count = 5,
                Collate = true,
                Query = sQueryTerm
            };

            FacetParameters FacetParams = new FacetParameters
            {
                MinCount = 1,
                Sort = true,
                Queries = new List<ISolrFacetQuery>
                {                   new SolrFacetFieldQuery("gender_name"), 
                                    new SolrFacetFieldQuery("home_state_name"),
                                    new SolrFacetFieldQuery("home_city"),
                                    new SolrFacetFieldQuery("cont_type_name_facet")
                                    //new SolrFacetQuery(new SolrQueryByRange<string>("cont_first_name", "A", "G")),
                                    //new SolrFacetQuery(new SolrQueryByRange<string>("cont_first_name", "H", "P")),
                                    //new SolrFacetQuery(new SolrQueryByRange<string>("cont_first_name", "Q", "Z"))
                }
            };

            List<ISolrQuery> FilterQuery = new List<ISolrQuery>() { };
            if (FacetFilterCollection != null && FacetFilterCollection.Count > 0)
            {
                foreach (KeyValuePair<string, string> kp in FacetFilterCollection)
                    FilterQuery.Add(new SolrQueryByField(kp.Key, kp.Value));
            }

            #endregion

            var QueryOptions = new QueryOptions
            {
                Rows = ResultsPerPage,
                Start = (PageNumber - 1) * ResultsPerPage,
                Fields = new[] { "*", "score" },
                OrderBy = new[] { new SortOrder("score", Order.DESC), new SortOrder("cont_first_name", Order.DESC) },
                FilterQueries = FilterQuery,
                Highlight = HighlightParams,
                SpellCheck = SpellCheckParams,
                Facet = FacetParams,
            };

            switch (sField)
            {
                case "cont_first_name":
                    sIncludeQuery = sField + ":" + sQueryTerm + "^2 OR " +
                            "cont_first_name_phonetic" + ":" + sQueryTerm;
                    if (sExcludeTerm != "")
                        sExcludeQuery = sField + ":" + sExcludeTerm;

                    break;

                case "*":

                    sIncludeQuery = "cont_first_name_phonetic:" + sQueryTerm + " OR " +
                                    "cont_first_name:" + sQueryTerm + "^2 OR " +
                             "cont_last_name:" + sQueryTerm + " OR " +
                             "email_addr:" + sQueryTerm + " OR " +
                             "cont_source_id:" + sQueryTerm + " OR " +
                             "cont_type_name:" + sQueryTerm + " OR " +
                             "cont_attachment_text:" + sQueryTerm + " OR " +
                             "cont_note_text:" + sQueryTerm;

                    if (sExcludeTerm != "")
                        sExcludeQuery = "cont_first_name_phonetic:" + sExcludeTerm + " OR " +
                               "cont_last_name:" + sExcludeTerm + " OR " +
                               "email_addr:" + sExcludeTerm + " OR " +
                               "cont_source_id:" + sExcludeTerm + " OR " +
                               "cont_type_name:" + sExcludeTerm + " OR " +
                               "cont_note_text:" + sExcludeTerm;

                    break;

                case "registration_date":

                    sIncludeQuery = sField + ":" + sQueryTerm + " AND registration_date:[" + sStartDate + " TO " + sEndDate + "]";

                    if (sExcludeTerm != "")
                        sExcludeQuery = "cont_first_name_phonetic:" + sExcludeTerm + " OR " +
                               "cont_last_name:" + sExcludeTerm + " OR " +
                               "email_addr:" + sExcludeTerm + " OR " +
                               "cont_source_id:" + sExcludeTerm + " OR " +
                               "cont_type_name:" + sExcludeTerm + " OR " +
                               "cont_note_text:" + sExcludeTerm;

                    break;

                case "~": // Raw Query Syntax - Don't process anything
                    sIncludeQuery = sQueryTerm;
                    break;

                default:
                    sIncludeQuery = sField + ":" + sQueryTerm;
                    if (sExcludeTerm != "")
                        sExcludeQuery = sField + ":" + sExcludeTerm;
                    break;
            }

            //// Enforce AccountId for Multitenancy
            //if (Int32.TryParse(sAcctId, out nAcctId))
            //    sIncludeQuery += " AND acct_id:" + nAcctId.ToString();

            SolrQuery ContactQuery = new SolrQuery(sIncludeQuery + (sExcludeQuery != "" ? " -(" + sExcludeQuery + ")" : ""));

            AbstractSolrQuery SolrQueryAb = ContactQuery;
            if (Int32.TryParse(sAcctId, out nAcctId))
                SolrQueryAb = ContactQuery && new SolrQuery("acct_id:" + nAcctId.ToString());

            var results = solr.Query(SolrQueryAb, QueryOptions);

            int idx = (QueryOptions.Start + 1) ?? 1;
            // Encapuslate the result and send object to caller
            var SearchResultSet = new SearchResults
            {
                ResultSet = results.Select((p) =>
                {
                    return new Contact
                    {
                        SerialNo = idx++,
                        FirstName = (sField.Contains("cont_first_name") && sQueryTerm != "*" ? results.Highlights[p.ContStub].ContainsKey("cont_first_name_phonetic") ? results.Highlights[p.ContStub]["cont_first_name_phonetic"].ToList()[0] : results.Highlights[p.ContStub]["cont_first_name"].ToList()[0] : p.FirstName) ?? "",
                        LastName = (sField == "cont_last_name" && sQueryTerm != "*" ? results.Highlights[p.ContStub][sField].ToList()[0] : p.LastName) ?? "",
                        EmailAddr = (sField == "email_addr" && sQueryTerm != "*" ? results.Highlights[p.ContStub][sField].ToList()[0] : p.EmailAddr) ?? "",
                        SourceId = (sField == "cont_source_id" && sQueryTerm != "*" ? results.Highlights[p.ContStub][sField].ToList()[0] : p.SourceId) ?? "",
                        NoteText = ((sField == "cont_note_text" || sField == "cont_attachment_text") && sQueryTerm != "*" ? results.Highlights[p.ContStub][sField].ToList()[0] : "") ?? "",
                        ResultScore = p.ResultScore,
                        AccountId = p.AccountId
                    };
                }).ToList(),
                QueryTime = results.Header.QTime,
                TotalHits = results.NumFound,
                QuerySuggestions = results.SpellChecking.SelectMany(p => p.Suggestions.ToList()),
                RawSolrQuery = ContactQuery.Query
            };

            if (results.FacetFields.Count > 0)
            {
                foreach (var facet in results.FacetFields["gender_name"])
                    SearchResultSet.GenderFacets.Add(facet.Key == "" ? "Blank" : facet.Key, facet.Value);

                foreach (var facet in results.FacetFields["home_state_name"])
                    SearchResultSet.StateFacets.Add(facet.Key == "" ? "Blank" : facet.Key, facet.Value);

                foreach (var facet in results.FacetFields["home_city"])
                    SearchResultSet.CityFacets.Add(facet.Key == "" ? "Blank" : facet.Key, facet.Value);

                foreach (var facet in results.FacetFields["cont_type_name_facet"])
                    SearchResultSet.ContTypeFacets.Add(facet.Key == "" ? "Blank" : facet.Key, facet.Value);
            }

            //// Arbitrary Facet Queries like Name Ranges and so on ..
            //if (results.FacetQueries.Count > 0)
            //{
            //    foreach (var arbitraryFacet in results.FacetQueries)
            //        SearchResultSet.ArbitraryFacets.Add(arbitraryFacet.Key, arbitraryFacet.Value);
            //}


            return SearchResultSet;
        }
    }
}