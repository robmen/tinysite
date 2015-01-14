using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    [DebuggerDisplay("LayoutFile: {Id}, Source: {SourcePath}")]
    public class LayoutFile
    {
        public LayoutFile(string path, string rootPath, string sourceContent, MetadataCollection metadata)
        {
            this.SourcePath = Path.GetFullPath(path);

            var relativePath = this.SourcePath.Substring(rootPath.Length);

            this.Id = Path.Combine(Path.GetDirectoryName(relativePath), Path.GetFileNameWithoutExtension(relativePath));

            this.Extension = Path.GetExtension(relativePath).TrimStart('.').ToLowerInvariant();

            var info = new FileInfo(this.SourcePath);

            this.Modified = info.LastWriteTime < info.CreationTime ? info.CreationTime : info.LastWriteTime;

            this.SourceContent = sourceContent;

            this.Metadata = metadata;
        }

        public string Id { get; private set; }

        public string Extension { get; private set; }

        public DateTime Modified { get; private set; }

        public string SourcePath { get; private set; }

        public string SourceContent { get; private set; }

        public MetadataCollection Metadata { get; private set; }

        public dynamic GetAsDynamic()
        {
            dynamic data = new CaseInsenstiveExpando();

            this.Metadata.Assign(data as IDictionary<string, object>);

            data.Id = this.Id;
            data.Modified = this.Modified;
            data.Path = this.SourcePath;
            data.Name = Path.GetFileName(this.SourcePath);
            data.SourceContent = this.SourceContent;

            return data;
        }
    }
}
