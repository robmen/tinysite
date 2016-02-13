using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TinySite.Models;

namespace TinySite.Commands
{
    public class WriteDocumentsCommand
    {
        public WriteDocumentsCommand(IEnumerable<DocumentFile> documents)
        {
            this.Documents = documents;
        }

        public int WroteDocuments { get; private set; }

        private IEnumerable<DocumentFile> Documents { get; }

        public int Execute()
        {
            var duplicates = this.Documents.ToLookup(d => d.OutputPath, StringComparer.OrdinalIgnoreCase);

            foreach (var dupe in duplicates.Where(d => d.Count() > 1))
            {
                foreach (var d in dupe)
                {
                    Console.Error.WriteLine("Duplicate, output: {0}, source: {1}", d.OutputPath, d.SourcePath);

                    // Do not render duplicates so it is clear there is something wrong. Also, given documents
                    // are written in parallel there is a reasonable chance the duplicates will be written at
                    // the same time throwing an access denied exception.
                    //
                    d.Rendered = false;
                }
            }

            return this.WroteDocuments = this.Documents
                .Where(d => d.Rendered)
                .AsParallel()
                .Select(WriteDocument)
                .Count();
        }

#if false
        public async Task<int> Execute()
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
#endif

        private static DocumentFile WriteDocument(DocumentFile document)
        {
            var folder = Path.GetDirectoryName(document.OutputPath);

            Directory.CreateDirectory(folder);

            var utf8 = Encoding.UTF8.GetBytes(document.RenderedContent);

            using (var writer = File.Open(document.OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete))
            {
                writer.Write(utf8, 0, utf8.Length);
            }

            var modified = document.LatestModifiedOfContributingFiles();

            File.SetCreationTime(document.OutputPath, document.Date);

            File.SetLastWriteTime(document.OutputPath, modified);

            return document;
        }
    }
}
