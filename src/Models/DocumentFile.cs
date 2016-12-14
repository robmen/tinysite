using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TinySite.Models
{
    [DebuggerDisplay("DocumentFile: {Id}, Source: {SourceRelativePath}")]
    public class DocumentFile : OutputFile
    {
        private object _lock = new object();

        public DocumentFile(string path, string rootPath, string outputPath, string outputRootPath, string url, string rootUrl, Author author, MetadataCollection metadata, IDictionary<string, string> queries)
            : base(path, rootPath, outputPath, outputRootPath, rootUrl, url)
        {
            this.Now = DateTime.Now;

            this.Author = author;

            if (metadata != null)
            {
                this.Layout = metadata.Get<string>("layout");
                metadata.Remove("layout");

                this.Metadata = metadata;
            }

            this.Queries = queries;
        }

        private DocumentFile(DocumentFile original)
            : base(original)
        {
            this.ExtensionsForRendering = new List<string>(original.ExtensionsForRendering);
            this.Layouts = new List<LayoutFile>(original.Layouts);
            this.Queries = original.Queries;
            this.Partial = original.Partial;

            this.Author = original.Author;
            this.Layout = original.Layout;
            this.Content = original.Content;
            this.Description = original.Description;
            this.Draft = original.Draft;
            this.Id = original.Id;
            this.Order = original.Order;
            this.PaginateQuery = original.PaginateQuery;
            this.ParentId = original.ParentId;
            this.SourceContent = original.SourceContent;
            this.Summary = original.Summary;
            this.NextDocument = original.NextDocument;
            this.ParentDocument = original.ParentDocument;
            this.PreviousDocument = original.PreviousDocument;
            this.Book = original.Book;
            this.Chapter = original.Chapter;
            this.Paginator = original.Paginator;
            this.Now = original.Now;
            this.Metadata = original.Metadata;

            this.Cloned = true;
        }

        internal IList<string> ExtensionsForRendering { get; set; }

        internal IEnumerable<LayoutFile> Layouts { get; private set; }

        internal IDictionary<string, string> Queries { get; }

        internal bool Cloned { get; }

        public bool Partial { get; internal set; }

        public bool Rendered { get; internal set; }

        public string RenderedContent { get; internal set; }

        public Author Author { get; }

        public string Layout { get; }

        public string Content { get; set; }

        public string Description { get; set; }

        public bool Draft { get; set; }

        public string Id { get; set; }

        public int Order { get; set; }

        public string PaginateQuery { get; set; }

        public string ParentId { get; set; }

        public string SourceContent { get; set; }

        public string Summary { get; set; }

        public DocumentFile NextDocument { get; set; }

        public DocumentFile ParentDocument { get; set; }

        public DocumentFile PreviousDocument { get; set; }

        public Book Book { get; set; }

        public BookPage Chapter { get; set; }

        public Paginator Paginator { get; set; }

        public DateTime Now { get; }

        public DateTime NowUtc => this.Now.ToUniversalTime();

        public string NowFriendlyDate => this.Now.ToString("D");

        public string NowStandardUtcDate => this.NowUtc.ToString("yyyy-MM-ddThh:mm:ssZ");

        public MetadataCollection Metadata { get; }

        public DocumentFile CloneForPage(string urlFormat, string prependPathFormat, Paginator paginator)
        {
            var prependPath = String.Format(prependPathFormat, paginator.Pagination.Page);

            var prependUrl = String.Format(urlFormat, paginator.Pagination.Page);

            var dupe = new DocumentFile(this);

            var updateFileName = Path.GetFileName(dupe.OutputRelativePath);

            dupe.OutputRelativePath = Path.Combine(prependPath, updateFileName);

            dupe.OutputPath = Path.Combine(dupe.OutputRootPath, prependPath, updateFileName);

            dupe.RelativeUrl = String.Concat(prependUrl, updateFileName.Equals("index.html", StringComparison.OrdinalIgnoreCase) ? String.Empty : updateFileName);

            dupe.Paginator = paginator;

            return dupe;
        }

        public void AssignLayouts(IEnumerable<LayoutFile> layouts)
        {
            this.Layouts = layouts.ToList();
        }
    }
}
