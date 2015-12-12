using System;
using System.Collections.Generic;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    public abstract class OutputFile : SourceFile
    {
        public OutputFile()
        {
        }

        public OutputFile(string path, string rootPath, string outputPath, string outputRootPath, string rootUrl, string relativeUrl)
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

        protected OutputFile(OutputFile original) :
            base(original)
        {
        }
        
        public string OutputPath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string OutputRootPath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string OutputRelativePath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string Url { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string RootUrl { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string RelativeUrl { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string TargetExtension { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        internal bool Unmodified { get; set; }
    }
}
