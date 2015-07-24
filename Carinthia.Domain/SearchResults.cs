using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carinthia.Domain;

namespace Carinthia.Domain
{
    /// <summary>
    /// Encapsulates the result of a query
    /// </summary>
    public class SearchResults
    {
        public IEnumerable<Contact> ResultSet { get; set; }
        public int QueryTime { get; set; }
        public int TotalHits { get; set; }
        public IEnumerable<string> QuerySuggestions { get; set; }
        public Dictionary<string, int> NameFacets { get; set; }
        public Dictionary<string, int> GenderFacets { get; set; }
        public Dictionary<string, int> StateFacets { get; set; }
        public Dictionary<string, int> CityFacets { get; set; }
        public Dictionary<string, int> ContTypeFacets { get; set; }
        public Dictionary<string, int> ArbitraryFacets { get; set; }
        public string RawSolrQuery { get; set; }

        public SearchResults()
        {
            NameFacets = new Dictionary<string, int>();
            GenderFacets = new Dictionary<string, int>();
            StateFacets = new Dictionary<string, int>();
            CityFacets = new Dictionary<string, int>();
            ContTypeFacets = new Dictionary<string, int>();
            ArbitraryFacets = new Dictionary<string, int>();
        }
    }
}
