using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class CommandLine
    {
        public string Command { get; private set; }

        public IEnumerable<string> Errors { get; private set; }

        public string OutputPath { get; private set; }

        public string SitePath { get; private set; }

        public static CommandLine Parse(string[] args)
        {
            var commandLine = new CommandLine();

            var errors = new List<string>();

            if (args.Length == 0)
            {
                errors.Add("Must specify a command: render or watch");
            }
            else
            {
                commandLine.Command = args[0];

                commandLine.SitePath = ".";

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

            commandLine.Errors = errors;

            return commandLine;
        }
    }
}
