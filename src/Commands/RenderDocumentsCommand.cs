using System.Collections.Generic;
using System.Linq;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    public class RenderDocumentsCommand
    {
        public IDictionary<string, RenderingEngine> Engines { private get; set; }

        public Site Site { private get; set; }

        public int RenderedDocuments { get; private set; }

        public int Execute()
        {
            using (var tx = new RenderingTransaction(this.Engines, this.Site))
            {
                var documentRendering = new ContentRendering(tx);

                IEnumerable<DocumentFile> renderedDocuments;
                using (var capture = Statistics.Current.Start(StatisticTiming.RenderDocumentContent))
                {
                    renderedDocuments = this.Site.Documents
                                        .Where(d => !d.Draft)
                                        .AsParallel()
                                        .Select(documentRendering.RenderDocumentContent)
                                        .ToList();
                }

                using (var capture = Statistics.Current.Start(StatisticTiming.RenderDocumentLayouts))
                {
                    foreach (var document in renderedDocuments)
                    {
                        var layout = documentRendering.GetLayoutForDocument(document);

                        if (layout == null)
                        {
                            document.RenderedContent = document.Content;
                        }
                        else
                        {
                            document.RenderedContent = documentRendering.RenderDocumentContentUsingLayout(document, document.Content, layout);
                        }

                        document.Rendered = document.RenderedContent != null;
                    }
                }

                return this.RenderedDocuments = renderedDocuments.Count();
            }
        }
    }
}
