using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TinySite.Commands;
using TinySite.Models;
using TinySite.Services;

namespace TinySite
{
    public class Program
    {
        public Program()
        {
            Statistics.Current = new Statistics();
        }

        public static int Main(string[] args)
        {
            // Parse the command line and if there are any errors, bail.
            //
            var commandLine = CommandLine.Parse(args);

            if (commandLine.Errors.Any())
            {
                foreach (var error in commandLine.Errors)
                {
                    Console.WriteLine(error);
                }

                return -2;
            }

            // Run the program.
            //
            try
            {
                Console.WriteLine("Processing site: {0}", Path.GetFullPath(commandLine.SitePath));

                var program = new Program();

                using (var capture = Statistics.Current.Start(StatisticTiming.Overall))
                {
                    AsyncPump.Run(async delegate { await program.Run(commandLine); });
                }

                Statistics.Current.Report();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                return -1;
            }

            return 0;
        }

        public async Task Run(CommandLine commandLine)
        {
            var engines = RenderingEngine.Load();

            // Load the site documents.
            //
            var site = await this.Load(commandLine.SitePath, commandLine.OutputPath, engines.Keys);

            // Order the documents.
            //
            this.Order(site);

            // Paginate the documents.
            //
            this.Paginate(site);

            // TODO: do any other sweeping updates to the documents here.

            // TODO: get the right command to process the commandLine.Command request.
            //       For now it's always render.
            //
            // Render the documents.
            //
            this.Render(site, engines);
        }

        private async Task<Site> Load(string sitePath, string outputPath, IEnumerable<string> renderedExtensions)
        {
            Site site;

            SiteConfig config;

            using (var capture = Statistics.Current.Start(StatisticTiming.LoadedConfiguration))
            {
                var command = new LoadSiteConfigCommand();
                command.ConfigPath = Path.Combine(sitePath, "site.config");
                command.OutputPath = outputPath;
                config = await command.ExecuteAsync();
            }

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
