using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TinySite.Models;

namespace TinySite.Commands
{
    public class OrderCommand
    {
        public IEnumerable<DocumentFile> Documents { private get; set; }

        public IEnumerable<Book> Books { get; private set; }

        public void Execute()
        {
            var documentsById = this.Documents.ToDictionary(d => d.Id);

            this.ProcessImplicitOrder(documentsById);

            var books = this.ProcessExplicitOrder(documentsById);

            this.Books = books;
        }

        private void ProcessImplicitOrder(IDictionary<string, DocumentFile> documentsById)
        {
            var unordered = this.Documents.Where(d => d.Order < 1)
                .OrderBy(d => d.Date)
                .ToList();

            var unorderedGroupedByParent = unordered.GroupBy(d => d.ParentId);

            foreach (var groupedDocuments in unorderedGroupedByParent)
            {
                DocumentFile parent = null;
                documentsById.TryGetValue(groupedDocuments.Key, out parent);

                DocumentFile previous = null;

                foreach (var document in groupedDocuments)
                {
                    previous = SetNextPreviousAndParent(previous, document, parent);
                }
            }
        }

        private IEnumerable<Book> ProcessExplicitOrder(IDictionary<string, DocumentFile> documentsById)
        {
            var ordered = this.Documents.Where(d => d.Order > 0)
                .OrderBy(d => d.Order)
                .ToList();

            var orderedGroupedByParent = ordered.GroupBy(d => d.ParentId);

            var documentsToChapter = new Dictionary<DocumentFile, BookChapter>();

            var books = new List<Book>();

            foreach (var groupedDocuments in orderedGroupedByParent)
            {
                DocumentFile parentDocument;

                if (documentsById.TryGetValue(groupedDocuments.Key, out parentDocument))
                {
                    BookChapter chapter;

                    // If the parent document has not been processed yet, make it a book chapter now.
                    if (!documentsToChapter.TryGetValue(parentDocument, out chapter))
                    {
                        chapter = new BookChapter();
                        chapter.Document = parentDocument;

                        documentsToChapter.Add(parentDocument, chapter);
                    }
                    else
                    {
                        // If the document is not already a chapter, convert the existing
                        // page for the document into a sub-chapter.
                        if (chapter.Document != parentDocument)
                        {
                            for (var i = 0; i < chapter.PagesOrSubChapters.Count; ++i)
                            {
                                var page = chapter.PagesOrSubChapters[i];

                                if (page.Document == parentDocument)
                                {
                                    Debug.Assert(!(page is BookChapter));

                                    var subChapter = new BookChapter();
                                    subChapter.Document = parentDocument;
                                    //chapter.ParentChapter = parentChapter;

                                    chapter.PagesOrSubChapters[i] = subChapter;
                                    documentsToChapter[parentDocument] = chapter;

                                    chapter = subChapter;
                                    break;
                                }
                            }
                        }
                    }

                    foreach (var document in groupedDocuments)
                    {
                        chapter.PagesOrSubChapters.Add(new BookPage() { /*ParentChapter = parentChapter,*/ Document = document });
                        documentsToChapter.Add(document, chapter);
                    }
                }
                else // no parent, documents must be a set of chapters in a new book.
                {
                    var chapters = new List<BookChapter>();

                    foreach (var document in groupedDocuments)
                    {
                        BookChapter chapter;

                        if (!documentsToChapter.TryGetValue(document, out chapter))
                        {
                            chapter = new BookChapter();
                            chapter.Document = document;
                        }

                        Debug.Assert(chapter.Document == document);

                        chapters.Add(chapter);
                        documentsToChapter.Add(document, chapter);
                    }

                    var book = new Book();
                    book.Id = groupedDocuments.Key;
                    book.Chapters = chapters;

                    books.Add(book);
                }
            }

            foreach (var book in books)
            {
                DocumentFile previous = null;

                foreach (var chapter in book.Chapters)
                {
                    previous = ProcessChapterOrder(chapter, null, previous);
                }
            }

            return books;
        }

        private static DocumentFile ProcessChapterOrder(BookChapter chapter, BookChapter parent, DocumentFile previous)
        {
            previous = SetNextPreviousAndParent(previous, chapter.Document, parent == null ? null : parent.Document);

            foreach (var page in chapter.PagesOrSubChapters)
            {
                previous = SetNextPreviousAndParent(previous, page.Document, chapter.Document);

                var subChapter = page as BookChapter;

                if (subChapter != null)
                {
                    previous = ProcessChapterOrder(subChapter, chapter, previous);
                }
            }

            return previous;
        }

        private static DocumentFile SetNextPreviousAndParent(DocumentFile previous, DocumentFile document, DocumentFile parent)
        {
            if (previous != null)
            {
                previous.NextDocument = document;
            }

            document.ParentDocument = parent;
            document.PreviousDocument = previous;

            previous = document;
            return previous;
        }
    }
}
