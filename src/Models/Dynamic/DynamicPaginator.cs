using System;
using System.Collections.Generic;

namespace TinySite.Models.Dynamic
{
    public class DynamicPaginator : DynamicBase
    {
        public DynamicPaginator(DocumentFile activeDocument, Paginator Paginator, Site site)
        {
            this.ActiveDocument = activeDocument;
            this.Paginator = Paginator;
            this.Site = site;
        }

        private DocumentFile ActiveDocument { get; }

        private Paginator Paginator { get; }

        public Site Site { get; }

        protected override IDictionary<string, object> GetData()
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(this.Paginator.Documents), new Lazy<object>(GetDocuments) },
                { nameof(this.Paginator.Pagination), new Lazy<object>(GetPagination) },
            };
        }

        private object GetDocuments()
        {
            var documents = new List<DynamicDocumentFile>();

            foreach (var document in this.Paginator.Documents)
            {
                this.ActiveDocument.AddContributingFile(document);
                documents.Add(new DynamicDocumentFile(this.ActiveDocument, document, this.Site));
            }

            return documents;
        }

        private object GetPagination()
        {
            if (this.Paginator.Pagination != null)
            {
                return new DynamicPagination(this.ActiveDocument, this.Paginator.Pagination);
            }

            return null;
        }
    }
}
