using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    public class RenderDocumentsCommand
    {
        public IDictionary<string, RenderingEngine> Engines { private get; set; }

        public Site Site { private get; set; }

        public int RenderedDocuments { get; private set; }

        public void Execute()
        {
            using (var tx = new RenderingTransaction(this.Engines, this.Site))
            {
                // TODO: Eventually skip documents that are up to date and don't need to be rendered again.

                // TODO: do not enable parallel render until we sort out how to get the document summary
                //       always set correctly when rendering in parallel.
                //Statistics.Current.RenderedDocuments = this.Site.Documents
                //    .Where(d => !d.Draft)
                //    .AsParallel()
                //    .Select(
                //        document =>
                //        {
                //            document.RenderDocument();

                //            return document;
                //        })
                //    .Count();

                foreach (var document in this.Site.Documents.Where(d => !d.Draft))
                {
                    document.RenderDocument();

                    ++this.RenderedDocuments;
                }
            }
        }
    }
}
