using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinySite.Models;

namespace TinySite.Commands
{
    public class CopyStaticFilesCommand
    {
        public CopyStaticFilesCommand(IEnumerable<StaticFile> files)
        {
            this.Files = files;
        }

        public int CopiedFiles { get; private set; }

        private IEnumerable<StaticFile> Files { get; }

        public int Execute()
        {
            return this.CopiedFiles = this.Files
                .Where(f => !f.Unmodified)
                .AsParallel()
                .Select(CopyStaticFile)
                .Count();
        }

#if false
        public async Task<int> Execute()
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

            return this.CopiedFiles;
        }
#endif

        private static StaticFile CopyStaticFile(StaticFile file)
        {
            var folder = Path.GetDirectoryName(file.OutputPath);

            Directory.CreateDirectory(folder);

            File.Copy(file.SourcePath, file.OutputPath, true);

            File.SetCreationTime(file.OutputPath, file.Date);

            File.SetLastWriteTime(file.OutputPath, file.Modified);

            return file;
        }
    }
}
