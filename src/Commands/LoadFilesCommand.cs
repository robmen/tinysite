using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinySite.Models;

namespace TinySite.Commands
{
    public class LoadFilesCommand
    {
        public string FilesPath { get; set; }

        public string OutputPath { get; set; }

        public string Url { get; set; }

        public string RootUrl { get; set; }

        public IEnumerable<StaticFile> Files { get; private set; }

        public IEnumerable<StaticFile> Execute()
        {
            if (!Directory.Exists(this.FilesPath))
            {
                return Enumerable.Empty<StaticFile>();
            }

            return this.Files = Directory.GetFiles(this.FilesPath, "*", SearchOption.AllDirectories)
                .AsParallel()
                .Select(file => new StaticFile(file, this.FilesPath, this.OutputPath, this.Url, this.RootUrl));
        }
    }
}
