using System.Collections.Generic;
using System.Linq;
using TinySite.Models.Query;

namespace TinySite.Models
{
    public class QueryResult
    {
        public IEnumerable<dynamic> Source { get; set; }

        public int PageEvery { get; set; }

        public string FormatUrl { get; set; }

        public OrderClause Order { get; set; }

        public int Take { get; set; }

        public WhereClause Where { get; set; }

        public IQueryable<dynamic> Results { get; set; }

    }
}
