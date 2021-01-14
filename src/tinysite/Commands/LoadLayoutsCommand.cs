using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TinySite.Models;

namespace TinySite.Commands
{
    public class LoadLayoutsCommand
    {
        public LoadLayoutsCommand(string layoutsPath, IEnumerable<AdditionalMetadataConfig> additionalMetadata, IEnumerable<Regex> ignoreFiles)
        {
            this.LayoutsPath = layoutsPath;
            this.AdditionalMetadataForFiles = additionalMetadata;
            this.IgnoreFiles = ignoreFiles;
        }

        public IEnumerable<LayoutFile> Layouts { get; private set; }

        private string LayoutsPath { get; }

        private IEnumerable<AdditionalMetadataConfig> AdditionalMetadataForFiles { get; }

        private IEnumerable<Regex> IgnoreFiles { get; }

        public IEnumerable<LayoutFile> Execute()
        {
            var loadTasks = this.LoadLayouts();

            return this.Layouts = loadTasks.ToList();
        }

        private IEnumerable<LayoutFile> LoadLayouts()
        {
            if (Directory.Exists(this.LayoutsPath))
            {
                foreach (var path in Directory.GetFiles(this.LayoutsPath, "*", SearchOption.AllDirectories))
                {
                    if (this.IgnoreFiles != null && this.IgnoreFile(path))
                    {
                        continue;
                    }

                    yield return this.LoadLayout(path);
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

        private LayoutFile LoadLayout(string path)
        {
            var parser = new ParseDocumentCommand(path);
            parser.Execute();

            if (this.AdditionalMetadataForFiles != null)
            {
                var rootPath = Path.GetDirectoryName(this.LayoutsPath.TrimEnd('\\'));
                var relativePath = path.Substring(rootPath.Length + 1);

                foreach (var additionalMetadataConfig in this.AdditionalMetadataForFiles)
                {
                    if (additionalMetadataConfig.Match.IsMatch(relativePath))
                    {
                        parser.Metadata.AssignFrom(path, additionalMetadataConfig.Metadata);
                    }
                }
            }

            return new LayoutFile(path, this.LayoutsPath, parser.Content, parser.Metadata, parser.Queries);
        }
    }
}
