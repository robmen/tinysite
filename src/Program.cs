using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TinySite.Commands;
using TinySite.Models;

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
                    {
                        var command = new RunRenderCommand();
                        command.Config = config;
                        await command.ExecuteAsync();
                    }

                    Statistics.Current.Report();
                    break;

                case ProcessingCommand.Serve:
                    {
                        var command = new RunServeCommand();
                        command.Config = config;
                        command.Execute();
                    }
                    break;

                case ProcessingCommand.Watch:
                    {
                        var command = new RunWatchCommand();
                        command.Config = config;
                        await command.ExecuteAsync();
                    }
                    break;

                default:
                    throw new InvalidOperationException(String.Format("Unknown ProcessingCommand: {0}", commandLine.Command));
            }
        }

        private async Task<SiteConfig> LoadConfig(string sitePath, string outputPath)
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
                return await command.ExecuteAsync();
            }
        }
    }
}
