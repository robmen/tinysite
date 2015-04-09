using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var config = await this.LoadConfig(commandLine.SitePath, commandLine.OutputPath);

            switch (commandLine.Command)
            {
                case ProcessingCommand.Render:
                    await this.RunRenderCommand(config);

                    Statistics.Current.Report();
                    break;

                case ProcessingCommand.Serve:
                    this.RunServeCommand(config);
                    break;

                default:
                    throw new InvalidOperationException(String.Format("Unknown ProcessingCommand: {0}", commandLine.Command));
            }
        }

        private async Task<SiteConfig> LoadConfig(string sitePath, string outputPath)
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.LoadedConfiguration))
            {
                var command = new LoadSiteConfigCommand();
                command.ConfigPath = Path.Combine(sitePath, "site.config");
                command.OutputPath = outputPath;
                return await command.ExecuteAsync();
            }
        }

        private async Task RunRenderCommand(SiteConfig config)
        {
            var engines = RenderingEngine.Load();

            // Load the site documents.
            //
            var site = await this.LoadSite(config, engines.Keys);

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

        private void RunServeCommand(SiteConfig config)
        {
            var iise = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"IIS Express\iisexpress.exe");
            var args = String.Format("/path:\"{0}\" /systray:false", config.OutputPath.TrimEnd('\\'));

            if (!File.Exists(iise))
            {
                Console.WriteLine();
                Console.Error.WriteLine("Could not find IIS Express at path: {0}. You will need to install IIS Express to use the 'serve' command. Download: http://www.microsoft.com/en-us/download/details.aspx?id=34679", iise);
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Press 'q' to quit.");
            Console.WriteLine();

            using (var process = new Process())
            {
                process.StartInfo.FileName = iise;
                process.StartInfo.Arguments = args;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WorkingDirectory = config.SitePath;
                process.Start();
                process.WaitForExit();
            }

            Console.WriteLine("IIS Express exited.");
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
