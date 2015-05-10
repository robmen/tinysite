using System;
using System.Linq;
using System.Text.RegularExpressions;
using TinySite.Extensions;
using TinySite.Models;

namespace TinySite.Services
{
    public class ContentRendering
    {
        public ContentRendering(RenderingTransaction transaction)
        {
            this.Transaction = transaction;
        }

        private RenderingTransaction Transaction { get; set; }

        public LayoutFile GetLayoutForDocument(DocumentFile document)
        {
            var layoutName = document.Layout;

            if (String.IsNullOrEmpty(layoutName))
            {
                if (!this.Transaction.Site.DefaultLayoutForExtension.TryGetValue(document.TargetExtension, out layoutName))
                {
                    this.Transaction.Site.DefaultLayoutForExtension.TryGetValue("*", out layoutName);
                }

                document.Layout = layoutName;
            }

            if (!String.IsNullOrEmpty(layoutName) && !this.Transaction.Site.Layouts.Contains(layoutName))
            {
                Console.Error.WriteLine("Cannot find layout: '{0}' while processing file: {1}", layoutName, document.SourcePath);

                layoutName = null;
            }

            return String.IsNullOrEmpty(layoutName) ? null : this.Transaction.Site.Layouts[layoutName];
        }

        public DocumentFile RenderDocumentContent(DocumentFile document)
        {
            var content = document.SourceContent;

            var layout = this.GetLayoutForDocument(document);

            foreach (var extension in document.ExtensionsForRendering)
            {
                content = this.RenderContentForExtension(document.SourcePath, content, extension, document, content, layout);
            }

            document.Content = content;

            if (String.IsNullOrEmpty(document.Summary) && !String.IsNullOrEmpty(document.Content))
            {
                document.Summary = Summarize(document.Content);
            }

            return document;
        }

        public string RenderDocumentContentUsingLayout(DocumentFile document, string documentContent, LayoutFile layout)
        {
            var content = this.RenderContentForExtension(layout.SourcePath, layout.SourceContent, layout.Extension, document, documentContent, layout);

            string parentLayoutName;

            if (layout.TryGet<string>("layout", out parentLayoutName))
            {
                var parentLayout = this.Transaction.Site.Layouts[parentLayoutName];

                content = this.RenderDocumentContentUsingLayout(document, content, parentLayout);
            }

            return content;
        }

        private string RenderContentForExtension(string path, string content, string extension, DocumentFile document, string documentContent, LayoutFile layout)
        {
            var data = new CaseInsensitiveExpando();

            var partialsContent = this.Transaction.Site.Partials.Where(d => d.Rendered).ToDictionary(d => d.Id.Replace('-', '_').Replace('\\', '_').Replace('/', '_'), d => (object)d.RenderedContent);

            var backupContent = document.Content;

            document.Content = documentContent;

            data["Site"] = this.Transaction.Site;
            data["Document"] = document;
            data["Layout"] = layout;
            data["PartialsContent"] = new CaseInsensitiveExpando(partialsContent);
            data["Books"] = this.Transaction.Site.Books == null ? null : this.Transaction.Site.Books.Select(b => b.GetBookWithActiveDocument(document)).ToList();

            var engine = this.Transaction.Engines[extension];

            var result = engine.Render(path, content, data);

            document.Content = backupContent;

            return result;
        }

        private string Summarize(string content)
        {
            string summary = null;

            Match match = Regex.Match(content, "<p>.*?</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success && match.Value != content)
            {
                summary = match.Value;
            }

            return summary;
        }
    }
}
