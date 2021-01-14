﻿using System;
using System.Collections.Generic;
using System.Linq;
using TinySite.Services;

namespace TinySite.Models.Dynamic
{
    public class DynamicLayoutFile : DynamicFileBase
    {
        public DynamicLayoutFile(DocumentFile activeDocument, LayoutFile layout, Site site)
            : base(layout.SourceRelativePath)
        {
            this.ActiveDocument = activeDocument;
            this.Layout = layout;
            this.Site = site;
        }

        private DocumentFile ActiveDocument { get; }

        private LayoutFile Layout { get; }

        public Site Site { get; }

        protected override IDictionary<string, object> GetData()
        {
            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(this.Layout.Id), this.Layout.Id },
                { nameof(this.Layout.Layout), this.Layout.Layout },
                { nameof(this.Layout.SourceContent), this.Layout.SourceContent }
            };

            this.Layout.Metadata?.AssignTo(this.Layout.SourceRelativePath, data);

            if (this.Layout.Queries != null)
            {
                foreach (var query in this.Layout.Queries)
                {
                    data.Add(query.Key, new Lazy<object>(() => this.ExecuteQuery(query.Value)));
                }
            }

            return data;
        }

        private object ExecuteQuery(string queryString)
        {
            var query = QueryProcessor.Parse(this.Site, queryString);

            var results = query.Results.ToList();
            foreach (var contributor in results.OfType<DynamicSourceFile>())
            {
                this.ActiveDocument.AddContributingFile(contributor.GetSourceFile());
            }

            return results;
        }
    }
}
