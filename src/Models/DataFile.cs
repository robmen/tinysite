using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TinySite.Models
{
    [DebuggerDisplay("DataFile: {Id}, Source: {SourcePath}")]
    public class DataFile : SourceFile
    {
        public DataFile(string path, string rootPath, string sourceContent, MetadataCollection metadata, IDictionary<string, string> queries)
            : base(path, rootPath)
        {
            var relativePath = this.SourcePath.Substring(rootPath.Length);

            this.Id = Path.Combine(Path.GetDirectoryName(relativePath), Path.GetFileNameWithoutExtension(relativePath));

            this.SourceContent = sourceContent;

            this.Metadata = metadata;

            this.Queries = queries;
        }

        internal IDictionary<string, string> Queries { get; }

        public string Id { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public string Content { get; internal set; }

        public string SourceContent { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public MetadataCollection Metadata { get; }
    }
}
