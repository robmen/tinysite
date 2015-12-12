using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class DynamicRenderingData : DynamicRenderingObject
    {
        // These are fields because as properties their typical name (e.g. "Document" 
        // for "_document") would hide the dynamic properties we are trying to expose
        // via the dictionary created by GetData().
        //
        private DocumentFile _document;
        private LayoutFile _layout;
        private Site _site;

        public DynamicRenderingData(DocumentFile document, LayoutFile layout, Site site)
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
                { "Document", new Lazy<object>(GetDocument) },
                { "Layout", new Lazy<object>(GetLayout) },
                { "Site", new Lazy<object>(GetSite) },
                { "Books", new Lazy<object>(GetBooks) },
                { "PartialsContent", new Lazy<object>(GetPartialsContent) },
            };
        }

        private object GetDocument()
        {
            return new DynamicRenderingDocument(this._document, this._document);
        }

        private object GetLayout()
        {
            return new DynamicRenderingLayout(this._document, this._layout);
        }

        private object GetSite()
        {
            return new DynamicRenderingSite(this._document, this._site);
        }

        private object GetBooks()
        {
            var books = new List<DynamicRenderingBook>();

            foreach (var book in this._site.Books)
            {
                books.Add(new DynamicRenderingBook(this._document, book));
            }

            return books;
        }

        private object GetPartialsContent()
        {
            return new PartialsContent(this._site.Partials, this._document);
        }
    }
}
