using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TinySite.Extensions;
using TinySite.Models;

namespace TinySite.Commands
{
    public class OrderCommand
    {
        public IEnumerable<DocumentFile> Documents { private get; set; }

        public IEnumerable<Book> Books { get; private set; }

        public void Execute()
        {
            var documentsById = this.Documents.ToLookup(d => d.Id);

            this.ProcessImplicitOrder(documentsById);

            var books = this.ProcessExplicitOrder(documentsById);

            this.Books = books;
        }

        private void ProcessImplicitOrder(ILookup<string, DocumentFile> documentsById)
        {
            var unordered = this.Documents.Where(d => d.Order < 1)
                .OrderBy(d => d.Date)
                .ToList();

            var unorderedGroupedByParent = unordered.GroupBy(d => d.ParentId);

            foreach (var groupedDocuments in unorderedGroupedByParent)
            {
                DocumentFile parent = documentsById[groupedDocuments.Key].FirstOrDefault();

                DocumentFile previous = null;

                foreach (var document in groupedDocuments)
                {
                    previous = SetNextPreviousAndParent(previous, document, parent);
                }
            }
        }

        private IEnumerable<Book> ProcessExplicitOrder(ILookup<string, DocumentFile> documentsById)
        {
            var ordered = this.Documents.Where(d => d.Order > 0)
                .OrderBy(d => d.Order)
                .ToList();

            var orderedGroupedByParent = ordered.GroupBy(d => d.ParentId);

            var documentsToChapter = new Dictionary<DocumentFile, BookPage>();

            var books = new List<Book>();

            foreach (var groupedDocuments in orderedGroupedByParent)
            {
                var parentDocument = documentsById[groupedDocuments.Key].FirstOrDefault();

                // If there is no parent document or the parent document is not an ordered
                // document then this set of grouped documents must be a set of chapters in
                // a new book.
                if (parentDocument == null || parentDocument.Order == 0)
                {
                    var chapters = new List<BookPage>();

                    foreach (var document in groupedDocuments)
                    {
                        if (!documentsToChapter.TryGetValue(document, out var chapter))
                        {
                            chapter = new BookPage(document, true);
                            documentsToChapter.Add(document, chapter);
                        }

                        Debug.Assert(chapter.Document == document);

                        chapters.Add(chapter);
                    }

                    var book = new Book(groupedDocuments.Key, chapters, parentDocument, null);

                    books.Add(book);
                }
                else
                {
                    // If the parent document has not been processed yet, make it a chapter page now.
                    if (!documentsToChapter.TryGetValue(parentDocument, out var chapter))
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
            }

            foreach (var book in books)
            {
                DocumentFile previous = null;

                foreach (var chapter in book.Chapters)
                {
                    previous = ProcessBookAndChapterOrder(book, chapter, book.ParentDocument, previous);
                }
            }

            return books;
        }

        private static DocumentFile ProcessBookAndChapterOrder(Book book, BookPage chapter, DocumentFile parent, DocumentFile previous)
        {
            var bookWithActiveDocument = book.GetBookWithRenderingDocument(chapter.Document);

            chapter.Document.Book = bookWithActiveDocument;

            chapter.Document.Chapter = AllChapters(bookWithActiveDocument).Where(c => c.Document == chapter.Document).Single();

            previous = SetNextPreviousAndParent(previous, chapter.Document, parent);

            foreach (var page in chapter.SubPages)
            {
                previous = SetNextPreviousAndParent(previous, page.Document, chapter.Document);

                if (page.SubPages != null && page.SubPages.Any())
                {
                    previous = ProcessBookAndChapterOrder(book, page, chapter.Document, previous);
                }
                else
                {
                    page.Document.Book = book.GetBookWithRenderingDocument(page.Document);
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
                    queue.EnqueueRange(page.SubPages);

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

            return document;
        }
    }
}
