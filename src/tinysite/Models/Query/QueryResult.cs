using System.Collections.Generic;
using System.Linq;

namespace TinySite.Models.Query
{
    public class QueryResult
    {
        public IEnumerable<dynamic> Source { get; set; }

        public int PageEvery { get; set; }

        public string FormatUrl { get; set; }

        public OrderClause Order { get; set; }

        public int Take { get; set; }

        public List<WhereClause> Wheres { get; } = new List<WhereClause>();

        public IQueryable<dynamic> Results { get; set; }
    }
}
