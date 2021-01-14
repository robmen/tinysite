using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TinySite.Models
{
    [DebuggerDisplay("LayoutFile: {Id}, Source: {SourcePath}")]
    public class LayoutFile : SourceFile
    {
        public LayoutFile(string path, string rootPath, string sourceContent, MetadataCollection metadata, IDictionary<string, string> queries)
            : base(path, rootPath)
        {
            var relativePath = this.SourcePath.Substring(rootPath.Length);

            this.Id = Path.Combine(Path.GetDirectoryName(relativePath), Path.GetFileNameWithoutExtension(relativePath));

            this.SourceContent = sourceContent;

            this.Metadata = metadata;

            string layout;
            if (this.Metadata != null && this.Metadata.TryGet("layout", out layout))
            {
                this.Layout = layout;

                this.Metadata.Remove("layout");
            }

            this.Queries = queries;
        }

        internal IDictionary<string, string> Queries { get; }

        public string Id { get; }

        public string SourceContent { get; }

        public string Layout { get; }

        public MetadataCollection Metadata { get; }
    }
}
