using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TinySite.Models;

namespace TinySite.Commands
{
    public class LoadDataFilesCommand
    {
        private static readonly Regex DateFromFileName = new Regex(@"^\s*(?<year>\d{4})-(?<month>\d{1,2})-(?<day>\d{1,2})([Tt@](?<hour>\d{1,2})\.(?<minute>\d{1,2})(\.(?<second>\d{1,2}))?)?[-\s]\s*", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

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

            var metadataDate = parser.Date;

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

            var disableDateFromFileName = parser.Metadata.GetAndRemove("DisableDateFromFileName", false);

            // If the parser metadata didn't specify the date, try to use a date from the filename.
            //
            if (!disableDateFromFileName && !metadataDate.HasValue)
            {
                var fileName = Path.GetFileName(path);
                var match = DateFromFileName.Match(fileName);

                if (match.Success)
                {
                    var year = Convert.ToInt32(match.Groups[1].Value, 10);
                    var month = Convert.ToInt32(match.Groups[2].Value, 10);
                    var day = Convert.ToInt32(match.Groups[3].Value, 10);
                    var hour = match.Groups[4].Success ? Convert.ToInt32(match.Groups[4].Value, 10) : 0;
                    var minute = match.Groups[5].Success ? Convert.ToInt32(match.Groups[5].Value, 10) : 0;
                    var second = match.Groups[6].Success ? Convert.ToInt32(match.Groups[6].Value, 10) : 0;

                    metadataDate = new DateTime(year, month, day, hour, minute, second);
                }
            }

            var dataFile = new DataFile(path, this.DataPath, parser.Content, parser.Metadata, parser.Queries);

            if (metadataDate.HasValue)
            {
                dataFile.SetTimes(metadataDate.Value);
            }

            return dataFile;
        }
    }
}
