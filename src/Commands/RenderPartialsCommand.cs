using System.Collections.Generic;
using System.Linq;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    public class RenderPartialsCommand
    {
        public RenderPartialsCommand(IDictionary<string, RenderingEngine> engines, Site site)
        {
            this.Engines = engines;
            this.Site = site;
        }

        public int RenderedPartials { get; private set; }

        private IDictionary<string, RenderingEngine> Engines { get; }

        private Site Site { get; }

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
                        var content = partial.Content;

                        foreach (var layout in partial.Layouts)
                        {
                            content = contentRendering.RenderDocumentContentUsingLayout(partial, content, layout);
                        }

                        partial.RenderedContent = content;

                        partial.Rendered = true;
                    }
                }

                return this.RenderedPartials = renderedPartials.Count();
            }
        }
    }
}
