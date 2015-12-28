using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TinySite.Models;

namespace TinySite.Commands
{
    public class LoadDataFilesCommand
    {
        public LoadDataFilesCommand(string dataPath)
        {
            this.DataPath = dataPath;
        }

        public IEnumerable<DataFile> DataFiles { get; private set; }

        private string DataPath { get; }

        public async Task<IEnumerable<DataFile>> ExecuteAsync()
        {
            var loadTasks = this.LoadDataFilesAscyn();

            return this.DataFiles = await Task.WhenAll(loadTasks);
        }

        private IEnumerable<Task<DataFile>> LoadDataFilesAscyn()
        {
            if (Directory.Exists(this.DataPath))
            {
                foreach (var path in Directory.GetFiles(this.DataPath, "*", SearchOption.AllDirectories))
                {
                    yield return this.LoadLayoutAsync(path);
                }
            }
        }

        private async Task<DataFile> LoadLayoutAsync(string path)
        {
            var parser = new ParseDocumentCommand(path);
            await parser.ExecuteAsync();

            return new DataFile(path, this.DataPath, parser.Content, parser.Metadata, parser.Queries);
        }
    }
}
