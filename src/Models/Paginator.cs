using System.Collections.Generic;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class Paginator : CaseInsensitiveExpando
    {
        public Paginator(IEnumerable<DocumentFile> documents, Pagination pagination)
        {
            this.Documents = documents;
            this.Pagination = pagination;
        }

        public IEnumerable<DocumentFile> Documents { get { return this.Get<IEnumerable<DocumentFile>>(); } set { this.Set<IEnumerable<DocumentFile>>(value); } }

        public Pagination Pagination { get { return this.Get<Pagination>(); } set { this.Set<Pagination>(value); } }
    }
}
