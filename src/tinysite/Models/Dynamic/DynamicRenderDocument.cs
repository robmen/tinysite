using System;
using System.Collections.Generic;

namespace TinySite.Models.Dynamic
{
    public class DynamicRenderDocument : DynamicFileBase
    {
        // These are fields because as properties their typical name (e.g. "Document" 
        // for "_document") would hide the dynamic properties we are trying to expose
        // via the dictionary created by GetData().
        //
        private readonly DocumentFile _document;
        private readonly LayoutFile _layout;
        private readonly Site _site;

        public DynamicRenderDocument(DocumentFile document, LayoutFile layout, Site site)
            : base(document.SourceRelativePath)
        {
            _document = document;
            _layout = layout;
            _site = site;
        }

        protected override IDictionary<string, object> GetData()
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "Document", new Lazy<object>(this.GetDocument) },
                { "Layout", new Lazy<object>(this.GetLayout) },
                { "Site", new Lazy<object>(this.GetSite) },
                { "Books", new Lazy<object>(this.GetBooks) },
                { "PartialsContent", new Lazy<object>(this.GetPartialsContent) },
            };
        }

        private object GetDocument()
        {
            return new DynamicDocumentFile(_document, _document, _site);
        }

        private object GetLayout()
        {
            return new DynamicLayoutFile(_document, _layout, _site);
        }

        private object GetSite()
        {
            return new DynamicSite(_document, _site);
        }

        private object GetBooks()
        {
            var books = new List<DynamicBook>();

            foreach (var book in _site.Books)
            {
                books.Add(new DynamicBook(_document, book, _site));
            }

            return books;
        }

        private object GetPartialsContent()
        {
            return new PartialsContent(_site.Partials, _document);
        }
    }
}
