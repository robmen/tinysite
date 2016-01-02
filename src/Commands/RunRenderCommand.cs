using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    class RunRenderCommand
    {
        public RunRenderCommand(SiteConfig config, IEnumerable<LastRunDocument> lastRunState, IDictionary<string, RenderingEngine> engines)
        {
            this.Config = config;
            this.Engines = engines;
            this.InitialLastRunState = lastRunState;
        }

        public IEnumerable<LastRunDocument> LastRunState { get; private set; }

        private SiteConfig Config { get; }

        private IDictionary<string, RenderingEngine> Engines { get; }

        private IEnumerable<LastRunDocument> InitialLastRunState { get; }

        public async Task ExecuteAsync()
        {
            // Load the site documents.
            //
            var site = await this.LoadSite(this.Config, this.InitialLastRunState, this.Engines.Keys);

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

            this.LastRunState = this.CalculateLastRunState(site).ToList();
        }

        private async Task<Site> LoadSite(SiteConfig config, IEnumerable<LastRunDocument> lastRunState, IEnumerable<string> renderedExtensions)
        {
            Site site;

            using (var capture = Statistics.Current.Start(StatisticTiming.LoadedSite))
            {
                // Load layouts.
                LayoutFileCollection layouts;
                {
                    var load = new LoadLayoutsCommand(config.LayoutsPath, config.AdditionalMetadataForFiles, config.IgnoreFiles);
                    var loaded = await load.ExecuteAsync();

                    layouts = new LayoutFileCollection(loaded);
                }

                // Load data files.
                IEnumerable<DataFile> data;
                {
                    var load = new LoadDataFilesCommand(config.DataPath, config.AdditionalMetadataForFiles, config.IgnoreFiles);
                    data = await load.ExecuteAsync();
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
                    load.AdditionalMetadataForFiles = config.AdditionalMetadataForFiles;
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

                // Calculate unmodified state.
                {
                    var unmodified = new SetUnmodifiedCommand(config.SitePath, documents, files, this.InitialLastRunState);
                    unmodified.Execute();
                }

                site = new Site(config, data, documents, files, layouts);
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

                    Statistics.Current.RenderedData = render.RenderedData;

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

        private IEnumerable<LastRunDocument> CalculateLastRunState(Site site)
        {
            var dt = this.InitialLastRunState.ToDictionary(l => l.Path);

            foreach (var document in site.Documents.Where(d => !d.Cloned && !d.Unmodified))
            {
                var contributors = document.AllContributingFiles()
                    .Select(d => new LastRunContributingFile(d.SourceRelativePath, d.Modified));

                var lastRunDocument = new LastRunDocument(document.SourceRelativePath, document.Modified, contributors);

                dt.Remove(document.SourceRelativePath);

                yield return lastRunDocument;
            }

            foreach (var lrd in dt.Values)
            {
                yield return lrd;
            }
        }
    }
}
