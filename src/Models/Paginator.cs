using System.Collections.Generic;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class Paginator
    {
        public IEnumerable<dynamic> Posts { get; set; }

        public Pagination Pagination { get; set; }

        public dynamic GetAsDynamic()
        {
            dynamic data = new CaseInsenstiveExpando();
            data.Posts = this.Posts;
            data.Pagination = Pagination;

            return data;
        }
    }
}
