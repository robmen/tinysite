using System;
using System.Collections.Generic;

namespace TinySite.Models.Dynamic
{
    public class DynamicPagination : DynamicBase
    {
        public DynamicPagination(DocumentFile activeDocument, Pagination Pagination)
        {
            this.ActiveDocument = activeDocument;
            this.Pagination = Pagination;
        }

        private DocumentFile ActiveDocument { get; }

        private Pagination Pagination { get; }

        protected override IDictionary<string, object> GetData()
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(this.Pagination.Page), this.Pagination.Page },
                { nameof(this.Pagination.Pages), new Lazy<object>(GetPages) },
                { nameof(this.Pagination.PerPage), this.Pagination.PerPage },
                { nameof(this.Pagination.NextPageUrl), this.Pagination.NextPageUrl },
                { nameof(this.Pagination.PreviousPageUrl), this.Pagination.PreviousPageUrl },
                { nameof(this.Pagination.TotalPage), this.Pagination.TotalPage },
            };
        }

        private object GetPages()
        {
            var pages = new List<DynamicPage>();

            foreach (var page in this.Pagination.Pages)
            {
                pages.Add(new DynamicPage(this.ActiveDocument, page));
            }

            return pages;
        }
    }
}