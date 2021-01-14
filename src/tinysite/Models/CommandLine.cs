using System;
using System.Collections.Generic;
using System.Reflection;

namespace TinySite.Models
{
    public enum ProcessingCommand
    {
        Unknown,
        Render,
        Serve,
        Watch,
    }

    public class CommandLine
    {
        public ProcessingCommand Command { get; private set; }

        public IEnumerable<string> Errors { get; private set; }

        public bool Help { get; private set; }

        public string OutputPath { get; private set; }

        public int Port { get; private set; } = -1;

        public string SitePath { get; private set; }

        public bool ReportStatistics { get; private set; }

        public static CommandLine Parse(string[] args)
        {
            var commandLine = new CommandLine();

            var errors = new List<string>();

            if (args.Length == 0)
            {
                commandLine.Help = true;
                return commandLine;
            }

            commandLine.Command = ProcessingCommand.Unknown;

            commandLine.SitePath = ".";

            for (int i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    var param = arg.Substring(1);
                    switch (param.ToLowerInvariant())
                    {
                        case "?":
                        case "help":
                            commandLine.Help = true;
                            return commandLine;

                        case "o":
                        case "out":
                            if (i < args.Length)
                            {
                                commandLine.OutputPath = args[++i];
                            }
                            else
                            {
                                errors.Add("Must specify output folder for -out command-line switch.");
                            }
                            break;

                        case "port":
                            {
                                int port;
                                if (i < args.Length && Int32.TryParse(args[++i], out port) && port > 0)
                                {
                                    commandLine.Port = port;
                                }
                                else
                                {
                                    errors.Add($"Port must be provided as a positive number: {arg}");
                                }
                            }
                            break;

                        default:
                            errors.Add($"Unknown command-line paramter: {arg}");
                            break;
                    }
                }
                else if (commandLine.Command == ProcessingCommand.Unknown)
                {
                    ProcessingCommand command;

                    if (!Enum.TryParse(arg, true, out command))
                    {
                        errors.Add($"Unknown processing command: {arg}. Supported commands are: render, serve or watch");
                    }

                    commandLine.Command = command;
                }
                else
                {
                    commandLine.SitePath = arg;
                }
            }

            commandLine.ReportStatistics = (commandLine.Command == ProcessingCommand.Render);

            commandLine.Errors = errors;

            return commandLine;
        }

        public static void DisplayHelp()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = String.Empty;

            var customAttributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            if (null != customAttributes && 0 < customAttributes.Length)
            {
                var attribute = customAttributes[0] as AssemblyFileVersionAttribute;
                version = attribute?.Version ?? String.Empty;
            }

            Console.WriteLine("tinysite.exe <render|serve|watch> [-port #] [-out folder] [root]");
            Console.WriteLine(" v{0}", version);
            Console.WriteLine();
            Console.WriteLine(" Commands (select one):");
            Console.WriteLine("  render  process documents to output folder");
            Console.WriteLine("  serve   start IIS Express to serve rendered ouput");
            Console.WriteLine("  watch   start IIS Express and rerender when files change");
            Console.WriteLine();
            Console.WriteLine("  -port   set the port for IIS Express to listen");
            Console.WriteLine("  -out    specify the folder where to render output");
            Console.WriteLine();
            Console.WriteLine("  root    folder containing site.json, defaults to '.'");
        }
    }
}
