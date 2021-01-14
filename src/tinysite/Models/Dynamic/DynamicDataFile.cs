using System;
using System.Collections.Generic;
using System.Linq;
using TinySite.Services;

namespace TinySite.Models.Dynamic
{
    public class DynamicDataFile : DynamicSourceFile
    {
        public DynamicDataFile(DocumentFile activeDocument, DataFile dataFile, Site site)
            : base(dataFile, dataFile.Metadata)
        {
            this.ActiveDocument = activeDocument;
            this.DataFile = dataFile;
            this.Site = site;
        }

        private DocumentFile ActiveDocument { get; }

        private DataFile DataFile { get; }

        public Site Site { get; }

        protected override IDictionary<string, object> GetData()
        {
            var data = base.GetData();

            data.Add(nameof(this.DataFile.Id), this.DataFile.Id);
            data.Add(nameof(this.DataFile.Content), this.DataFile.Content);
            data.Add(nameof(this.DataFile.SourceContent), this.DataFile.SourceContent);

            this.DataFile.Metadata?.AssignTo(this.DataFile.SourceRelativePath, data);

            if (this.DataFile.Queries != null)
            {
                foreach (var query in this.DataFile.Queries)
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
