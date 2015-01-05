using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinySite.Models;

namespace TinySite.Commands
{
    public class WriteDocumentsCommand
    {
        public IEnumerable<DocumentFile> Documents { private get; set; }

        public int WroteDocuments { get; private set; }

        public async Task ExecuteAsync()
        {
            var streams = new List<Stream>();

            var tasks = new List<Task>();

            try
            {
                foreach (var document in this.Documents.Where(d => !d.Draft && d.Rendered))
                {
                    var folder = Path.GetDirectoryName(document.OutputPath);

                    Directory.CreateDirectory(folder);

                    var utf8 = Encoding.UTF8.GetBytes(document.RenderedContent);

                    var writer = File.Open(document.OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete);

                    streams.Add(writer);

                    var task = writer.WriteAsync(utf8, 0, utf8.Length);

                    tasks.Add(task);

                    ++this.WroteDocuments;
                }

                await Task.WhenAll(tasks);
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
