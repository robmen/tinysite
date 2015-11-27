using System;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    public abstract class OutputFile : CaseInsensitiveExpando
    {
        public OutputFile()
        {
        }

        public OutputFile(string path, string rootPath, string outputPath, string outputRootPath, string rootUrl, string relativeUrl)
        {
            var actualRootPath = Path.GetDirectoryName(rootPath.TrimEnd('\\'));

            var info = new FileInfo(path);

            this.Date = info.CreationTime;

            this.Modified = info.LastWriteTime;

            this.Name = Path.GetFileName(path);

            this.Extension = Path.GetExtension(path);

            this.SourcePath = Path.GetFullPath(path);

            this.SourceRelativePath = this.SourcePath.Substring(actualRootPath.Length + 1);

            this.OutputRelativePath = outputPath ?? this.SourcePath.Substring(rootPath.Length);

            this.OutputRootPath = outputRootPath;

            this.OutputPath = Path.Combine(this.OutputRootPath, this.OutputRelativePath);

            this.TargetExtension = Path.GetExtension(this.OutputRelativePath).TrimStart('.');

            this.RelativeUrl = relativeUrl;

            this.RootUrl = rootUrl;

            this.Url = this.RootUrl.EnsureEndsWith("/") + this.RelativeUrl.TrimStart('/');
        }

        protected OutputFile(OutputFile original) :
            base(original)
        {
        }

        public DateTime Date { get { return this.Get<DateTime>(); } set { this.SetTimes(null, value); } }

        public DateTime Modified { get { return this.Get<DateTime>(); } set { this.Set<DateTime>(value); } }

        public string Name { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string Extension { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string OutputPath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string OutputRootPath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string OutputRelativePath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string SourcePath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string SourceRelativePath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string Url { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string RootUrl { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string RelativeUrl { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string TargetExtension { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        protected void SetTimes(string prefix, DateTime time)
        {
            this.Set<DateTime>(time, prefix ?? "Date");
            this.Set<DateTime>(time.ToUniversalTime(), prefix ?? "Date" + "Utc");
            this.Set<string>(time.ToString("D"), prefix + "FriendlyDate");
            this.Set<string>(time.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ"), prefix + "StandardUtcDate");
        }
    }
}
