using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    [DebuggerDisplay("DocumentFile: {Id}, Source: {SourceRelativePath}")]
    public class DocumentFile : OutputFile
    {
        private object _lock = new object();

        public DocumentFile(string path, string rootPath, string outputPath, string outputRootPath, string url, string rootUrl, Author author, MetadataCollection metadata)
            : base(path, rootPath, outputPath, outputRootPath, rootUrl, url)
        {
            metadata.Assign(this);

            this.Author = author;

            this.Now = DateTime.Now;
        }

        private DocumentFile(DocumentFile original)
            : base(original)
        {
            this.ExtensionsForRendering = new List<string>(original.ExtensionsForRendering);

            this.Partial = Partial;
        }

        internal IList<string> ExtensionsForRendering { get; set; }

        internal bool Partial { get; set; }

        internal bool Rendered { get; set; }

        internal string RenderedContent { get; set; }

        public Author Author { get { return this.Get<Author>(); } set { this.Set<Author>(value); } }

        public string Content { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public bool Draft { get { return this.Get<bool>(); } set { this.Set<bool>(value); } }

        public string Id { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public int Order { get { return this.Get<int>(); } set { this.Set<int>(value); } }

        public string PaginateQuery { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string ParentId { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string SourceContent { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string Summary { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public DocumentFile NextDocument { get { return this.Get<DocumentFile>(); } set { this.Set<DocumentFile>(value); } }

        public DocumentFile ParentDocument { get { return this.Get<DocumentFile>(); } set { this.Set<DocumentFile>(value); } }

        public DocumentFile PreviousDocument { get { return this.Get<DocumentFile>(); } set { this.Set<DocumentFile>(value); } }

        public Book Book { get { return this.Get<Book>(); } set { this.Set<Book>(value); } }

        public BookPage Chapter { get { return this.Get<BookPage>(); } set { this.Set<BookPage>(value); } }

        public Paginator Paginator { get { return this.Get<Paginator>(); } set { this.Set<Paginator>(value); } }

        public DateTime Now { get { return this.Get<DateTime>(); } set { this.SetTimes("Now", value); } }

        public DocumentFile Clone()
        {
            var clone = new DocumentFile(this);
            return clone;
        }
    }
}
