using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TinySite.Extensions;
using TinySite.Models;

namespace TinySite.Commands
{
    public class LoadDocumentsCommand
    {
        private static readonly Regex DateFromFileName = new Regex(@"^\s*(?<year>\d{4})-(?<month>\d{1,2})-(?<day>\d{1,2})([Tt@](?<hour>\d{1,2})\.(?<minute>\d{1,2})(\.(?<second>\d{1,2}))?)?[-\s]\s*", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
        private static readonly Regex OrderFromFileName = new Regex(@"^\s*(?<order>\d+)\.[-\s]\s*", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        public IEnumerable<AdditionalMetadataConfig> AdditionalMetadataForFiles { private get; set; }

        public IEnumerable<Regex> IgnoreFiles { private get; set; }

        public string DocumentsPath { private get; set; }

        public string OutputRootPath { private get; set; }

        public string RootUrl { private get; set; }

        public string ApplicationUrl { private get; set; }

        public Author Author { private get; set; }

        public IEnumerable<string> RenderedExtensions { private get; set; }

        public LayoutFileCollection Layouts { private get; set; }

        public IDictionary<string, string> DefaultLayoutForExtension { private get; set; }

        public IEnumerable<DocumentFile> Documents { get; private set; }

        public IEnumerable<DocumentFile> Execute()
        {
            return this.Documents = this.LoadDocuments().ToList();
        }

        private IEnumerable<DocumentFile> LoadDocuments()
        {
            if (Directory.Exists(this.DocumentsPath))
            {
                foreach (var path in Directory.GetFiles(this.DocumentsPath, "*", SearchOption.AllDirectories))
                {
                    if (this.IgnoreFiles != null && this.IgnoreFile(path))
                    {
                        continue;
                    }

                    yield return this.LoadDocument(path, this.RenderedExtensions, this.Layouts);
                }
            }
        }

        private DocumentFile LoadDocument(string file, IEnumerable<string> knownExtensions, LayoutFileCollection availableLayouts)
        {
            // Parse the document and update our document metadata.
            //
            var parser = new ParseDocumentCommand(file);
            parser.Execute();

            if (this.AdditionalMetadataForFiles != null)
            {
                var rootPath = Path.GetDirectoryName(this.DocumentsPath.TrimEnd('\\'));
                var relativePath = file.Substring(rootPath.Length + 1);

                foreach (var additionalMetadataConfig in this.AdditionalMetadataForFiles)
                {
                    if (additionalMetadataConfig.Match.IsMatch(relativePath))
                    {
                        parser.Metadata.AssignFrom(file, additionalMetadataConfig.Metadata);
                    }
                }
            }

            var metadataDate = parser.Date;

            var partial = false;

            var order = 0;

            var relativeDocumentPath = Path.GetFullPath(file).Substring(this.DocumentsPath.Length);

            var outputRelativePath = parser.Metadata.GetAndRemove("output", relativeDocumentPath);

            // The rest of this function is about calculating the correct
            // name for the file.
            //
            var fileName = Path.GetFileName(outputRelativePath);

            var outputRelativeFolder = Path.GetDirectoryName(outputRelativePath);

            // See if this file should be processed by any of the
            // rendering engines.
            //
            var extensionsForRendering = new List<string>();

            for (;;)
            {
                var extension = Path.GetExtension(fileName).TrimStart('.');
                if (extension.Equals("partial", StringComparison.OrdinalIgnoreCase))
                {
                    partial = true;
                    fileName = Path.GetFileNameWithoutExtension(fileName);
                }
                else if (knownExtensions.Contains(extension))
                {
                    extensionsForRendering.Add(extension);
                    fileName = Path.GetFileNameWithoutExtension(fileName);
                }
                else
                {
                    break;
                }
            }

            var targetExtension = Path.GetExtension(fileName).TrimStart('.');

            var layoutName = parser.Metadata.Get<string>("layout");

            var layouts = GetLayouts(layoutName, targetExtension, file);

            var disableDateFromFileName = parser.Metadata.GetAndRemove("DisableDateFromFileName", false);

            if (!disableDateFromFileName)
            {
                var match = DateFromFileName.Match(fileName);

                if (match.Success)
                {
                    // If the parser metadata didn't specify the date, use the date from the filename.
                    //
                    if (!metadataDate.HasValue)
                    {
                        var year = Convert.ToInt32(match.Groups[1].Value, 10);
                        var month = Convert.ToInt32(match.Groups[2].Value, 10);
                        var day = Convert.ToInt32(match.Groups[3].Value, 10);
                        var hour = match.Groups[4].Success ? Convert.ToInt32(match.Groups[4].Value, 10) : 0;
                        var minute = match.Groups[5].Success ? Convert.ToInt32(match.Groups[5].Value, 10) : 0;
                        var second = match.Groups[6].Success ? Convert.ToInt32(match.Groups[6].Value, 10) : 0;

                        metadataDate = new DateTime(year, month, day, hour, minute, second);
                    }

                    fileName = fileName.Substring(match.Length);
                }
            }

            var disableOrderFromFileName = parser.Metadata.GetAndRemove("DisableOrderFromFileName", false);

            if (!disableOrderFromFileName)
            {
                var match = OrderFromFileName.Match(fileName);

                if (match.Success)
                {
                    order = Convert.ToInt32(match.Groups[1].Value, 10);

                    fileName = fileName.Substring(match.Length);
                }
            }

            var parentId = String.IsNullOrEmpty(outputRelativeFolder) ? null : SanitizePath(outputRelativeFolder);

            var disableInsertDateIntoPath = parser.Metadata.GetAndRemove("DisableInsertDateIntoPath", false);

            if (!disableInsertDateIntoPath && metadataDate.HasValue)
            {
                outputRelativeFolder = Path.Combine(outputRelativeFolder, metadataDate.Value.Year.ToString(), metadataDate.Value.Month.ToString(), metadataDate.Value.Day.ToString());
            }

            if (!parser.Metadata.Contains("title"))
            {
                parser.Metadata.Add("title", Path.GetFileNameWithoutExtension(fileName));
            }

            // Sanitize the filename into a good URL.
            //
            var sanitized = SanitizeEntryId(fileName);

            if (!fileName.Equals(sanitized))
            {
                fileName = sanitized;
            }

            var disableSanitizePath = parser.Metadata.GetAndRemove("DisableSanitizePath", false);

            if (!disableSanitizePath)
            {
                outputRelativeFolder = SanitizePath(outputRelativeFolder);
            }

            var disableCleanUrls = parser.Metadata.GetAndRemove("DisableCleanUrls", false);

            if (!disableCleanUrls && !"index.html".Equals(fileName, StringComparison.OrdinalIgnoreCase) && ".html".Equals(Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase))
            {
                outputRelativeFolder = Path.Combine(outputRelativeFolder, Path.GetFileNameWithoutExtension(fileName)) + "\\";

                fileName = null;
            }

            var id = String.IsNullOrEmpty(fileName) ? outputRelativeFolder : Path.Combine(outputRelativeFolder, Path.GetFileNameWithoutExtension(fileName));

            var output = Path.Combine(outputRelativeFolder, fileName ?? "index.html");

            var relativeUrl = this.ApplicationUrl.EnsureEndsWith("/") + Path.Combine(outputRelativeFolder, fileName ?? String.Empty).Replace('\\', '/');

            // Finally create the document.
            //
            var documentFile = new DocumentFile(file, this.DocumentsPath, output, this.OutputRootPath, relativeUrl, this.RootUrl, this.Author, parser.Metadata, parser.Queries);

            documentFile.Partial = partial;

            if (metadataDate.HasValue)
            {
                documentFile.SetTimes(metadataDate.Value);
            }

            documentFile.Id = parser.Metadata.GetAndRemove("id", id.Trim('\\'));

            documentFile.ParentId = parser.Metadata.GetAndRemove("parent", parentId ?? String.Empty);

            documentFile.Draft = (parser.Draft || documentFile.Date > DateTime.Now);

            documentFile.ExtensionsForRendering = extensionsForRendering;

            documentFile.AssignLayouts(layouts);

            documentFile.Order = parser.Metadata.GetAndRemove("order", order);

            documentFile.PaginateQuery = parser.Metadata.GetAndRemove<string>("paginate");

            documentFile.SourceContent = parser.Content;

            return documentFile;
        }

        private IEnumerable<LayoutFile> GetLayouts(string layoutName, string targetExtension, string file)
        {
            if (String.IsNullOrEmpty(layoutName) && this.DefaultLayoutForExtension != null)
            {
                if (!this.DefaultLayoutForExtension.TryGetValue(targetExtension, out layoutName))
                {
                    this.DefaultLayoutForExtension.TryGetValue("*", out layoutName);
                }
            }

            while (!String.IsNullOrEmpty(layoutName))
            {
                if (!this.Layouts.Contains(layoutName))
                {
                    Console.Error.WriteLine("Cannot find layout: '{0}' while processing file: {1}", layoutName, Path.GetFullPath(file));

                    break;
                }

                var layout = this.Layouts[layoutName];

                yield return layout;

                file = layout.SourcePath;

                layoutName = layout.Layout;
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

        private static string SanitizeEntryId(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return String.Empty;
            }

            id = Regex.Replace(id, @"[^\w\s_\-\.]+", String.Empty); // first, allow only words, spaces, underscores, dashes and dots.
            id = Regex.Replace(id, @"\.{2,}", String.Empty); // strip out any dots stuck together (no pathing attempts).
            id = Regex.Replace(id, @"\s{2,}", " "); // convert multiple spaces into single space.
            id = id.Trim(' ', '.'); // ensure the string does not start or end with a dot
            return id.Replace(' ', '-').ToLowerInvariant(); // finally, replace all spaces with dashes and lowercase it.
        }

        private static string SanitizePath(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return String.Empty;
            }

            id = Regex.Replace(id, @"[^\w\-\\/]+", "-"); // first, allow only words, underscores, dashes, and path separators.
            return id.Trim('-').ToLowerInvariant(); // ensure the string does not start or end with dashes and lowercase it.
        }
    }
}
