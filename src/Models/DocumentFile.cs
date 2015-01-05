using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TinySite.Extensions;
using TinySite.Services;

namespace TinySite.Models
{
    public class DocumentFile : OutputFile
    {
        private static readonly Regex DateFromFileName = new Regex(@"^\s*(?<year>\d{4})-(?<month>\d{1,2})-(?<day>\d{1,2})([Tt@](?<hour>\d{1,2})\.(?<minute>\d{1,2})(\.(?<second>\d{1,2}))?)?[-\s]\s*", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        private object _renderLock = new object();

        private string _summary;

        private bool _explicitSummary;

        public DocumentFile(string path, string rootPath, string outputRootPath, string url, string rootUrl, Author author)
            : base(path, rootPath, outputRootPath, url, rootUrl)
        {
            this.Author = author;
        }

        private DocumentFile(DocumentFile original)
            : base(original)
        {
            this.Author = original.Author;
            this.Draft = original.Draft;
            this.Date = original.Date;
            this.Paginate = original.Paginate; // TODO: probably should not be shallow copying this, right?

            _summary = original._summary;
            _explicitSummary = original._explicitSummary;

            this.Metadata = new MetadataCollection(original.Metadata);
            this.ExtensionsForRendering = new List<string>(original.ExtensionsForRendering);
        }

        public Author Author { get; set; }

        public DateTime Date { get; set; }

        public bool Draft { get; set; }

        public int Paginate { get; set; }

        public MetadataCollection Metadata { get; set; }

        public string Summary
        {
            get
            {
                if (!_explicitSummary && !this.Rendered && !this.Rendering)
                {
                    this.RenderDocument();
                }

                return _summary;
            }

            set
            {
                _summary = value;
                _explicitSummary = !String.IsNullOrEmpty(_summary);
            }
        }

        public IList<string> ExtensionsForRendering { get; set; }

        public DocumentFile Clone()
        {
            var clone = new DocumentFile(this);
            return clone;
        }

        public dynamic GetAsDynamic(bool selfRender)
        {
            var now = DateTime.Now;

            dynamic data = new CaseInsenstiveExpando();

            this.Metadata.Assign(data as IDictionary<string, object>);

            data.Author = this.Author;
            data.Modified = this.Modified;
            data.OutputPath = this.OutputPath;
            data.RelativePath = this.RelativePath;
            data.SourcePath = this.SourcePath;
            data.Url = this.Url;
            data.RootUrl = this.RootUrl;
            data.FullUrl = this.RootUrl.EnsureEndsWith("/") + this.Url.TrimStart('/');
            data.Draft = this.Draft;
            data.Date = this.Date;
            data.DateUtc = this.Date.ToUniversalTime();
            data.FriendlyDate = this.Date.ToString("D");
            data.StandardUtcDate = this.Date.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
            data.Now = now;
            data.NowUtc = now.ToUniversalTime();
            data.NowFriendlyDate = now.ToString("D");
            data.NowStandardUtcDate = now.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
            data.Content = selfRender ? null : this.Content;
            data.Summary = selfRender ? null : this.Summary;

            return data;
        }

        public void RenderDocument()
        {
            this.RenderContent();
        }

        public void UpdateOutputPaths(string appendPath, string updateFileName)
        {
            if (String.IsNullOrEmpty(updateFileName))
            {
                updateFileName = Path.GetFileName(this.RelativePath);
            }

            var updateInUrl = String.IsNullOrEmpty(appendPath) ? String.Empty : appendPath.Replace('\\', '/') + '/';

            this.RelativePath = Path.Combine(Path.GetDirectoryName(this.RelativePath), appendPath, updateFileName);

            this.OutputPath = Path.Combine(Path.GetDirectoryName(this.OutputPath), appendPath, updateFileName);

            var lastSlash = this.Url.LastIndexOf('/');

            this.Url = String.Concat(this.Url.Substring(0, lastSlash + 1), updateInUrl, updateFileName.Equals("index.html", StringComparison.OrdinalIgnoreCase) ? String.Empty : updateFileName);
        }

        protected override void RenderContent()
        {
            if (RenderingTransaction.Current == null)
            {
                throw new InvalidOperationException("Rendering a document's content can only occur inside a rendering transaction. Create a rendering transaction and try the operation again.");
            }

            if (this.Rendered)
            {
                return;
            }

            lock (_renderLock)
            {
                if (this.Rendered)
                {
                    return;
                }

                base.StartRendering();

                var layoutName = this.Metadata.Get<string>("layout", "default");

                var layout = RenderingTransaction.Current.Layouts[layoutName];

                var paginator = this.Metadata.Get<Paginator>("paginator");

                foreach (var extension in this.ExtensionsForRendering)
                {
                    this.Content = this.RenderContentForExtension(true, extension, this.Content, layout, paginator);
                }

                if (!String.IsNullOrEmpty(this.Content) && !_explicitSummary)
                {
                    _summary = Summarize(this.Content);
                }

                this.RenderDocumentWithLayout(layout, paginator as Paginator);

                base.RenderContent();
            }
        }

        private static string Summarize(string content)
        {
            string summary = null;

            Match match = Regex.Match(content, "<p>.*?</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success && match.Value != content)
            {
                summary = match.Value;
            }

            return summary;
        }

        private void RenderDocumentWithLayout(LayoutFile layout, Paginator paginator)
        {
            var content = this.RenderContentForExtension(false, layout.Extension, layout.Content, layout, paginator);
            this.Content = content;

            string parentLayout;
            if (layout.Metadata.TryGet<string>("layout", out parentLayout))
            {
                layout = RenderingTransaction.Current.Layouts[parentLayout];
                this.RenderDocumentWithLayout(layout, paginator);
            }
        }

        private string RenderContentForExtension(bool renderingSelf, string extension, string template, LayoutFile layout, Paginator paginator)
        {
            var engine = RenderingTransaction.Current.Engines[extension];

            dynamic data = new CaseInsenstiveExpando();
            data.Site = RenderingTransaction.Current.Site.GetAsDynamic();
            data.Layout = layout.GetAsDynamic();
            data.Document = this.GetAsDynamic(renderingSelf);
            data.Paginator = paginator == null ? null : paginator.GetAsDynamic();

            return engine.Render(template, data);
        }
    }
}
