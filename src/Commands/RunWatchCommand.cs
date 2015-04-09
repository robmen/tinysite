using System;
using System.IO;
using System.Threading.Tasks;
using TinySite.Models;

namespace TinySite.Commands
{
    public class RunWatchCommand
    {
        private object sync = new object();

        public SiteConfig Config { private get; set; }

        private int ChangeCount { get; set; }

        public async Task ExecuteAsync()
        {
            await this.Render();

            using (var documentsWatcher = this.CreateWatcher(this.Config.DocumentsPath))
            using (var filesWatcher = this.CreateWatcher(this.Config.FilesPath))
            using (var layoutsWatcher = this.CreateWatcher(this.Config.LayoutsPath))
            {
                documentsWatcher.EnableRaisingEvents = true;
                filesWatcher.EnableRaisingEvents = true;
                layoutsWatcher.EnableRaisingEvents = true;

                var serve = new RunServeCommand();
                serve.Config = this.Config;
                serve.Execute();
            }
        }

        private async Task Render()
        {
            var command = new RunRenderCommand();
            command.Config = this.Config;
            await command.ExecuteAsync();
        }

        private FileSystemWatcher CreateWatcher(string path)
        {
            var watcher = new FileSystemWatcher(path);

            watcher.Changed += WatcherFileChanged;
            watcher.Created += WatcherFileChanged;
            watcher.Deleted += WatcherFileChanged;
            watcher.Renamed += WatcherFileRenamed;

            watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            watcher.IncludeSubdirectories = true;

            return watcher;
        }

        private async void WatcherFileChanged(object sender, FileSystemEventArgs e)
        {
            await this.ChangeHappend(e.FullPath);
        }

        private async void WatcherFileRenamed(object sender, RenamedEventArgs e)
        {
            await this.ChangeHappend(e.FullPath);
        }

        private async Task ChangeHappend(string path)
        {
            lock (sync)
            {
                ++this.ChangeCount;

                if (this.ChangeCount > 1)
                {
                    return;
                }
            }

            while (true)
            {
                lock (sync)
                {
                    this.ChangeCount = 1;
                }

                Console.WriteLine("Rendering change to: {0}", path);
                await this.Render();
                Console.WriteLine("  Rendering complete for: {0}", path);

                lock (sync)
                {
                    if (--this.ChangeCount == 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}
