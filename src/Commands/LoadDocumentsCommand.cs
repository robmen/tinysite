using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TinySite.Models;

namespace TinySite.Commands
{
    public class LoadDocumentsCommand
    {
        private static readonly Regex DateFromFileName = new Regex(@"^\s*(?<year>\d{4})-(?<month>\d{1,2})-(?<day>\d{1,2})([Tt@](?<hour>\d{1,2})\.(?<minute>\d{1,2})(\.(?<second>\d{1,2}))?)?[-\s]\s*", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        public string DocumentsPath { private get; set; }

        public string OutputPath { private get; set; }

        public string Url { private get; set; }

        public string RootUrl { private get; set; }

        public Author Author { private get; set; }

        public IEnumerable<string> RenderedExtensions { private get; set; }

        public IEnumerable<DocumentFile> Documents { get; private set; }

        public async Task<IEnumerable<DocumentFile>> ExecuteAsync()
        {
            var loadTasks = this.LoadDocumentsAsync();

            return this.Documents = await Task.WhenAll(loadTasks);
        }

        private IEnumerable<Task<DocumentFile>> LoadDocumentsAsync()
        {
            if (Directory.Exists(this.DocumentsPath))
            {
                foreach (var path in Directory.GetFiles(this.DocumentsPath, "*", SearchOption.AllDirectories))
                {
                    yield return this.LoadDocumentAsync(path, LoadDocumentFlags.DateFromFileName | LoadDocumentFlags.DateInPath | LoadDocumentFlags.CleanUrls, this.RenderedExtensions);
                }
            }
        }

        private async Task<DocumentFile> LoadDocumentAsync(string file, LoadDocumentFlags flags, IEnumerable<string> knownExtensions)
        {
            // Parse the document and update our document metadata.
            //
            var parser = new ParseDocumentCommand();
            parser.DocumentPath = file;
            parser.SummaryMarker = "\r\n\r\n===";
            await parser.ExecuteAsync();

            var documentFile = new DocumentFile(file, this.DocumentsPath, this.OutputPath, this.Url, this.RootUrl, this.Author);

            documentFile.Content = parser.Content;

            if (parser.Date.HasValue)
            {
                documentFile.Date = parser.Date.Value;
            }

            documentFile.Draft = (parser.Draft || documentFile.Date > DateTime.Now);

            documentFile.Paginate = parser.Metadata.Get<int>("paginate", 0);
            parser.Metadata.Remove("paginate");

            string output;
            if (parser.Metadata.TryGet<string>("output", out output))
            {
                this.SetOutputPaths(documentFile, output);
                parser.Metadata.Remove("output");
            }

            documentFile.Metadata = parser.Metadata;

            // The rest of this function is about calculating the correct
            // name for the file.
            //
            var fileName = Path.GetFileName(documentFile.RelativePath);

            string updateFileName = null;

            string updateInPath = String.Empty;

            // See if this file should be processed by any of the
            // rendering engines.
            //
            documentFile.ExtensionsForRendering = new List<string>();

            for (; ; )
            {
                var extension = Path.GetExtension(fileName).TrimStart('.');
                if (knownExtensions.Contains(extension))
                {
                    documentFile.ExtensionsForRendering.Add(extension);
                    fileName = Path.GetFileNameWithoutExtension(fileName);

                    updateFileName = fileName;
                }
                else
                {
                    break;
                }
            }

            if (LoadDocumentFlags.DateFromFileName == (flags & LoadDocumentFlags.DateFromFileName))
            {
                var match = DateFromFileName.Match(fileName);

                if (match.Success)
                {
                    var year = Convert.ToInt32(match.Groups[1].Value, 10);
                    var month = Convert.ToInt32(match.Groups[2].Value, 10);
                    var day = Convert.ToInt32(match.Groups[3].Value, 10);
                    var hour = match.Groups[4].Success ? Convert.ToInt32(match.Groups[4].Value, 10) : 0;
                    var minute = match.Groups[5].Success ? Convert.ToInt32(match.Groups[5].Value, 10) : 0;
                    var second = match.Groups[6].Success ? Convert.ToInt32(match.Groups[6].Value, 10) : 0;

                    // If the parser didn't override the date, use the date from the filename.
                    //
                    if (!parser.Date.HasValue)
                    {
                        documentFile.Date = new DateTime(year, month, day, hour, minute, second);
                    }

                    fileName = fileName.Substring(match.Length);

                    updateFileName = fileName;
                }
            }

            if (LoadDocumentFlags.DateInPath == (flags & LoadDocumentFlags.DateInPath) && documentFile.Date != DateTime.MinValue)
            {
                updateInPath = String.Join("\\", documentFile.Date.Year, documentFile.Date.Month, documentFile.Date.Day);
            }

            if (!documentFile.Metadata.Contains("title"))
            {
                documentFile.Metadata.Add("title", Path.GetFileNameWithoutExtension(fileName));
            }

            // Sanitize the filename into a good URL.
            //
            var sanitized = SanitizeEntryId(fileName);

            if (!fileName.Equals(sanitized))
            {
                fileName = sanitized;

                updateFileName = fileName;
            }

            if (LoadDocumentFlags.CleanUrls == (flags & LoadDocumentFlags.CleanUrls) && !"index.html".Equals(fileName, StringComparison.OrdinalIgnoreCase) && ".html".Equals(Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase))
            {
                updateInPath = Path.Combine(updateInPath, Path.GetFileNameWithoutExtension(fileName));

                fileName = "index.html";

                updateFileName = fileName;
            }

            // If the name or path was updated, update the appropriately parts of the document.
            //
            if (!String.IsNullOrEmpty(updateFileName) || !String.IsNullOrEmpty(updateInPath))
            {
                documentFile.UpdateOutputPaths(updateInPath, updateFileName);
            }

            return documentFile;
        }

        private void SetOutputPaths(DocumentFile documentFile, string output)
        {
            var path = output.Replace('/', '\\');

            var url = output.Replace('\\', '/');

            documentFile.RelativePath = path;

            documentFile.OutputPath = Path.Combine(documentFile.OutputRootPath, path);

            documentFile.Url = String.Concat(documentFile.RootUrl, url);
        }

        private static string SanitizeEntryId(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            id = Regex.Replace(id, @"[^\w\s_\-\.]+", String.Empty); // first, allow only words, spaces, underscores, dashes and dots.
            id = Regex.Replace(id, @"\.{2,}", String.Empty); // strip out any dots stuck together (no pathing attempts).
            id = Regex.Replace(id, @"\s{2,}", " "); // convert multiple spaces into single space.
            id = id.Trim(new[] { ' ', '.' }); // ensure the string does not start or end with a dot
            return id.Replace(' ', '-').ToLowerInvariant(); // finally, replace all spaces with dashes and lowercase it.
        }
    }
}
