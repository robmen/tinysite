using System;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    public abstract class OutputFile
    {
        private string _content;

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
            this.RelativePath = original.RelativePath;
            this.RelativeSourcePath = original.RelativeSourcePath;
            this.SourcePath = original.SourcePath;
            this.Url = original.Url;
            this.RootUrl = original.RootUrl;

            this._content = original._content;
            this.Rendered = original.Rendered;
            this.Rendering = original.Rendering;
        }

        public DateTime Modified { get; set; }

        public string OutputPath { get; set; }

        public string OutputRootPath { get; set; }

        public string RelativePath { get; set; }

        public string RelativeSourcePath { get; set; }

        public string SourcePath { get; set; }

        public string Url { get; set; }

        public string RootUrl { get; set; }

        public string Content
        {
            get
            {
                if (!this.Rendered && !this.Rendering)
                {
                    this.RenderContent();
                }

                return _content;
            }

            set
            {
                this.Rendered = false;
                _content = value;
            }
        }

        public bool Rendered { get; private set; }

        public bool Rendering { get; private set; }

        protected virtual void RenderContent()
        {
            this.Rendered = true;
            this.Rendering = false;
        }

        protected void StartRendering()
        {
            this.Rendering = true;
        }
    }
}
