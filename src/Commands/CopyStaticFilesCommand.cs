using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TinySite.Models;

namespace TinySite.Commands
{
    public class CopyStaticFilesCommand
    {
        public IEnumerable<StaticFile> Files { private get; set; }

        public int CopiedFiles { get; private set; }

        public async Task ExecuteAsync()
        {
            var streams = new List<Stream>(this.Files.Count() * 2);

            var copyTasks = new List<Task>();

            try
            {
                foreach (var file in this.Files)
                {
                    var folder = Path.GetDirectoryName(file.OutputPath);

                    Directory.CreateDirectory(folder);

                    var source = File.Open(file.SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);

                    streams.Add(source);

                    var target = File.Open(file.OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete);

                    streams.Add(target);

                    var copyTask = source.CopyToAsync(target);

                    copyTasks.Add(copyTask);

                    ++this.CopiedFiles;
                }

                await Task.WhenAll(copyTasks);
            }
            finally
            {
                foreach (var stream in streams)
                {
                    stream.Dispose();
                }
            }
        }
    }
}
