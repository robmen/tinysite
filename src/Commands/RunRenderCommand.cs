using System.Collections.Generic;
using System.Threading.Tasks;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    class RunRenderCommand
    {
        public SiteConfig Config { private get; set; }

        public async Task ExecuteAsync()
        {
            var engines = RenderingEngine.Load();

            // Load the site documents.
            //
            var site = await this.LoadSite(this.Config, engines.Keys);

            // Order the documents.
            //
            this.Order(site);

            // Paginate the documents.
            //
            this.Paginate(site);

            // TODO: do any other sweeping updates to the documents here.

            // Render the documents.
            //
            this.Render(site, engines);
        }

        private async Task<Site> LoadSite(SiteConfig config, IEnumerable<string> renderedExtensions)
        {
            Site site;

            using (var capture = Statistics.Current.Start(StatisticTiming.LoadedSite))
            {
                // Load documents.
                IEnumerable<DocumentFile> documents;
                {
                    var load = new LoadDocumentsCommand();
                    load.Author = config.Author;
                    load.OutputRootPath = config.OutputPath;
                    load.RenderedExtensions = renderedExtensions;
                    load.DocumentsPath = config.DocumentsPath;
                    load.RootUrl = config.RootUrl;
                    load.ApplicationUrl = config.Url;
                    load.IgnoreFiles = config.IgnoreFiles;
                    documents = await load.ExecuteAsync();
                }

                // Load files.
                IEnumerable<StaticFile> files;
                {
                    var load = new LoadFilesCommand();
                    load.OutputPath = config.OutputPath;
                    load.FilesPath = config.FilesPath;
                    load.RootUrl = config.RootUrl;
                    load.Url = config.Url;
                    files = load.Execute();
                }

                // Load layouts.
                IEnumerable<LayoutFile> layouts;
                {
                    var load = new LoadLayoutsCommand();
                    load.LayoutsPath = config.LayoutsPath;
                    layouts = await load.ExecuteAsync();
                }

                site = new Site(config, documents, files, layouts);
            }

            Statistics.Current.SiteFiles = site.Documents.Count + site.Files.Count + site.Layouts.Count;

            return site;
        }

        private void Order(Site site)
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.Ordering))
            {
                var order = new OrderCommand();
                order.Documents = site.Documents;
                order.Execute();

                site.Books = order.Books;
            }
        }

        private void Paginate(Site site)
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.Pagination))
            {
                var paginate = new PaginateCommand();
                paginate.RootUrl = site.Url;
                paginate.Documents = site.Documents;
                paginate.Execute();

                foreach (var doc in paginate.PagedDocuments)
                {
                    site.Documents.Add(doc);

                    ++Statistics.Current.PagedFiles;
                }
            }
        }

        private void Render(Site site, IDictionary<string, RenderingEngine> engines)
        {
            using (var rendering = Statistics.Current.Start(StatisticTiming.Rendered))
            {
                using (var capture = Statistics.Current.Start(StatisticTiming.RenderPartials))
                {
                    var render = new RenderPartialsCommand() { Engines = engines, Site = site };
                    render.Execute();

                    Statistics.Current.RenderedPartials = render.RenderedPartials;
                }

                using (var capture = Statistics.Current.Start(StatisticTiming.RenderDocuments))
                {
                    var render = new RenderDocumentsCommand() { Engines = engines, Site = site };
                    render.Execute();

                    Statistics.Current.RenderedDocuments = render.RenderedDocuments;
                }

                using (var capture = Statistics.Current.Start(StatisticTiming.WriteDocuments))
                {
                    var write = new WriteDocumentsCommand() { Documents = site.Documents };
                    write.Execute();

                    Statistics.Current.WroteDocuments = write.WroteDocuments;
                }

                using (var capture = Statistics.Current.Start(StatisticTiming.CopyStaticFiles))
                {
                    var copy = new CopyStaticFilesCommand() { Files = site.Files };
                    copy.Execute();

                    Statistics.Current.CopiedFiles = copy.CopiedFiles;
                }
            }
        }
    }
}
