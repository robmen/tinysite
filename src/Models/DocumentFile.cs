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
        public DocumentFile(string path, string rootPath, string outputPath, string outputRootPath, string url, string rootUrl, Author author)
            : base(path, rootPath, outputPath, outputRootPath, rootUrl, url)
        {
            this.Author = author;
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
            this.Paginator = original.Paginator;
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

        public Paginator Paginator { get; set; }

        public DocumentFile Clone()
        {
            var clone = new DocumentFile(this);
            return clone;
        }

        public dynamic GetAsDynamic(string documentContent = null, bool expandNextPrev = true)
        {
            var now = DateTime.Now;

            var data = new CaseInsensitiveExpando();

            this.Metadata.Assign(data);

            data.Add("Author", this.Author);
            data.Add("Draft", this.Draft);
            data.Add("Modified", this.Modified);
            data.Add("Id", this.Id);
            data.Add("Order", this.Order);
            data.Add("OutputPath", this.OutputPath);
            data.Add("RelativePath", this.OutputRelativePath); // TODO: rename "OutputPath" to "RelativeOutputPath".
            data.Add("SourcePath", this.SourcePath);
            data.Add("SourceContent", this.SourceContent);
            data.Add("Url", this.RelativeUrl); // TODO: make the dyanmic object "url" fields match the document fields.
            data.Add("RootUrl", this.RootUrl);
            data.Add("FullUrl", this.Url);
            data.Add("Date", this.Date);
            data.Add("DateUtc", this.Date.ToUniversalTime());
            data.Add("FriendlyDate", this.Date.ToString("D"));
            data.Add("StandardUtcDate", this.Date.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ"));
            data.Add("Now", now);
            data.Add("NowUtc", now.ToUniversalTime());
            data.Add("NowFriendlyDate", now.ToString("D"));
            data.Add("NowStandardUtcDate", now.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ"));
            data.Add("Content", String.IsNullOrEmpty(documentContent) ? this.Content : documentContent);
            data.Add("Summary", this.Summary);

            if (this.NextDocument != null && expandNextPrev)
            {
                data.Add("NextDocument", this.NextDocument.GetAsDynamic(null, false));
            }

            if (this.ParentDocument != null && expandNextPrev)
            {
                data.Add("ParentDocument", this.ParentDocument.GetAsDynamic(null, false));
            }

            if (this.PreviousDocument != null && expandNextPrev)
            {
                data.Add("PreviousDocument", this.PreviousDocument.GetAsDynamic(null, false));
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
    }
}
