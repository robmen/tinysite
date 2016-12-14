using System;
using System.Linq;
using System.Text.RegularExpressions;
using TinySite.Models;
using TinySite.Models.Dynamic;

namespace TinySite.Services
{
    public class ContentRendering
    {
        private static readonly Regex _summarizeRegex = new Regex("<p>.*?</p>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex _stripHtmlRegex = new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.Singleline);

        public ContentRendering(RenderingTransaction transaction)
        {
            this.Transaction = transaction;
        }

        private RenderingTransaction Transaction { get; }

        public DataFile RenderDataContent(DataFile dataFile)
        {
            RenderingEngine engine;

            if (this.Transaction.Engines.TryGetValue(dataFile.Extension, out engine))
            {
                var data = new DynamicRenderData(dataFile, this.Transaction.Site);

                dataFile.Content = engine.Render(dataFile, dataFile.SourceContent, data);
            }

            return dataFile;
        }

        public DocumentFile RenderDocumentContent(DocumentFile document)
        {
            var content = document.SourceContent;

            var layout = document?.Layouts.FirstOrDefault();

            foreach (var extension in document.ExtensionsForRendering)
            {
                content = this.RenderContentForExtension(document, content, extension, document, content, layout);
            }

            document.Content = content;

            if (String.IsNullOrEmpty(document.Summary) && !String.IsNullOrEmpty(document.Content))
            {
                document.Summary = Summarize(document.Content);
            }

            if (String.IsNullOrEmpty(document.Description) && !String.IsNullOrEmpty(document.Summary))
            {
                document.Description = StripHtml(document.Summary);
            }

            if (layout != null)
            {
                this.AssignLayoutMetadataToDocument(document, layout);
            }

            return document;
        }

        public string RenderDocumentContentUsingLayout(DocumentFile document, string documentContent, LayoutFile layout)
        {
            var content = this.RenderContentForExtension(layout, layout.SourceContent, layout.Extension, document, documentContent, layout);

            document.AddContributingFile(layout);

            this.AssignLayoutMetadataToDocument(document, layout);

            return content;
        }

        private string RenderContentForExtension(SourceFile source, string content, string extension, DocumentFile contextDocument, string documentContent, LayoutFile contextLayout)
        {
            RenderingEngine engine;

            if (this.Transaction.Engines.TryGetValue(extension, out engine))
            {
                var backupContent = contextDocument.Content;

                try
                {
                    contextDocument.Content = documentContent;

                    dynamic data = new DynamicRenderDocument(contextDocument, contextLayout, this.Transaction.Site);

                    var result = engine.Render(source, content, data);

                    return result;
                }
                finally
                {
                    contextDocument.Content = backupContent;
                }
            }
            else
            {
                Console.WriteLine("Cannot find a rendering engine for extension: {0}", extension);
                return null;
            }
        }

        private string Summarize(string content)
        {
            string summary = null;

            var match = _summarizeRegex.Match(content);
            if (match.Success && match.Value != content)
            {
                summary = match.Value;
            }

            return summary;
        }

        private string StripHtml(string content)
        {
            var stripped = _stripHtmlRegex.Replace(content, String.Empty);
            return stripped;
        }

        private void AssignLayoutMetadataToDocument(DocumentFile document, LayoutFile layout)
        {
            foreach (var metadataKeyValue in layout.Metadata)
            {
                if (!metadataKeyValue.Key.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("Extension", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("Layout", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("Modified", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("Name", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("SourcePath", StringComparison.OrdinalIgnoreCase) &&
                    !metadataKeyValue.Key.Equals("SourceContent", StringComparison.OrdinalIgnoreCase) &&
                    !document.Metadata.Contains(metadataKeyValue.Key))
                {
                    document.Metadata.Add(metadataKeyValue.Key, metadataKeyValue.Value);
                }
            }
        }
    }
}
