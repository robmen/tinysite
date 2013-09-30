using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    public class RenderCommand
    {
        public IDictionary<string, RenderingEngine> Engines { private get; set; }

        public Site Site { private get; set; }

        public void Execute()
        {
            using (var tx = new RenderingTransaction(this.Engines, this.Site))
            {
                this.RenderDocuments();

                this.WriteDocumentsToDisk();

                this.CopyStaticFiles();
            }
        }

        private void RenderDocuments()
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.RenderDocuments))
            {
                // TODO: Eventually skip documents that are up to date and don't need to be rendered again.

                // TODO: do not enable parallel render until we sort out how to get the document summary
                //       always set correctly when rendering in parallel.
                //Statistics.Current.RenderedDocuments = this.Site.Documents
                //    .Where(d => !d.Draft)
                //    .AsParallel()
                //    .Select(
                //        document =>
                //        {
                //            document.RenderDocument();

                //            return document;
                //        })
                //    .Count();

                foreach (var document in this.Site.Documents.Where(d => !d.Draft))
                {
                    document.RenderDocument();

                    ++Statistics.Current.RenderedDocuments;
                }
            }
        }

        private void WriteDocumentsToDisk()
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.WriteDocuments))
            {
                Statistics.Current.WroteDocuments = this.Site.Documents
                    .Where(d => !d.Draft && d.Rendered)
                    .AsParallel()
                    .Select(
                        document =>
                        {
                            var folder = Path.GetDirectoryName(document.OutputPath);
                            Directory.CreateDirectory(folder);

                            using (var writer = File.Open(document.OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete))
                            {
                                var utf8 = Encoding.UTF8.GetBytes(document.Content);
                                writer.Write(utf8, 0, utf8.Length);
                            }

                            return document;
                        })
                    .Count();

                //foreach (var document in this.Site.Documents.Where(d => !d.Draft && d.Rendered))
                //{
                //    var folder = Path.GetDirectoryName(document.OutputPath);
                //    Directory.CreateDirectory(folder);

                //    using (var writer = File.Open(document.OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete))
                //    {
                //        var utf8 = Encoding.UTF8.GetBytes(document.Content);
                //        writer.Write(utf8, 0, utf8.Length);

                //        ++Statistics.Current.WroteDocuments;
                //    }
                //}
            }
        }

        private void CopyStaticFiles()
        {
            using (var capture = Statistics.Current.Start(StatisticTiming.CopyStaticFiles))
            {
                Statistics.Current.CopiedFiles = this.Site.Files
                    .AsParallel()
                    .Select(
                        file =>
                        {
                            var folder = Path.GetDirectoryName(file.OutputPath);
                            Directory.CreateDirectory(folder);

                            File.Copy(file.SourcePath, file.OutputPath, true);

                            return file;
                        })
                    .Count();

                //foreach (var file in this.Site.Files)
                //{
                //    var folder = Path.GetDirectoryName(file.OutputPath);
                //    Directory.CreateDirectory(folder);

                //    File.Copy(file.SourcePath, file.OutputPath, true);
                //    ++Statistics.Current.CopiedFiles;
                //}
            }
        }
    }
}
