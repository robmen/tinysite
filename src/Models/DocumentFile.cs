using System;
using System.Collections.Generic;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
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

        public DocumentFile Clone()
        {
            var clone = new DocumentFile(this);
            return clone;
        }

        public dynamic GetAsDynamic(string documentContent = null, bool expandNextPrev = true)
        {
            var now = DateTime.Now;

            dynamic data = new CaseInsenstiveExpando();

            this.Metadata.Assign(data as IDictionary<string, object>);

            data.Author = this.Author;
            data.Draft = this.Draft;
            data.Modified = this.Modified;
            data.Id = this.Id;
            data.Order = this.Order;
            data.OutputPath = this.OutputPath;
            data.RelativePath = this.OutputRelativePath; // TODO: rename "OutputPath" to "RelativeOutputPath".
            data.SourcePath = this.SourcePath;
            data.SourceContent = this.SourceContent;
            data.Url = this.RelativeUrl; // TODO: make the dyanmic object "url" fields match the document fields.
            data.RootUrl = this.RootUrl;
            data.FullUrl = this.Url;
            data.Date = this.Date;
            data.DateUtc = this.Date.ToUniversalTime();
            data.FriendlyDate = this.Date.ToString("D");
            data.StandardUtcDate = this.Date.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
            data.Now = now;
            data.NowUtc = now.ToUniversalTime();
            data.NowFriendlyDate = now.ToString("D");
            data.NowStandardUtcDate = now.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
            data.Content = String.IsNullOrEmpty(documentContent) ? this.Content : documentContent;
            data.Summary = this.Summary;

            if (this.NextDocument != null && expandNextPrev)
            {
                data.NextDocument = this.NextDocument.GetAsDynamic(null, false);
            }

            if (this.ParentDocument != null && expandNextPrev)
            {
                data.ParentDocument = this.ParentDocument.GetAsDynamic(null, false);
            }

            if (this.PreviousDocument != null && expandNextPrev)
            {
                data.PreviousDocument = this.PreviousDocument.GetAsDynamic(null, false);
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
