using System.Collections.Generic;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class Paginator
    {
        public IEnumerable<dynamic> Documents { get; set; }

        public Pagination Pagination { get; set; }

        public dynamic GetAsDynamic()
        {
            dynamic data = new CaseInsenstiveExpando();
            data.Documents = this.Documents;
            data.Pagination = Pagination;

            return data;
        }
    }
}
