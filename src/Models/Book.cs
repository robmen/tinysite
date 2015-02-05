using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TinySite.Extensions;

namespace TinySite.Models
{
    [DebuggerDisplay("Book: {Id}")]
    public class Book : CaseInsensitiveExpando
    {
        public string Id { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public IEnumerable<BookPage> Chapters { get { return this.Get<IEnumerable<BookPage>>(); } set { this.Set<IEnumerable<BookPage>>(value); } }

        public Book GetBookWithActiveDocument(DocumentFile activeDocument)
        {
            var book = this;

            var chapters = this.Chapters.Select(c => c.GetWithActiveDocument(activeDocument)).ToList();

            if (chapters.Any(c => c.Active || c.SubPageActive))
            {
                book = new Book() { Id = this.Id };

                book.Chapters = chapters;
            }

            return book;
        }
    }
}
