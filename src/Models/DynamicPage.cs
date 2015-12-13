using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class DynamicPage : DynamicBase
    {
        public DynamicPage(DocumentFile activeDocument, Page Page)
            : base(null)
        {
            this.ActiveDocument = activeDocument;
            this.Page = Page;
        }

        private DocumentFile ActiveDocument { get; }

        private Page Page { get; }

        protected override IDictionary<string, object> GetData()
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(this.Page.Active), this.Page.Active },
                { nameof(this.Page.Number), this.Page.Number },
                { nameof(this.Page.Url), this.Page.Url },
            };
        }
    }
}