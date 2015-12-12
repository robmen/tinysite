using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class DynamicRenderingSite : DynamicRenderingObject
    {
        public DynamicRenderingSite(DocumentFile activeDocument, Site site)
            : base(site.SitePath)
        {
            this.ActiveDocument = activeDocument;
            this.Site = site;
        }

        private DocumentFile ActiveDocument { get; }

        private Site Site { get; }

        protected override IDictionary<string, object> GetData()
        {
            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(this.Site.Author), this.Site.Author },
                { nameof(this.Site.LiveReloadScript), this.Site.LiveReloadScript },
                { nameof(this.Site.DocumentsPath), this.Site.DocumentsPath },
                { nameof(this.Site.FilesPath), this.Site.FilesPath },
                { nameof(this.Site.LayoutsPath), this.Site.LayoutsPath },
                { nameof(this.Site.OutputPath), this.Site.OutputPath },
                { nameof(this.Site.Parent), new Lazy<object>(GetParentSite) },
                { nameof(this.Site.TimeZone), this.Site.TimeZone },
                { nameof(this.Site.Url), this.Site.Url },
                { nameof(this.Site.RootUrl), this.Site.RootUrl },
                { nameof(this.Site.FullUrl), this.Site.FullUrl },
                { nameof(this.Site.Books), new Lazy<object>(GetBooks) },
                { nameof(this.Site.Documents), new Lazy<object>(GetDocuments) },
                { nameof(this.Site.Files), new Lazy<object>(GetFiles) },
                { nameof(this.Site.Layouts), new Lazy<object>(GetLayouts) },
                { nameof(this.Site.Partials), new Lazy<object>(GetPartials) },
            };

            this.Site.Metadata?.AssignTo(this.Site.SitePath, data);

            return data;
        }

        private object GetParentSite()
        {
            return (this.Site.Parent == null) ? null : new DynamicRenderingSite(this.ActiveDocument, this.Site.Parent);
        }

        private object GetBooks()
        {
            var books = new List<DynamicRenderingBook>();

            foreach (var book in this.Site.Books)
            {
                books.Add(new DynamicRenderingBook(this.ActiveDocument, book));
            }

            return books;
        }

        private object GetDocuments()
        {
            var documents = new List<DynamicRenderingDocument>(this.Site.Documents.Count);

            foreach (var document in this.Site.Documents)
            {
                this.ActiveDocument.AddContributingFile(document);
                documents.Add(new DynamicRenderingDocument(this.ActiveDocument, document));
            }

            return documents;
        }

        private object GetFiles()
        {
            var files = new List<DynamicRenderingStaticFile>(this.Site.Files.Count);

            foreach (var file in this.Site.Files)
            {
                this.ActiveDocument.AddContributingFile(file);
                files.Add(new DynamicRenderingStaticFile(this.ActiveDocument, file));
            }

            return files;
        }

        private object GetLayouts()
        {
            var layouts = new List<DynamicRenderingLayout>(this.Site.Layouts.Count);

            foreach (var layout in this.Site.Layouts)
            {
                this.ActiveDocument.AddContributingFile(layout);
                layouts.Add(new DynamicRenderingLayout(this.ActiveDocument, layout));
            }

            return layouts;
        }

        private object GetPartials()
        {
            return new PartialsContent(this.Site.Partials, this.ActiveDocument);
        }
    }
}
