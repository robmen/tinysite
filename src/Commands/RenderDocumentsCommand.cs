using System;
using System.Collections.Generic;
using System.Linq;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    public class RenderDocumentsCommand
    {
        public RenderDocumentsCommand(IDictionary<string, RenderingEngine> engines, Site site)
        {
            this.Engines = engines;
            this.Site = site;
        }

        public int RenderedDocuments { get; private set; }

        private IDictionary<string, RenderingEngine> Engines { get; }

        private Site Site { get; }

        public int Execute()
        {
            using (var tx = new RenderingTransaction(this.Engines, this.Site))
            {
                var documentRendering = new ContentRendering(tx);

                IEnumerable<DocumentFile> renderedDocuments;
                using (var capture = Statistics.Current.Start(StatisticTiming.RenderDocumentContent))
                {
                    renderedDocuments = this.Site.Documents
                                        .Where(d => !d.Draft && !d.Unmodified)
                                        .AsParallel()
                                        .Select(documentRendering.RenderDocumentContent)
                                        .ToList();
                }

                using (var capture = Statistics.Current.Start(StatisticTiming.RenderDocumentLayouts))
                {
                    foreach (var document in renderedDocuments)
                    {
                        var content = document.Content;

                        foreach (var layout in document.Layouts)
                        {
                            content = documentRendering.RenderDocumentContentUsingLayout(document, content, layout);
                        }

                        document.RenderedContent = content;

                        document.Rendered = (document.RenderedContent != null);
                    }
                }

                return this.RenderedDocuments = renderedDocuments.Count();
            }
        }
    }
}
