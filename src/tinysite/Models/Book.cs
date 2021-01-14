using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TinySite.Models
{
    [DebuggerDisplay("Book: {Id}")]
    public class Book
    {
        public Book(string id, List<BookPage> chapters, DocumentFile parentDocument, DocumentFile renderingDocument)
        {
            this.Id = id;

            this.ParentDocument = parentDocument;

            this.Chapters = chapters;

            this.RenderingDocument = renderingDocument;
        }

        public string Id { get; }

        public IEnumerable<BookPage> Chapters { get; }

        public DocumentFile ParentDocument { get; }

        internal DocumentFile RenderingDocument { get; private set; }

        public Book GetBookWithRenderingDocument(DocumentFile renderingDocument)
        {
            var chapters = this.Chapters.Select(c => c.GetWithRenderingDocument(renderingDocument)).ToList();

            return new Book(this.Id, chapters, this.ParentDocument, renderingDocument);
        }
    }
}
