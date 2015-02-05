using System;
using System.Diagnostics;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    [DebuggerDisplay("LayoutFile: {Id}, Source: {SourcePath}")]
    public class LayoutFile : CaseInsensitiveExpando
    {
        public LayoutFile(string path, string rootPath, string sourceContent, MetadataCollection metadata)
        {
            metadata.Assign(this);

            this.SourcePath = Path.GetFullPath(path);

            this.Name = Path.GetFileName(path);

            var relativePath = this.SourcePath.Substring(rootPath.Length);

            this.Id = Path.Combine(Path.GetDirectoryName(relativePath), Path.GetFileNameWithoutExtension(relativePath));

            this.Extension = Path.GetExtension(relativePath).TrimStart('.').ToLowerInvariant();

            var info = new FileInfo(this.SourcePath);

            this.Modified = info.LastWriteTime < info.CreationTime ? info.CreationTime : info.LastWriteTime;

            this.SourceContent = sourceContent;
        }

        public string Id { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public string Extension { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public DateTime Modified { get { return this.Get<DateTime>(); } private set { this.Set<DateTime>(value); } }

        public string Name { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public string SourcePath { get { return this.Get<string>(); } private set { this.Set<string>(value); } }

        public string SourceContent { get { return this.Get<string>(); } private set { this.Set<string>(value); } }
    }
}
