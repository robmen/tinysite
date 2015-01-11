using System;
using System.Collections.Generic;
using System.Linq;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class Site
    {
        public Site(SiteConfig config, IEnumerable<DocumentFile> documents, IEnumerable<StaticFile> files, IEnumerable<LayoutFile> layouts, Site parent = null)
        {
            this.Author = config.Author;
            this.DocumentsPath = config.DocumentsPath;
            this.FilesPath = config.FilesPath;
            this.LayoutsPath = config.LayoutsPath;
            this.OutputPath = config.OutputPath;
            this.Parent = parent;
            this.TimeZone = config.TimeZone;
            this.Url = config.Url.EnsureEndsWith("/");
            this.RootUrl = config.RootUrl.EnsureEndsWith("/");
            this.Metadata = new MetadataCollection(config.Metadata);

            this.Documents = documents.ToList();

            this.Files = files.ToList();

            this.Layouts = new LayoutFileCollection(layouts);
        }

        public Author Author { get; set; }

        public string DocumentsPath { get; set; }

        public string FilesPath { get; set; }

        public string LayoutsPath { get; set; }

        public string OutputPath { get; set; }

        public Site Parent { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public string Url { get; set; }

        public string RootUrl { get; set; }

        public IList<DocumentFile> Documents { get; set; }

        public IList<StaticFile> Files { get; set; }

        public LayoutFileCollection Layouts { get; set; }

        public MetadataCollection Metadata { get; set; }

        public IEnumerable<Book> Books { get; set; }

        public dynamic GetAsDynamic()
        {
            dynamic data = new CaseInsenstiveExpando();

            this.Metadata.Assign(data as IDictionary<string, object>);

            data.Author = this.Author;
            data.Output = this.OutputPath;
            data.Url = this.Url;
            data.RootUrl = this.RootUrl;
            data.FullUrl = this.RootUrl.EnsureEndsWith("/") + this.Url.TrimStart('/');
            data.Parent = this.Parent;
            data.TimeZoneInfo = this.TimeZone;

            return data;
        }
    }
}
