using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public IDictionary<string, RenderingEngine> Engines { get; set; }

        public Site Site { get; set; }

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

                return -1;
            }

            // Run the program.
            //
            try
            {
                var program = new Program();

                using (var capture = Statistics.Current.Start(StatisticTiming.Overall))
                {
                    program.Run(commandLine);
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

        public void Run(CommandLine commandLine)
        {
            // Load the engines and configuration.
            //
            var config = LoadConfiguration(commandLine);

            // Load the site documents.
            //
            this.LoadSite(config);

            // Paginate the documents.
            //
            this.PaginateDocuments();

            // TODO: do any other sweeping updates to the documents here.

            // TODO: get the right command to process the commandLine.Command request.
            //       For now it's always render.
            //
            // Render the documents.
            //
            this.RenderDocuments();
        }

        private SiteConfig LoadConfiguration(CommandLine commandLine)
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.LoadedConfiguration))
            {
                this.Engines = RenderingEngine.Load();

                var config = SiteConfig.Load(Path.Combine(commandLine.SitePath, "site.config"));

                config.OutputPath = String.IsNullOrEmpty(commandLine.OutputPath) ? Path.GetFullPath(config.OutputPath) : Path.GetFullPath(commandLine.OutputPath);

                return config;
            }
        }

        private void LoadSite(SiteConfig config)
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.LoadedSite))
            {
                this.Site = Site.Load(config, this.Engines.Keys);
            }

            Statistics.Current.SiteFiles = this.Site.Documents.Count() + this.Site.Files.Count() + this.Site.Layouts.Count();
        }

        private void PaginateDocuments()
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.Pagination))
            {
                var paginate = new PaginateCommand();
                paginate.RootUrl = this.Site.Url;
                paginate.Documents = this.Site.Documents;
                paginate.Execute();

                foreach (var doc in paginate.PagedDocuments)
                {
                    this.Site.Documents.Add(doc);

                    ++Statistics.Current.PagedFiles;
                }
            }
        }

        private void RenderDocuments()
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.Rendered))
            {
                var render = new RenderCommand() { Engines = this.Engines, Site = this.Site };
                render.Execute();
            }
        }
    }
}
