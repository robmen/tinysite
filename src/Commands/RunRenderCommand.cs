using System.Collections.Generic;
using System.Threading.Tasks;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    class RunRenderCommand
    {
        public SiteConfig Config { private get; set; }

        public IDictionary<string, RenderingEngine> Engines { get; set; }

        public async Task ExecuteAsync()
        {
            // Load the site documents.
            //
            var site = await this.LoadSite(this.Config, this.Engines.Keys);

            // Order the documents.
            //
            this.Order(site);

            // Paginate the documents.
            //
            this.Paginate(site);

            // TODO: do any other sweeping updates to the documents here.

            // Render the documents.
            //
            this.Render(site, this.Engines);
        }

        private async Task<Site> LoadSite(SiteConfig config, IEnumerable<string> renderedExtensions)
        {
            Site site;

            using (var capture = Statistics.Current.Start(StatisticTiming.LoadedSite))
            {
                // Load layouts.
                LayoutFileCollection layouts;
                {
                    var load = new LoadLayoutsCommand();
                    load.LayoutsPath = config.LayoutsPath;
                    var loaded = await load.ExecuteAsync();

                    layouts = new LayoutFileCollection(loaded);
                }

                // Load documents.
                IEnumerable<DocumentFile> documents;
                {
                    var load = new LoadDocumentsCommand();
                    load.Author = config.Author;
                    load.OutputRootPath = config.OutputPath;
                    load.Layouts = layouts;
                    load.RenderedExtensions = renderedExtensions;
                    load.DefaultLayoutForExtension = config.DefaultLayoutForExtension;
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
                paginate.Site = site;
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
                using (var capture = Statistics.Current.Start(StatisticTiming.RenderDocuments))
                {
                    var render = new RenderDocumentsCommand(engines, site);
                    render.Execute();

                    Statistics.Current.RenderedDocuments = render.RenderedDocuments;
                }

                using (var capture = Statistics.Current.Start(StatisticTiming.WriteDocuments))
                {
                    var write = new WriteDocumentsCommand(site.Documents);
                    write.Execute();

                    Statistics.Current.WroteDocuments = write.WroteDocuments;
                }

                using (var capture = Statistics.Current.Start(StatisticTiming.CopyStaticFiles))
                {
                    var copy = new CopyStaticFilesCommand(site.Files);
                    copy.Execute();

                    Statistics.Current.CopiedFiles = copy.CopiedFiles;
                }
            }
        }
    }
}
