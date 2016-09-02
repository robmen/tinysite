using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
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

            this.LastRunJsonSettings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore,
            };
        }

        private JsonSerializerSettings LastRunJsonSettings { get;  }

        public static int Main(string[] args)
        {
            // Parse the command line and if there are any errors, bail.
            //
            var commandLine = CommandLine.Parse(args);

            if (commandLine.Help)
            {
                CommandLine.DisplayHelp();
                return 0;
            }

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
                    program.Run(commandLine);
                }

                if (commandLine.ReportStatistics)
                {
                    Statistics.Current.Report();
                }
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
            var config = this.LoadConfig(commandLine.SitePath, commandLine.OutputPath);

            var lastRunState = this.LoadLastRunState(commandLine.SitePath);

            switch (commandLine.Command)
            {
                case ProcessingCommand.Render:
                    {
                        var engines = RenderingEngine.Load();
                        var command = new RunRenderCommand(config, lastRunState, engines);
                        lastRunState = command.Execute();
                    }
                    break;

                case ProcessingCommand.Serve:
                    {
                        var command = new RunServeCommand(config, commandLine.Port);
                        command.Execute();
                    }
                    break;

                case ProcessingCommand.Watch:
                    {
                        var engines = RenderingEngine.Load();
                        var command = new RunWatchCommand(config, commandLine.Port, lastRunState, engines);
                        command.Execute();
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unknown ProcessingCommand: {commandLine.Command}");
            }

            this.SaveLastRunState(commandLine.SitePath, lastRunState);
        }

        private SiteConfig LoadConfig(string sitePath, string outputPath)
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.LoadedConfiguration))
            {
                var configPath = Path.Combine(sitePath, "site.json");
                if (!File.Exists(configPath))
                {
                    configPath = Path.Combine(sitePath, "site.config");
                }

                var command = new LoadSiteConfigCommand();
                command.ConfigPath = configPath;
                command.OutputPath = outputPath;
                return command.Execute();
            }
        }

        private IEnumerable<LastRunDocument> LoadLastRunState(string sitePath)
        {
            var statePath = Path.GetFullPath(Path.Combine(sitePath, "site.lastrun"));

            if (!File.Exists(statePath))
            {
                return Enumerable.Empty<LastRunDocument>();
            }

            string json;
            using (var reader = new StreamReader(statePath))
            {
                json = reader.ReadToEnd();
            }

            var result = JsonConvert.DeserializeObject<IEnumerable<LastRunDocument>>(json, this.LastRunJsonSettings);
            return result;
        }

        private void SaveLastRunState(string sitePath, IEnumerable<LastRunDocument> lastRunState)
        {
            var statePath = Path.GetFullPath(Path.Combine(sitePath, "site.lastrun"));

            if (!lastRunState.Any())
            {
                File.Delete(statePath);
            }
            else
            {
                var json = JsonConvert.SerializeObject(lastRunState, this.LastRunJsonSettings);

                var bytes = Encoding.UTF8.GetBytes(json);

                using (var writer = File.Open(statePath, FileMode.Create, FileAccess.Write, FileShare.Delete))
                {
                    writer.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
