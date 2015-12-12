using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class DynamicRenderingPaginator : DynamicRenderingObject
    {
        public DynamicRenderingPaginator(DocumentFile activeDocument, Paginator Paginator)
            : base(null)
        {
            this.ActiveDocument = activeDocument;
            this.Paginator = Paginator;
        }

        private DocumentFile ActiveDocument { get; }

        private Paginator Paginator { get; }

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
            var documents = new List<DynamicRenderingDocument>();

            foreach (var document in this.Paginator.Documents)
            {
                this.ActiveDocument.AddContributingFile(document);
                documents.Add(new DynamicRenderingDocument(this.ActiveDocument, document));
            }

            return documents;
        }

        private object GetPagination()
        {
            if (this.Paginator.Pagination != null)
            {
                return new DynamicRenderingPagination(this.ActiveDocument, this.Paginator.Pagination);
            }

            return null;
        }
    }
}