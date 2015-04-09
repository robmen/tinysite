using System;
using System.Collections.Generic;
using System.Linq;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class Site : CaseInsensitiveExpando
    {
        public Site(SiteConfig config, IEnumerable<DocumentFile> documents, IEnumerable<StaticFile> files, IEnumerable<LayoutFile> layouts, Site parent = null)
        {
            config.Metadata.Assign(this);

            this.DefaultLayoutForExtension = new Dictionary<string, string>(config.DefaultLayoutForExtension);

            this.Author = config.Author;
            this.DocumentsPath = config.DocumentsPath;
            this.FilesPath = config.FilesPath;
            this.LayoutsPath = config.LayoutsPath;
            this.OutputPath = config.OutputPath;
            this.Parent = parent;
            this.TimeZone = config.TimeZone;
            this.Url = config.Url.EnsureEndsWith("/");
            this.RootUrl = config.RootUrl.EnsureEndsWith("/");

            var allDocuments = documents.ToList();

            this.Partials = allDocuments.Where(d => d.Partial).ToList();

            this.Documents = allDocuments.Where(d => !d.Partial).ToList();

            this.Files = files.ToList();

            this.Layouts = new LayoutFileCollection(layouts);
        }

        public IDictionary<string, string> DefaultLayoutForExtension { get; private set; }

        public Author Author { get { return this.Get<Author>(); } set { this.Set<Author>(value); } }

        public string DocumentsPath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string FilesPath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string LayoutsPath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string OutputPath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public Site Parent { get { return this.Get<Site>(); } set { this.Set<Site>(value); } }

        public TimeZoneInfo TimeZone { get { return this.Get<TimeZoneInfo>(); } set { this.Set<TimeZoneInfo>(value); } }

        public string Url { get { return this.Get<string>(); } set { this.Set<string>(value); this.UpdateFullUrl(); } }

        public string RootUrl { get { return this.Get<string>(); } set { this.Set<string>(value); this.UpdateFullUrl(); } }

        public string FullUrl { get { return this.Get<string>(); } }

        public IList<DocumentFile> Partials { get { return this.Get<IList<DocumentFile>>(); } set { this.Set<IList<DocumentFile>>(value); } }

        public IList<DocumentFile> Documents { get { return this.Get<IList<DocumentFile>>(); } set { this.Set<IList<DocumentFile>>(value); } }

        public IList<StaticFile> Files { get { return this.Get<IList<StaticFile>>(); } set { this.Set<IList<StaticFile>>(value); } }

        public LayoutFileCollection Layouts { get { return this.Get<LayoutFileCollection>(); } set { this.Set<LayoutFileCollection>(value); } }

        public IEnumerable<Book> Books { get { return this.Get<IEnumerable<Book>>(); } set { this.Set<IEnumerable<Book>>(value); } }

        private void UpdateFullUrl()
        {
            var rootUrl = (this.RootUrl == null) ? null : this.RootUrl.EnsureEndsWith("/");

            var relativeUrl = (this.Url == null) ? null : this.Url.TrimStart('/');

            var fullUrl = String.Concat(rootUrl, relativeUrl);

            this.Set<string>(fullUrl, "FullUrl");
        }
    }
}
