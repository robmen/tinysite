using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TinySite.Commands;
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

        public void Load(LoadDocumentFlags flags, IEnumerable<string> knownExtensions)
        {
            // Parse the document and update our document metadata.
            //
            var parser = new ParseDocumentCommand();
            parser.DocumentPath = this.SourcePath;
            parser.SummaryMarker = "\r\n\r\n===";
            parser.Execute();

            this.Content = parser.Content;

            if (parser.Date.HasValue)
            {
                this.Date = parser.Date.Value;
            }

            this.Draft = (parser.Draft || this.Date > DateTime.Now);

            this.Paginate = parser.Metadata.Get<int>("paginate", 0);
            parser.Metadata.Remove("paginate");

            string output;
            if (parser.Metadata.TryGet<string>("output", out output))
            {
                this.SetOutputPaths(output);
                parser.Metadata.Remove("output");
            }

            this.Metadata = parser.Metadata;

            // The rest of this function is about calculating the correct
            // name for the file.
            //
            var fileName = Path.GetFileName(this.RelativePath);

            string updateFileName = null;

            string updateInPath = String.Empty;

            // See if this file should be processed by any of the
            // rendering engines.
            //
            this.ExtensionsForRendering = new List<string>();

            for (; ; )
            {
                var extension = Path.GetExtension(fileName).TrimStart('.');
                if (knownExtensions.Contains(extension))
                {
                    this.ExtensionsForRendering.Add(extension);
                    fileName = Path.GetFileNameWithoutExtension(fileName);

                    updateFileName = fileName;
                }
                else
                {
                    break;
                }
            }

            if (LoadDocumentFlags.DateFromFileName == (flags & LoadDocumentFlags.DateFromFileName))
            {
                var match = DateFromFileName.Match(fileName);

                if (match.Success)
                {
                    var year = Convert.ToInt32(match.Groups[1].Value, 10);
                    var month = Convert.ToInt32(match.Groups[2].Value, 10);
                    var day = Convert.ToInt32(match.Groups[3].Value, 10);
                    var hour = match.Groups[4].Success ? Convert.ToInt32(match.Groups[4].Value, 10) : 0;
                    var minute = match.Groups[5].Success ? Convert.ToInt32(match.Groups[5].Value, 10) : 0;
                    var second = match.Groups[6].Success ? Convert.ToInt32(match.Groups[6].Value, 10) : 0;

                    // If the parser didn't override the date, use the date from the filename.
                    //
                    if (!parser.Date.HasValue)
                    {
                        this.Date = new DateTime(year, month, day, hour, minute, second);
                    }

                    fileName = fileName.Substring(match.Length);

                    updateFileName = fileName;
                }
            }

            if (LoadDocumentFlags.DateInPath == (flags & LoadDocumentFlags.DateInPath) && this.Date != DateTime.MinValue)
            {
                updateInPath = String.Join("\\", this.Date.Year, this.Date.Month, this.Date.Day);
            }

            if (!this.Metadata.Contains("title"))
            {
                this.Metadata.Add("title", Path.GetFileNameWithoutExtension(fileName));
            }

            // Sanitize the filename into a good URL.
            //
            var sanitized = SanitizeEntryId(fileName);

            if (!fileName.Equals(sanitized))
            {
                fileName = sanitized;

                updateFileName = fileName;
            }

            if (LoadDocumentFlags.CleanUrls == (flags & LoadDocumentFlags.CleanUrls) && !"index.html".Equals(fileName, StringComparison.OrdinalIgnoreCase) && ".html".Equals(Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase))
            {
                updateInPath = Path.Combine(updateInPath, Path.GetFileNameWithoutExtension(fileName));

                fileName = "index.html";

                updateFileName = fileName;
            }

            // If the name or path was updated, update the appropriately parts of the document.
            //
            if (!String.IsNullOrEmpty(updateFileName) || !String.IsNullOrEmpty(updateInPath))
            {
                if (String.IsNullOrEmpty(updateFileName))
                {
                    updateFileName = fileName;
                }

                var updateInUrl = String.IsNullOrEmpty(updateInPath) ? String.Empty : updateInPath.Replace('\\', '/') + '/';

                this.RelativePath = Path.Combine(Path.GetDirectoryName(this.RelativePath), updateInPath, updateFileName);

                this.OutputPath = Path.Combine(Path.GetDirectoryName(this.OutputPath), updateInPath, updateFileName);

                var lastSlash = this.Url.LastIndexOf('/');

                this.Url = String.Concat(this.Url.Substring(0, lastSlash + 1), updateInUrl, updateFileName.Equals("index.html", StringComparison.OrdinalIgnoreCase) ? String.Empty : updateFileName);
            }
        }

        public void RenderDocument()
        {
            this.RenderContent();
        }

        private void SetOutputPaths(string output)
        {
            var path = output.Replace('/', '\\');

            var url = output.Replace('\\', '/');

            this.RelativePath = path;

            this.OutputPath = Path.Combine(this.OutputRootPath, path);

            this.Url = String.Concat(this.RootUrl, url);
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
                throw new InvalidOperationException("Rendering a document's content can only occur inside a rendering transaction. Create a rendering transaction and the operation again.");
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

                var layoutName = this.Metadata.Get<string>("layout", "post");

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

        private static string SanitizeEntryId(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            id = Regex.Replace(id, @"[^\w\s_\-\.]+", String.Empty); // first, allow only words, spaces, underscores, dashes and dots.
            id = Regex.Replace(id, @"\.{2,}", String.Empty); // strip out any dots stuck together (no pathing attempts).
            id = Regex.Replace(id, @"\s{2,}", " "); // convert multiple spaces into single space.
            id = id.Trim(new char[] { ' ', '.' }); // ensure the string does not start or end with a dot
            return id.Replace(' ', '-').ToLowerInvariant(); // finally, replace all spaces with dashes and lowercase it.
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
            var data = new { site = RenderingTransaction.Current.Site.GetAsDynamic(), layout = layout.GetAsDynamic(), document = this.GetAsDynamic(renderingSelf), paginator };

            return engine.Render(template, data);
        }
    }
}
