using System.Collections.Generic;

namespace TinySite.Models
{
    public class Paginator
    {
        public Paginator(IEnumerable<DocumentFile> documents, Pagination pagination)
        {
            this.Documents = documents;
            this.Pagination = pagination;
        }

        public IEnumerable<DocumentFile> Documents { get; }

        public Pagination Pagination { get; }
    }
}
