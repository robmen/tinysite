﻿using System;
using System.Collections.Generic;

namespace TinySite.Models.Dynamic
{
    public class DynamicSite : DynamicFileBase
    {
        public DynamicSite(DocumentFile activeDocument, Site site)
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
                { nameof(this.Site.Data), new Lazy<object>(GetDataFiles) },
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
            return (this.Site.Parent == null) ? null : new DynamicSite(this.ActiveDocument, this.Site.Parent);
        }

        private object GetBooks()
        {
            var books = new List<DynamicBook>();

            foreach (var book in this.Site.Books)
            {
                books.Add(new DynamicBook(this.ActiveDocument, book, this.Site));
            }

            return books;
        }

        private object GetDataFiles()
        {
            var dataFiles = new List<DynamicDataFile>(this.Site.Data.Count);

            foreach (var data in this.Site.Data)
            {
                this.ActiveDocument?.AddContributingFile(data);
                dataFiles.Add(new DynamicDataFile(this.ActiveDocument, data, this.Site));
            }

            return dataFiles;
        }

        private object GetDocuments()
        {
            var documents = new List<DynamicDocumentFile>(this.Site.Documents.Count);

            foreach (var document in this.Site.Documents)
            {
                this.ActiveDocument?.AddContributingFile(document);
                documents.Add(new DynamicDocumentFile(this.ActiveDocument, document, this.Site));
            }

            return documents;
        }

        private object GetFiles()
        {
            var files = new List<DynamicStaticFile>(this.Site.Files.Count);

            foreach (var file in this.Site.Files)
            {
                this.ActiveDocument?.AddContributingFile(file);
                files.Add(new DynamicStaticFile(this.ActiveDocument, file));
            }

            return files;
        }

        private object GetLayouts()
        {
            var layouts = new List<DynamicLayoutFile>(this.Site.Layouts.Count);

            foreach (var layout in this.Site.Layouts)
            {
                this.ActiveDocument?.AddContributingFile(layout);
                layouts.Add(new DynamicLayoutFile(this.ActiveDocument, layout, this.Site));
            }

            return layouts;
        }

        private object GetPartials()
        {
            return new PartialsContent(this.Site.Partials, this.ActiveDocument);
        }
    }
}
