using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TinySite.Extensions;

namespace TinySite.Models
{
    [DebuggerDisplay("Book: {Id}")]
    public class Book : CaseInsensitiveExpando
    {
        public Book(string id, List<BookPage> chapters, DocumentFile parentDocument)
        {
            this.Id = id;

            this.ParentDocument = parentDocument;

            this.Chapters = chapters;
        }

        public string Id { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public IEnumerable<BookPage> Chapters { get { return this.Get<IEnumerable<BookPage>>(); } private set { this.Set<IEnumerable<BookPage>>(value); } }

        public DocumentFile ParentDocument { get { return this.Get<DocumentFile>(); } private set { this.Set<DocumentFile>(value); } }

        public Book GetBookWithActiveDocument(DocumentFile activeDocument)
        {
            var book = this;

            var chapters = this.Chapters.Select(c => c.GetWithActiveDocument(activeDocument)).ToList();

            if (chapters.Any(c => c.Active || c.SubPageActive))
            {
                book = new Book(this.Id, chapters, this.ParentDocument);
            }

            return book;
        }
    }
}
