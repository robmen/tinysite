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

            var documentsToChapter = new Dictionary<DocumentFile, BookPage>();

            var books = new List<Book>();

            foreach (var groupedDocuments in orderedGroupedByParent)
            {
                DocumentFile parentDocument;

                if (documentsById.TryGetValue(groupedDocuments.Key, out parentDocument))
                {
                    BookPage chapter;

                    // If the parent document has not been processed yet, make it a chapter page now.
                    if (!documentsToChapter.TryGetValue(parentDocument, out chapter))
                    {
                        chapter = new BookPage(parentDocument, true);

                        documentsToChapter.Add(parentDocument, chapter);
                    }
                    else
                    {
                        // If the document is not already a chapter, convert the existing
                        // page for the document into a sub-chapter.
                        if (chapter.Document != parentDocument)
                        {
                            for (var i = 0; i < chapter.SubPages.Count; ++i)
                            {
                                var page = chapter.SubPages[i];

                                if (page.Document == parentDocument)
                                {
                                    Debug.Assert(!page.Chapter);

                                    var subChapter = new BookPage(parentDocument, true);

                                    chapter.SubPages[i] = subChapter;
                                    documentsToChapter[parentDocument] = chapter;

                                    chapter = subChapter;
                                    break;
                                }
                            }
                        }
                    }

                    foreach (var document in groupedDocuments)
                    {
                        chapter.SubPages.Add(new BookPage(document));

                        documentsToChapter.Add(document, chapter);
                    }
                }
                else // no parent, documents must be a set of chapters in a new book.
                {
                    var chapters = new List<BookPage>();

                    foreach (var document in groupedDocuments)
                    {
                        BookPage chapter;

                        if (!documentsToChapter.TryGetValue(document, out chapter))
                        {
                            chapter = new BookPage(document, true);
                        }

                        Debug.Assert(chapter.Document == document);

                        chapters.Add(chapter);
                        documentsToChapter.Add(document, chapter);
                    }

                    var book = new Book(groupedDocuments.Key, chapters);

                    books.Add(book);
                }
            }

            foreach (var book in books)
            {
                DocumentFile previous = null;

                foreach (var chapter in book.Chapters)
                {
                    previous = ProcessBookAndChapterOrder(book, chapter, null, previous);
                }
            }

            return books;
        }

        private static DocumentFile ProcessBookAndChapterOrder(Book book, BookPage chapter, BookPage parent, DocumentFile previous)
        {
            var bookWithActiveDocument = book.GetBookWithActiveDocument(chapter.Document);

            chapter.Document.Book = bookWithActiveDocument;

            chapter.Document.Chapter = AllChapters(bookWithActiveDocument).Where(c => c.Document == chapter.Document).Single();

            previous = SetNextPreviousAndParent(previous, chapter.Document, parent == null ? null : parent.Document);

            foreach (var page in chapter.SubPages)
            {
                previous = SetNextPreviousAndParent(previous, page.Document, chapter.Document);

                if (page.SubPages != null && page.SubPages.Any())
                {
                    previous = ProcessBookAndChapterOrder(book, page, chapter, previous);
                }
                else
                {
                    page.Document.Book = book.GetBookWithActiveDocument(page.Document);
                }
            }

            return previous;
        }

        private static IEnumerable<BookPage> AllChapters(Book book)
        {
            var queue = new Queue<BookPage>(book.Chapters);

            while (queue.Count > 0)
            {
                var page = queue.Dequeue();

                if (page.Chapter)
                {
                    foreach (var subPage in page.SubPages)
                    {
                        queue.Enqueue(subPage);
                    }

                    yield return page;
                }
            }
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
