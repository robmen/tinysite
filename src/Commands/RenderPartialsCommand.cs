using System.Collections.Generic;
using System.Linq;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    public class RenderPartialsCommand
    {
        public IDictionary<string, RenderingEngine> Engines { private get; set; }

        public Site Site { private get; set; }

        public int RenderedPartials { get; private set; }

        public int Execute()
        {
            using (var tx = new RenderingTransaction(this.Engines, this.Site))
            {
                var contentRendering = new ContentRendering(tx);

                IEnumerable<DocumentFile> renderedPartials;
                using (var capture = Statistics.Current.Start(StatisticTiming.RenderPartialsContent))
                {
                    renderedPartials = this.Site.Partials
                                       .Where(d => !d.Draft)
                                       .AsParallel()
                                       .Select(contentRendering.RenderDocumentContent)
                                       .ToList();
                }

                using (var capture = Statistics.Current.Start(StatisticTiming.RenderPartialsLayouts))
                {
                    foreach (var partial in renderedPartials)
                    {
                        var layout = contentRendering.GetLayoutForDocument(partial);

                        if (layout == null)
                        {
                            partial.RenderedContent = partial.Content;
                        }
                        else
                        {
                            partial.RenderedContent = contentRendering.RenderDocumentContentUsingLayout(partial, partial.Content, layout);
                        }

                        partial.Rendered = true;
                    }
                }

                return this.RenderedPartials = renderedPartials.Count();
            }
        }
    }
}
