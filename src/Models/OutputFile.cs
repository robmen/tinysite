using System;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    public abstract class OutputFile
    {
        public OutputFile(string path, string rootPath, string outputRootPath, string relativeUrl, string rootUrl)
        {
            var actualRootPath = Path.GetDirectoryName(rootPath.TrimEnd('\\'));

            var info = new FileInfo(path);

            this.Modified = info.LastWriteTime;

            this.SourcePath = Path.GetFullPath(path);

            this.RelativePath = this.SourcePath.Substring(rootPath.Length);

            this.RelativeSourcePath = this.SourcePath.Substring(actualRootPath.Length + 1);

            this.OutputRootPath = outputRootPath;

            this.OutputPath = Path.Combine(this.OutputRootPath, this.RelativePath);

            this.Url = relativeUrl.EnsureEndsWith("/") + this.RelativePath.Replace('\\', '/');

            this.RootUrl = rootUrl;
        }

        protected OutputFile(OutputFile original)
        {
            this.Modified = original.Modified;
            this.OutputPath = original.OutputPath;
            this.OutputRootPath = original.OutputRootPath;
            this.RelativePath = original.RelativePath;
            this.RelativeSourcePath = original.RelativeSourcePath;
            this.SourcePath = original.SourcePath;
            this.Url = original.Url;
            this.RootUrl = original.RootUrl;
        }

        public DateTime Modified { get; set; }

        public string OutputPath { get; set; }

        public string OutputRootPath { get; set; }

        public string RelativePath { get; set; }

        public string RelativeSourcePath { get; set; }

        public string SourcePath { get; set; }

        public string Url { get; set; }

        public string RootUrl { get; set; }
    }
}
