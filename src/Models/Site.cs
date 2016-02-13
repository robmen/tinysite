using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class Site
    {
        public Site(SiteConfig config, IEnumerable<DataFile> data, IEnumerable<DocumentFile> documents, IEnumerable<StaticFile> files, IEnumerable<LayoutFile> layouts, Site parent = null)
            : this(config, data, documents, files, new LayoutFileCollection(layouts), parent)
        {
        }

        public Site(SiteConfig config, IEnumerable<DataFile> data, IEnumerable<DocumentFile> documents, IEnumerable<StaticFile> files, LayoutFileCollection layouts, Site parent = null)
        {
            this.DefaultLayoutForExtension = new Dictionary<string, string>(config.DefaultLayoutForExtension);
            this.IgnoreFiles = config.IgnoreFiles;

            this.SitePath = config.SitePath;

            this.Author = config.Author;
            this.LiveReloadScript = config.LiveReloadScript;
            this.DocumentsPath = config.DocumentsPath;
            this.FilesPath = config.FilesPath;
            this.LayoutsPath = config.LayoutsPath;
            this.OutputPath = config.OutputPath;
            this.Parent = parent;
            this.TimeZone = config.TimeZone;
            this.Url = config.Url.EnsureEndsWith("/");
            this.RootUrl = config.RootUrl.EnsureEndsWith("/");

            var relativeUrl = this.Url?.TrimStart('/');

            this.FullUrl = String.Concat(this.RootUrl, relativeUrl);

            var allDocuments = documents.ToList();

            this.Data = data.ToList();

            this.Partials = allDocuments.Where(d => d.Partial).ToList();

            this.Documents = allDocuments.Where(d => !d.Partial).ToList();

            this.Files = files.ToList();

            this.Layouts = new LayoutFileCollection(layouts);

            this.Metadata = config.Metadata;
        }

        public string SitePath { get; }

        public IDictionary<string, string> DefaultLayoutForExtension { get; }

        public IEnumerable<Regex> IgnoreFiles { get; }

        public Author Author { get; }

        public string LiveReloadScript { get; }

        public string DocumentsPath { get; }

        public string FilesPath { get; }

        public string LayoutsPath { get; }

        public string OutputPath { get; }

        public Site Parent { get; }

        public TimeZoneInfo TimeZone { get; }

        public string Url { get; }

        public string RootUrl { get; }

        public string FullUrl { get; }

        public IList<DocumentFile> Partials { get; }

        public IList<DataFile> Data { get; }

        public IList<DocumentFile> Documents { get; }

        public IList<StaticFile> Files { get; }

        public LayoutFileCollection Layouts { get; }

        public IEnumerable<Book> Books { get; set; }

        public MetadataCollection Metadata { get; }
    }
}
