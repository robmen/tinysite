using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TinySite.Models;

namespace TinySite.Commands
{
    public class LoadLayoutsCommand
    {
        public string LayoutsPath { private get; set; }

        public IEnumerable<LayoutFile> Layouts { get; private set; }

        public async Task<IEnumerable<LayoutFile>> ExecuteAsync()
        {
            var loadTasks = this.LoadLayoutsAsync();

            return this.Layouts = await Task.WhenAll(loadTasks);
        }

        private IEnumerable<Task<LayoutFile>> LoadLayoutsAsync()
        {
            if (Directory.Exists(this.LayoutsPath))
            {
                foreach (var path in Directory.GetFiles(this.LayoutsPath, "*", SearchOption.AllDirectories))
                {
                    yield return this.LoadLayoutAsync(path);
                }
            }
        }

        private async Task<LayoutFile> LoadLayoutAsync(string path)
        {
            var parser = new ParseDocumentCommand();
            parser.DocumentPath = path;
            await parser.ExecuteAsync();

            return new LayoutFile(path, this.LayoutsPath, parser.Content, parser.Metadata);
        }
    }
}
