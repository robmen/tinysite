using System;
using System.Diagnostics;
using System.IO;
using TinySite.Models;

namespace TinySite.Commands
{
    public class RunServeCommand
    {
        public RunServeCommand(SiteConfig config, int port)
        {
            this.Config = config;
            this.Port = port;
        }

        private SiteConfig Config { get; }

        private int Port { get; }

        public void Execute()
        {
            var iise = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"IIS Express\iisexpress.exe");
            var args = String.Format("/path:\"{0}\" /systray:false", this.Config.OutputPath.TrimEnd('\\'));

            if (this.Port > 0)
            {
                args += $" /port:{this.Port}";
            }

            if (!File.Exists(iise))
            {
                Console.WriteLine();
                Console.Error.WriteLine("Could not find IIS Express at path: {0}. You will need to install IIS Express to use the 'serve' command. Download: http://www.microsoft.com/en-us/download/details.aspx?id=34679", iise);
                return;
            }

            if (!Directory.Exists(this.Config.OutputPath))
            {
                Directory.CreateDirectory(this.Config.OutputPath);
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
                process.StartInfo.WorkingDirectory = this.Config.SitePath;
                process.Start();
                process.WaitForExit();
            }

            Console.WriteLine("IIS Express exited.");
        }
    }
}
