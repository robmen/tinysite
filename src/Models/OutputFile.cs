using System;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    public abstract class OutputFile
    {
        public OutputFile(string path, string rootPath, string outputPath, string outputRootPath, string rootUrl, string relativeUrl)
        {
            var actualRootPath = Path.GetDirectoryName(rootPath.TrimEnd('\\'));

            var info = new FileInfo(path);

            this.Date = info.CreationTime;

            this.Modified = info.LastWriteTime;

            this.SourcePath = Path.GetFullPath(path);

            this.SourceRelativePath = this.SourcePath.Substring(actualRootPath.Length + 1);

            this.OutputRelativePath = outputPath ?? this.SourcePath.Substring(rootPath.Length);

            this.OutputRootPath = outputRootPath;

            this.OutputPath = Path.Combine(this.OutputRootPath, this.OutputRelativePath);

            this.RelativeUrl = relativeUrl;

            this.RootUrl = rootUrl;

            this.Url = this.RootUrl.EnsureEndsWith("/") + this.RelativeUrl.TrimStart('/');
        }

        protected OutputFile(OutputFile original)
        {
            this.Date = original.Date;
            this.Modified = original.Modified;
            this.OutputPath = original.OutputPath;
            this.OutputRootPath = original.OutputRootPath;
            this.OutputRelativePath = original.OutputRelativePath;
            this.SourcePath = original.SourcePath;
            this.SourceRelativePath = original.SourceRelativePath;
            this.Url = original.Url;
            this.RootUrl = original.RootUrl;
            this.RelativeUrl = original.RelativeUrl;
        }

        public DateTime Date { get; set; }

        public DateTime Modified { get; set; }

        public string OutputPath { get; set; }

        public string OutputRootPath { get; set; }

        public string OutputRelativePath { get; set; }

        public string SourcePath { get; set; }

        public string SourceRelativePath { get; set; }

        public string Url { get; set; }

        public string RootUrl { get; set; }

        public string RelativeUrl { get; set; }
    }
}
