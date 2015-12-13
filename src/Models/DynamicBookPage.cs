using System;
using System.Collections.Generic;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class DynamicBookPage : DynamicBase
    {
        public DynamicBookPage(DocumentFile activeDocument, BookPage BookPage)
            : base(null)
        {
            this.ActiveDocument = activeDocument;
            this.BookPage = BookPage;
        }

        private DocumentFile ActiveDocument { get; }

        private BookPage BookPage { get; }

        protected override IDictionary<string, object> GetData()
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(this.BookPage.Chapter), this.BookPage.Chapter },
                { "Active", new Lazy<object>(GetActive) },
                { "SubPageActive", new Lazy<object>(GetSubPageActive) },
                { nameof(this.BookPage.Document), new Lazy<object>(GetDocument) },
                { nameof(this.BookPage.SubPages), new Lazy<object>(GetSubPages) },
            };
        }

        private object GetActive()
        {
            return (this.BookPage.Document == this.ActiveDocument);
        }

        private object GetSubPageActive()
        {
            if (this.BookPage.SubPages == null)
            {
                return false;
            }

            // If this page is active, then no sub pages are active.
            //
            if (this.BookPage.Document == this.ActiveDocument)
            {
                return false;
            }

            var pages = new Queue<BookPage>(this.BookPage.SubPages);

            while (pages.Count > 0)
            {
                var page = pages.Dequeue();

                if (page.Document == this.ActiveDocument)
                {
                    return true;
                }

                if (page.SubPages != null)
                {
                    pages.EnqueueRange(page.SubPages);
                }
            }

            return false;
        }

        private DynamicDocumentFile GetDocument()
        {
            this.ActiveDocument.AddContributingFile(this.BookPage.Document);
            return new DynamicDocumentFile(this.ActiveDocument, this.BookPage.Document);
        }

        private IEnumerable<DynamicBookPage> GetSubPages()
        {
            var pages = new List<DynamicBookPage>(this.BookPage.SubPages.Count);

            foreach (var page in this.BookPage.SubPages)
            {
                this.ActiveDocument.AddContributingFile(page.Document);
                pages.Add(new DynamicBookPage(this.ActiveDocument, page));
            }

            return pages;
        }
    }
}