using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    public abstract class OutputFile : SourceFile
    {
        protected OutputFile(string path, string rootPath, string outputPath, string outputRootPath, string rootUrl, string relativeUrl)
                : base(path, rootPath)
        {
            this.OutputRelativePath = outputPath ?? this.SourcePath.Substring(rootPath.Length);

            this.OutputRootPath = outputRootPath;

            this.OutputPath = Path.Combine(this.OutputRootPath, this.OutputRelativePath);

            this.TargetExtension = Path.GetExtension(this.OutputRelativePath).TrimStart('.');

            this.RelativeUrl = relativeUrl;

            this.RootUrl = rootUrl;

            this.Url = this.RootUrl.EnsureEndsWith("/") + this.RelativeUrl.TrimStart('/');
        }

        protected OutputFile(OutputFile original)
            : base(original)
        {
            this.OutputPath = original.OutputPath;
            this.OutputRootPath = original.OutputRootPath;
            this.OutputRelativePath = original.OutputRelativePath;
            this.Url = original.Url;
            this.RootUrl = original.RootUrl;
            this.RelativeUrl = original.RelativeUrl;
            this.TargetExtension = original.TargetExtension;
            this.Unmodified = original.Unmodified;
        }

        public string OutputPath { get; set; }

        public string OutputRootPath { get; }

        public string OutputRelativePath { get; set; }

        public string Url { get; }

        public string RootUrl { get; }

        public string RelativeUrl { get; set; }

        public string TargetExtension { get; }

        internal bool Unmodified { get; set; }
    }
}
