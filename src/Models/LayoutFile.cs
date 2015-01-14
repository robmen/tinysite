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
            var data = new CaseInsensitiveExpando();

            this.Metadata.Assign(data);

            data.Add("Id", this.Id);
            data.Add("Modified", this.Modified);
            data.Add("Path", this.SourcePath);
            data.Add("Name", Path.GetFileName(this.SourcePath));
            data.Add("SourceContent", this.SourceContent);

            return data;
        }
    }
}
