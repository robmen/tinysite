using System;
using System.Collections.Generic;

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

        public string OutputPath { get; private set; }

        public string SitePath { get; private set; }

        public bool ReportStatistics { get; private set; }

        public static CommandLine Parse(string[] args)
        {
            var commandLine = new CommandLine();

            var errors = new List<string>();

            if (args.Length == 0)
            {
                errors.Add("Must specify a command: render, serve or watch");
            }
            else
            {
                var command = ProcessingCommand.Unknown;

                if (!Enum.TryParse<ProcessingCommand>(args[0], true, out command))
                {
                    errors.Add(String.Format("Unknown processing command: {0}. Supported commands are: render, serve or watch", args[0]));
                }
                else
                {
                    commandLine.Command = command;

                    commandLine.SitePath = ".";

                    commandLine.ReportStatistics = command == ProcessingCommand.Render;

                    for (int i = 1; i < args.Length; ++i)
                    {
                        var arg = args[i];
                        if (arg.StartsWith("-") || arg.StartsWith("/"))
                        {
                            var param = arg.Substring(1);
                            switch (param.ToLowerInvariant())
                            {
                                case "o":
                                case "out":
                                    commandLine.OutputPath = args[++i];
                                    break;

                                default:
                                    errors.Add(String.Format("Unknown command-line paramter: {0}", arg));
                                    break;
                            }
                        }
                        else
                        {
                            commandLine.SitePath = arg;
                        }
                    }
                }
            }

            commandLine.Errors = errors;

            return commandLine;
        }
    }
}
