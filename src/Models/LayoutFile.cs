using System.Diagnostics;
using System.IO;

namespace TinySite.Models
{
    [DebuggerDisplay("LayoutFile: {Id}, Source: {SourcePath}")]
    public class LayoutFile : SourceFile
    {
        public LayoutFile(string path, string rootPath, string sourceContent, MetadataCollection metadata)
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
        }

        public string Id { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public string SourceContent { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public string Layout { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public MetadataCollection Metadata { get; }
    }
}
