using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinySite.Models;

namespace TinySite.Commands
{
    internal class SetUnmodifiedCommand
    {
        public SetUnmodifiedCommand(string sitePath, IEnumerable<DocumentFile> documents, IEnumerable<StaticFile> files, IEnumerable<LastRunDocument> lastRunState)
        {
            this.Documents = documents;
            this.Files = files;
            this.LastRunState = lastRunState;
            this.SitePath = sitePath;
        }

        private IEnumerable<DocumentFile> Documents { get; }

        private IEnumerable<StaticFile> Files { get; }

        private IEnumerable<LastRunDocument> LastRunState { get; }

        private string SitePath { get; }

        public void Execute()
        {
            this.UpdateFilesUnmodifiedState();

            this.UpdateDocumentUnmodifiedState();
        }

        private void UpdateFilesUnmodifiedState()
        {
            foreach (var file in this.Files)
            {
                var outputModified = this.GetModifiedDateTime(file.OutputPath);

                file.Unmodified = (outputModified.HasValue && outputModified.Value == file.Modified);
            }
        }

        private void UpdateDocumentUnmodifiedState()
        {
            var existingTimes = new Dictionary<string, DateTime>();

            var documentsByPath = this.Documents.ToDictionary(d => d.SourceRelativePath);

            foreach (var lastRunDoc in this.LastRunState)
            {
                DocumentFile doc;

                if (documentsByPath.TryGetValue(lastRunDoc.Path, out doc))
                {
                    var modified = false;

                    var outputModified = this.GetModifiedDateTime(doc.OutputPath);

                    if (outputModified.HasValue)
                    {
                        if (outputModified.Value < doc.Modified)
                        {
                            modified = true;
                        }
                        else if (lastRunDoc.Contributors != null)
                        {
                            foreach (var contributor in lastRunDoc.Contributors)
                            {
                                DateTime contributorModified;

                                if (!existingTimes.TryGetValue(contributor.Path, out contributorModified))
                                {
                                    var contributorPath = Path.Combine(this.SitePath, contributor.Path);

                                    var contributorFile = new FileInfo(contributorPath);

                                    contributorModified = (contributorFile.LastWriteTime < contributorFile.CreationTime) ? contributorFile.CreationTime : contributorFile.LastWriteTime;

                                    existingTimes.Add(contributor.Path, contributorModified);
                                }

                                if (outputModified < contributorModified)
                                {
                                    modified = true;
                                    break;
                                }
                            }
                        }

                        if (!modified)
                        {
                            doc.Unmodified = true;
                        }
                    }
                }
            }
        }

        private DateTime? GetModifiedDateTime(string path)
        {
            var fileInfo = new FileInfo(path);

            if (fileInfo.Exists)
            {
                return (fileInfo.LastWriteTime < fileInfo.CreationTime) ? fileInfo.CreationTime : fileInfo.LastWriteTime;
            }

            return null;
        }
    }
}