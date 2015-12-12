using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TinySite.Extensions;

namespace TinySite.Models
{
    [DebuggerDisplay("BookPage: {Document.Id}, Source: {Document.SourceRelativePath}")]
    public class BookPage : CaseInsensitiveExpando
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

        public bool Active { get { return this.Get<bool>(); } set { this.Set<bool>(value); } }

        public bool Chapter { get { return this.Get<bool>(); } private set { this.Set<bool>(value); } }

        public bool SubPageActive { get { return this.Get<bool>(); } set { this.Set<bool>(value); } }

        public DocumentFile Document { get { return this.Get<DocumentFile>(); } private set { this.Set<DocumentFile>(value); } }

        public IList<BookPage> SubPages { get { return this.Get<IList<BookPage>>(); } private set { this.Set<IList<BookPage>>(value); } }

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
