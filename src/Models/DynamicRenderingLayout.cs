using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class DynamicRenderingLayout : DynamicRenderingObject
    {
        public DynamicRenderingLayout(DocumentFile activeDocument, LayoutFile layout)
            : base(layout.SourceRelativePath)
        {
            this.ActiveDocument = activeDocument;
            this.Layout = layout;
        }

        private DocumentFile ActiveDocument { get; }

        private LayoutFile Layout { get; }

        protected override IDictionary<string, object> GetData()
        {
            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(this.Layout.Id), this.Layout.Id },
                { nameof(this.Layout.Layout), this.Layout.Layout },
                { nameof(this.Layout.SourceContent), this.Layout.SourceContent }
            };

            this.Layout.Metadata?.AssignTo(this.Layout.SourceRelativePath, data);

            return data;
        }
    }
}