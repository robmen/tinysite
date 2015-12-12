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
            : this(id, chapters, parentDocument, null)
        {
        }

        public Book(string id, List<BookPage> chapters, DocumentFile parentDocument, DocumentFile renderingDocument)
        {
            this.Id = id;

            this.ParentDocument = parentDocument;

            this.Chapters = chapters;

            this.RenderingDocument = renderingDocument;
        }

        public string Id { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public IEnumerable<BookPage> Chapters { get { return this.Get<IEnumerable<BookPage>>(); } private set { this.Set<IEnumerable<BookPage>>(value); } }

        public DocumentFile ParentDocument { get { return this.Get<DocumentFile>(); } private set { this.Set<DocumentFile>(value); } }

        internal DocumentFile RenderingDocument { get; private set; }

        public Book GetBookWithRenderingDocument(DocumentFile renderingDocument)
        {
            var chapters = this.Chapters.Select(c => c.GetWithRenderingDocument(renderingDocument)).ToList();

            return new Book(this.Id, chapters, this.ParentDocument, renderingDocument);
        }
    }
}
