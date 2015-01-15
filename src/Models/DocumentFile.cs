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

        public DocumentFile(string path, string rootPath, string outputPath, string outputRootPath, string url, string rootUrl, Author author)
            : base(path, rootPath, outputPath, outputRootPath, rootUrl, url)
        {
            this.Author = author;

            this.Now = DateTime.Now;
        }

        private DocumentFile(DocumentFile original)
            : base(original)
        {
            this.Author = original.Author;
            this.Draft = original.Draft;
            this.Id = original.Id;
            this.ExtensionsForRendering = new List<string>(original.ExtensionsForRendering);
            this.Metadata = new MetadataCollection(original.Metadata);
            this.Order = original.Order;
            this.Paginate = original.Paginate; // TODO: probably should not be shallow copying this, right?
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
        }

        public Author Author { get; set; }

        public string Content { get; set; }

        public bool Draft { get; set; }

        public IList<string> ExtensionsForRendering { get; set; }

        public string Id { get; set; }

        public MetadataCollection Metadata { get; set; }

        public int Order { get; set; }

        public int Paginate { get; set; }

        public string ParentId { get; set; }

        public bool Rendered { get; set; }

        public string RenderedContent { get; set; }

        public string SourceContent { get; set; }

        public string Summary { get; set; }

        public DocumentFile NextDocument { get; set; }

        public DocumentFile ParentDocument { get; set; }

        public DocumentFile PreviousDocument { get; set; }

        public Book Book { get; set; }

        public BookChapter Chapter { get; set; }

        public Paginator Paginator { get; set; }

        private dynamic Dynamic { get; set; }

        private DateTime Now { get; set; }

        public DocumentFile Clone()
        {
            var clone = new DocumentFile(this);
            return clone;
        }

        public dynamic GetAsDynamic(string documentContent = null)
        {
            dynamic data = null;

            if (!String.IsNullOrEmpty(documentContent))
            {
                var expando = new CaseInsensitiveExpando();

                data = expando;

                this.AssignDocumentToExpando(expando, documentContent);
            }
            else
            {
                if (this.Dynamic == null)
                {
                    lock (_lock)
                    {
                        if (this.Dynamic == null)
                        {
                            var expando = new CaseInsensitiveExpando();

                            this.Dynamic = expando;

                            this.AssignDocumentToExpando(expando, null);
                        }
                    }
                }

                data = this.Dynamic;
            }

            return data;
        }

        public void UpdateOutputPaths(string appendPath, string updateFileName)
        {
            if (String.IsNullOrEmpty(updateFileName))
            {
                updateFileName = Path.GetFileName(this.OutputRelativePath);
            }

            var appendUrl = String.IsNullOrEmpty(appendPath) ? String.Empty : appendPath.Replace('\\', '/').EnsureEndsWith("/");

            this.OutputRelativePath = Path.Combine(Path.GetDirectoryName(this.OutputRelativePath), appendPath, updateFileName);

            this.OutputPath = Path.Combine(Path.GetDirectoryName(this.OutputPath), appendPath, updateFileName);

            var lastSlash = this.RelativeUrl.LastIndexOf('/');

            this.RelativeUrl = String.Concat(this.RelativeUrl.Substring(0, lastSlash + 1), appendUrl, updateFileName.Equals("index.html", StringComparison.OrdinalIgnoreCase) ? String.Empty : updateFileName);
        }

        private void AssignDocumentToExpando(CaseInsensitiveExpando data, string documentContent)
        {
            var expandNextPrev = String.IsNullOrEmpty(documentContent);

            this.Metadata.Assign(data);

            data["Author"] = this.Author;
            data["Draft"] = this.Draft;
            data["Modified"] = this.Modified;
            data["Id"] = this.Id;
            data["Order"] = this.Order;
            data["OutputPath"] = this.OutputPath;
            data["RelativePath"] = this.OutputRelativePath; // TODO: rename "OutputPath" to "RelativeOutputPath".
            data["SourcePath"] = this.SourcePath;
            data["SourceContent"] = this.SourceContent;
            data["Url"] = this.RelativeUrl; // TODO: make the dyanmic object "url" fields match the document fields.
            data["RootUrl"] = this.RootUrl;
            data["FullUrl"] = this.Url;
            data["Date"] = this.Date;
            data["DateUtc"] = this.Date.ToUniversalTime();
            data["FriendlyDate"] = this.Date.ToString("D");
            data["StandardUtcDate"] = this.Date.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
            data["Now"] = this.Now;
            data["NowUtc"] = this.Now.ToUniversalTime();
            data["NowFriendlyDate"] = this.Now.ToString("D");
            data["NowStandardUtcDate"] = this.Now.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
            data["Content"] = String.IsNullOrEmpty(documentContent) ? this.Content : documentContent;
            data["Summary"] = this.Summary;

            data["NextDocument"] = (this.NextDocument != null && expandNextPrev) ? this.NextDocument.GetAsDynamic() : null;
            data["ParentDocument"] = (this.ParentDocument != null && expandNextPrev) ? this.ParentDocument.GetAsDynamic() : null;
            data["PreviousDocument"] = (this.PreviousDocument != null && expandNextPrev) ? this.PreviousDocument.GetAsDynamic() : null;

            data["Book"] = this.Book == null ? null : this.Book.GetAsDynamic(this);
            data["Chapter"] = this.Chapter == null ? null : this.Chapter.GetAsDynamic(this);
            data["Paginator"] = this.Paginator == null ? null : this.Paginator.GetAsDynamic();
        }
    }
}
