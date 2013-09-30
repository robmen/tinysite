using System.Collections.Generic;

namespace TinySite.Models
{
    public class Paginator
    {
        public IEnumerable<dynamic> Posts { get; set; }

        public Pagination Pagination { get; set; }
    }
}
