using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TinySite.Models
{
    [DebuggerDisplay("BookPage: {Document.Id}, Source: {Document.SourceRelativePath}")]
    public class BookPage
    {
        public BookPage(DocumentFile document, bool chapterPage = false)
        {
            this.Document = document;

            this.Chapter = chapterPage;

            if (this.Chapter)
            {
                this.SubPages = new List<BookPage>();
            }
        }

        public bool Active { get; private set; }

        public bool Chapter { get; }

        public bool SubPageActive { get; private set; }

        public DocumentFile Document { get; }

        public IList<BookPage> SubPages { get; private set; }

        public BookPage GetWithRenderingDocument(DocumentFile activeDocument)
        {
            var page = this;

            if (this.Document == activeDocument)
            {
                page = new BookPage(this.Document, this.Chapter);

                page.Active = true;

                page.SubPageActive = false;

                page.SubPages = this.SubPages;
            }
            else if (this.SubPages != null)
            {
                var subPages = this.SubPages.Select(p => p.GetWithRenderingDocument(activeDocument)).ToList();

                if (subPages.Any(c => c.Active || c.SubPageActive))
                {
                    page = new BookPage(this.Document, this.Chapter);

                    page.Active = false;

                    page.SubPageActive = true;

                    page.SubPages = subPages;
                }
            }

            return page;
        }
    }
}
