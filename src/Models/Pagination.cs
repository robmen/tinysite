using System.Collections.Generic;

namespace TinySite.Models
{
    public class Pagination
    {
        public int Page { get; set; }

        public int PerPage { get; set; }

        public int TotalPage { get; set; }

        public IEnumerable<Page> Pages { get; set; }

        public string NextPageUrl { get; set; }

        public string PreviousPageUrl { get; set; }
    }
}
