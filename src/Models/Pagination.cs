using System.Collections.Generic;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class Pagination : CaseInsensitiveExpando
    {
        public int Page { get { return this.Get<int>(); } set { this.Set<int>(value); } }

        public int PerPage { get { return this.Get<int>(); } set { this.Set<int>(value); } }

        public int TotalPage { get { return this.Get<int>(); } set { this.Set<int>(value); } }

        public IEnumerable<Page> Pages { get { return this.Get<IEnumerable<Page>>(); } set { this.Set<IEnumerable<Page>>(value); } }

        public string NextPageUrl { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string PreviousPageUrl { get { return this.Get<string>(); } set { this.Set<string>(value); } }
    }
}
