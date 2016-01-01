using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TinySite.Models;

namespace TinySite.Commands
{
    public class LoadDataFilesCommand
    {
        public LoadDataFilesCommand(string dataPath, IEnumerable<AdditionalMetadataConfig> additionalMetadata, IEnumerable<Regex> ignoreFiles)
        {
            this.DataPath = dataPath;
            this.AdditionalMetadataForFiles = additionalMetadata;
            this.IgnoreFiles = ignoreFiles;
        }

        public IEnumerable<DataFile> DataFiles { get; private set; }

        private string DataPath { get; }

        private IEnumerable<AdditionalMetadataConfig> AdditionalMetadataForFiles { get; }

        private IEnumerable<Regex> IgnoreFiles { get; }

        public async Task<IEnumerable<DataFile>> ExecuteAsync()
        {
            var loadTasks = this.LoadDataFilesAsync();

            return this.DataFiles = await Task.WhenAll(loadTasks);
        }

        private IEnumerable<Task<DataFile>> LoadDataFilesAsync()
        {
            if (Directory.Exists(this.DataPath))
            {
                foreach (var path in Directory.GetFiles(this.DataPath, "*", SearchOption.AllDirectories))
                {
                    if (this.IgnoreFiles != null && this.IgnoreFile(path))
                    {
                        continue;
                    }

                    yield return this.LoadLayoutAsync(path);
                }
            }
        }

        private bool IgnoreFile(string path)
        {
            var filename = Path.GetFileName(path);

            foreach (var ignoreFile in this.IgnoreFiles)
            {
                if (ignoreFile.IsMatch(filename))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<DataFile> LoadLayoutAsync(string path)
        {
            var parser = new ParseDocumentCommand(path);
            await parser.ExecuteAsync();

            if (this.AdditionalMetadataForFiles != null)
            {
                var rootPath = Path.GetDirectoryName(this.DataPath.TrimEnd('\\'));
                var relativePath = path.Substring(rootPath.Length + 1);

                foreach (var additionalMetadataConfig in this.AdditionalMetadataForFiles)
                {
                    if (additionalMetadataConfig.Match.IsMatch(relativePath))
                    {
                        parser.Metadata.AssignFrom(path, additionalMetadataConfig.Metadata);
                    }
                }
            }

            return new DataFile(path, this.DataPath, parser.Content, parser.Metadata, parser.Queries);
        }
    }
}
