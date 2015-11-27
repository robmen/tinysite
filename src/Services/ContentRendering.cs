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

        private RenderingTransaction Transaction { get; }

        public DocumentFile RenderDocumentContent(DocumentFile document)
        {
            var content = document.SourceContent;

            var layout = document?.Layouts.FirstOrDefault();

            foreach (var extension in document.ExtensionsForRendering)
            {
                content = this.RenderContentForExtension(document.SourcePath, content, extension, document, content, layout);
            }

            document.Content = content;

            if (String.IsNullOrEmpty(document.Summary) && !String.IsNullOrEmpty(document.Content))
            {
                document.Summary = Summarize(document.Content);
            }

            if (layout != null)
            {
                this.AssignLayoutMetadataToDocument(document, layout);
            }

            return document;
        }

        public string RenderDocumentContentUsingLayout(DocumentFile document, string documentContent, LayoutFile layout)
        {
            return this.RenderContentForExtension(layout.SourcePath, layout.SourceContent, layout.Extension, document, documentContent, layout);
        }

        private string RenderContentForExtension(string path, string content, string extension, DocumentFile document, string documentContent, LayoutFile layout)
        {
            var data = new CaseInsensitiveExpando();

            var partialsContent = new PartialsContent(this.Transaction.Site.Partials);

            var backupContent = document.Content;

            document.Content = documentContent;

            data["Site"] = this.Transaction.Site;
            data["Document"] = document;
            data["Layout"] = layout;
            data["PartialsContent"] = partialsContent;
            data["Books"] = this.Transaction.Site.Books?.Select(b => b.GetBookWithActiveDocument(document)).ToList();

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

        private void AssignLayoutMetadataToDocument(DocumentFile document, LayoutFile layout)
        {
            foreach (var metadataKeyValue in layout)
            {
                if (!metadataKeyValue.Key.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("Extension", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("Layout", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("Modified", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("Name", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("SourcePath", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("SourceContent", StringComparison.OrdinalIgnoreCase) &&
                    !document.ContainsKey(metadataKeyValue.Key))
                {
                    document.Add(metadataKeyValue);
                }
            }
        }
    }
}
