using System;
using System.Collections.Generic;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class DocumentFile : OutputFile
    {
        public DocumentFile(string path, string rootPath, string outputRootPath, string url, string rootUrl, Author author)
            : base(path, rootPath, outputRootPath, url, rootUrl)
        {
            this.Author = author;
        }

        private DocumentFile(DocumentFile original)
            : base(original)
        {
            this.Author = original.Author;
            this.Date = original.Date;
            this.Draft = original.Draft;
            this.ExtensionsForRendering = new List<string>(original.ExtensionsForRendering);
            this.Metadata = new MetadataCollection(original.Metadata);
            this.Paginate = original.Paginate; // TODO: probably should not be shallow copying this, right?

            this.SourceContent = original.SourceContent;
            this.Summary = original.Summary;
        }

        public Author Author { get; set; }

        public string Content { get; set; }

        public DateTime Date { get; set; }

        public bool Draft { get; set; }

        public IList<string> ExtensionsForRendering { get; set; }

        public MetadataCollection Metadata { get; set; }

        public int Paginate { get; set; }

        public bool Rendered { get; set; }

        public string RenderedContent { get; set; }

        public string SourceContent { get; set; }

        public string Summary { get; set; }

        public DocumentFile Clone()
        {
            var clone = new DocumentFile(this);
            return clone;
        }

        public dynamic GetAsDynamic(string documentContent = null)
        {
            var now = DateTime.Now;

            dynamic data = new CaseInsenstiveExpando();

            this.Metadata.Assign(data as IDictionary<string, object>);

            data.Author = this.Author;
            data.Modified = this.Modified;
            data.OutputPath = this.OutputPath;
            data.RelativePath = this.RelativePath;
            data.SourcePath = this.SourcePath;
            data.SourceContent = this.SourceContent;
            data.Url = this.Url;
            data.RootUrl = this.RootUrl;
            data.FullUrl = this.RootUrl.EnsureEndsWith("/") + this.Url.TrimStart('/');
            data.Draft = this.Draft;
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

            return data;
        }

        public void UpdateOutputPaths(string appendPath, string updateFileName)
        {
            if (String.IsNullOrEmpty(updateFileName))
            {
                updateFileName = Path.GetFileName(this.RelativePath);
            }

            var updateInUrl = String.IsNullOrEmpty(appendPath) ? String.Empty : appendPath.Replace('\\', '/') + '/';

            this.RelativePath = Path.Combine(Path.GetDirectoryName(this.RelativePath), appendPath, updateFileName);

            this.OutputPath = Path.Combine(Path.GetDirectoryName(this.OutputPath), appendPath, updateFileName);

            var lastSlash = this.Url.LastIndexOf('/');

            this.Url = String.Concat(this.Url.Substring(0, lastSlash + 1), updateInUrl, updateFileName.Equals("index.html", StringComparison.OrdinalIgnoreCase) ? String.Empty : updateFileName);
        }
    }
}
