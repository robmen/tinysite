using System;
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

        public int Execute()
        {
#if DEBUG
            var duplicates = this.Documents.ToLookup(d => d.OutputPath, StringComparer.OrdinalIgnoreCase);

            foreach (var dupe in duplicates.Where(d => d.Count() > 1))
            {
                foreach (var d in dupe)
                {
                    Console.WriteLine("Duplicate, output: {0}, source: {1}", d.OutputPath, d.SourcePath);
                }
            }
#endif
            return this.WroteDocuments = this.Documents.Where(d => d.Rendered)
                .AsParallel()
                .Select(WriteDocument)
                .Count();
        }

        public async Task<int> ExecuteAsync()
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

            return this.WroteDocuments;
        }

        private static DocumentFile WriteDocument(DocumentFile document)
        {
            var folder = Path.GetDirectoryName(document.OutputPath);

            Directory.CreateDirectory(folder);

            var utf8 = Encoding.UTF8.GetBytes(document.RenderedContent);

            using (var writer = File.Open(document.OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete))
            {
                writer.Write(utf8, 0, utf8.Length);
            }

            return document;
        }
    }
}
