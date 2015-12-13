using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class DynamicBook : DynamicBase
    {
        public DynamicBook(DocumentFile activeDocument, Book book)
            : base(null)
        {
            this.ActiveDocument = activeDocument;
            this.Book = book;
        }

        private DocumentFile ActiveDocument { get; }

        private Book Book { get; }

        protected override IDictionary<string, object> GetData()
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(this.Book.Id), this.Book.Id },
                { nameof(this.Book.Chapters), new Lazy<object>(GetChapters) },
                { nameof(this.Book.ParentDocument), new Lazy<object>(GetParentDocument) },
            };
        }

        private object GetChapters()
        {
            var chapters = new List<DynamicBookPage>();

            foreach (var chapter in this.Book.Chapters)
            {
                this.ActiveDocument.AddContributingFile(chapter.Document);
                chapters.Add(new DynamicBookPage(this.ActiveDocument, chapter));
            }

            return chapters;
        }

        private object GetParentDocument()
        {
            this.ActiveDocument.AddContributingFile(this.Book.ParentDocument);
            return new DynamicDocumentFile(this.ActiveDocument, this.Book.ParentDocument);
        }
    }
}