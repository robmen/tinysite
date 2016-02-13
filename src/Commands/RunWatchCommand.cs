using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    public class RunWatchCommand
    {
        private object sync = new object();

        public RunWatchCommand(SiteConfig config, IEnumerable<LastRunDocument> lastRunState, IDictionary<string, RenderingEngine> engines)
        {
            this.Config = config;
            this.Engines = engines;
            this.LastRunState = lastRunState;

            this.Paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            this.Waits = new EventWaitHandle[] {
                new ManualResetEvent(false),
                new AutoResetEvent(false),
            };
        }

        private SiteConfig Config { get; }

        private IDictionary<string, RenderingEngine> Engines { get; }

        private IEnumerable<LastRunDocument> LastRunState { get; }

        private ISet<string> Paths { get; set; }

        private EventWaitHandle[] Waits { get; set; }

        public void Execute()
        {
            this.Config.LiveReloadScript = "<script type=\"text/javascript\" src=\"http://livejs.com/live.js\"></script>";

            var thread = new Thread(RenderThread);

            try
            {
                using (var documentsWatcher = this.CreateWatcher(this.Config.DocumentsPath))
                using (var filesWatcher = this.CreateWatcher(this.Config.FilesPath))
                using (var layoutsWatcher = this.CreateWatcher(this.Config.LayoutsPath))
                {
                    documentsWatcher.EnableRaisingEvents = true;
                    filesWatcher.EnableRaisingEvents = true;
                    layoutsWatcher.EnableRaisingEvents = true;

                    thread.Start(this);

                    var serve = new RunServeCommand();
                    serve.Config = this.Config;
                    serve.Execute();
                }
            }
            finally
            {
                this.Waits[(int)EventTypes.EndWatch].Set();
                thread.Join();
            }
        }

        private FileSystemWatcher CreateWatcher(string path)
        {
            var watcher = new FileSystemWatcher(path);

            watcher.Changed += WatcherFileChanged;
            watcher.Created += WatcherFileChanged;
            watcher.Deleted += WatcherFileChanged;
            watcher.Renamed += WatcherFileRenamed;

            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            watcher.IncludeSubdirectories = true;

            return watcher;
        }

        private void WatcherFileChanged(object sender, FileSystemEventArgs e)
        {
            this.ChangeHappend(e.FullPath);
        }

        private void WatcherFileRenamed(object sender, RenamedEventArgs e)
        {
            this.ChangeHappend(e.FullPath);
        }

        private void ChangeHappend(string path)
        {
            if (this.IgnoreFile(path) || Directory.Exists(path))
            {
                return;
            }

            lock (this.Paths)
            {
                if (this.Paths.Add(path))
                {
                    this.Waits[(int)EventTypes.FilesChange].Set();
                }
            }
        }

        private bool IgnoreFile(string path)
        {
            var filename = Path.GetFileName(path);

            foreach (var ignoreFile in this.Config.IgnoreFiles)
            {
                if (ignoreFile.IsMatch(filename))
                {
                    return true;
                }
            }

            return false;
        }

        private static void RenderThread(object context)
        {
            var command = context as RunWatchCommand;

            do
            {
                Thread.Sleep(10); // wait a bit for any changes to file system to settle.

                IEnumerable<string> paths;
                IEnumerable<EngineWithPath> enginesWithPaths;
                lock (command.Paths)
                {
                    paths = command.Paths.ToList();
                    command.Paths.Clear();

                    enginesWithPaths = command.GetEnginesWithPathsToUnload(paths).ToList();
                }

                var stopwatch = Stopwatch.StartNew();

                Console.WriteLine("Refreshing changed document(s): {0}", String.Join(", ", paths));

                foreach (var group in enginesWithPaths.GroupBy(e => e.Engine))
                {
                    group.Key.Unload(group.Select(e => e.Path).Distinct());
                }

                command.Render();
                Console.WriteLine("  Refresh completed in {0:n0} s", stopwatch.ElapsedMilliseconds);

            } while ((int)EventTypes.FilesChange == WaitHandle.WaitAny(command.Waits));
        }

        private IEnumerable<EngineWithPath> GetEnginesWithPathsToUnload(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                for (var remainder = path; ; remainder = Path.GetFileNameWithoutExtension(remainder))
                {
                    RenderingEngine engine = null;

                    var extension = Path.GetExtension(remainder).TrimStart('.');

                    if (extension.Equals("partial", StringComparison.OrdinalIgnoreCase))
                    {
                    }
                    else if (this.Engines.TryGetValue(extension, out engine))
                    {
                        yield return new EngineWithPath() { Engine = engine, Path = path };
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void Render()
        {
            var command = new RunRenderCommand(this.Config, this.LastRunState, this.Engines);
            command.Execute();
        }

        private class EngineWithPath
        {
            public RenderingEngine Engine { get; set; }

            public string Path { get; set; }
        }

        private enum EventTypes
        {
            EndWatch,
            FilesChange,
        }
    }
}
