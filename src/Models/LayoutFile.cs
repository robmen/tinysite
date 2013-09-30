using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using TinySite.Commands;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class LayoutFile
    {
        public LayoutFile(string path, string rootPath)
        {
            this.SourcePath = Path.GetFullPath(path);

            var relativePath = this.SourcePath.Substring(rootPath.Length);

            this.Id = Path.Combine(Path.GetDirectoryName(relativePath), Path.GetFileNameWithoutExtension(relativePath));

            this.Extension = Path.GetExtension(relativePath).TrimStart('.').ToLowerInvariant();

            var info = new FileInfo(this.SourcePath);

            this.Modified = info.LastWriteTime < info.CreationTime ? info.CreationTime : info.LastWriteTime;
        }

        public string Id { get; private set; }

        public string Extension { get; private set; }

        public DateTime Modified { get; private set; }

        public string SourcePath { get; private set; }

        public string Content { get; private set; }

        public MetadataCollection Metadata { get; private set; }

        public void Load()
        {
            var parser = new ParseDocumentCommand();
            parser.DocumentPath = this.SourcePath;
            parser.Execute();

            this.Content = parser.Content;

            this.Metadata = parser.Metadata;
        }

        public dynamic GetAsDynamic()
        {
            dynamic data = new CaseInsenstiveExpando();

            this.Metadata.Assign(data as IDictionary<string, object>);

            data.Id = this.Id;
            data.Modified = this.Modified;
            data.Path = this.SourcePath;
            data.Name = Path.GetFileName(this.SourcePath);
            data.Content = this.Content;

            return data;
        }
    }
}
