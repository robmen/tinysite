using System;
using System.Collections.Generic;
using System.IO;
using TinySite.Extensions;

namespace TinySite.Models
{
    public abstract class SourceFile
    {
        protected SourceFile(string path, string rootPath)
        {
            var actualRootPath = Path.GetDirectoryName(rootPath.TrimEnd('\\'));

            var info = new FileInfo(path);

            this.Date = info.CreationTime;

            this.Modified = (info.LastWriteTime < info.CreationTime) ? info.CreationTime : info.LastWriteTime;

            this.FileName = Path.GetFileName(path);

            this.Name = Path.GetFileNameWithoutExtension(path);

            this.Extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();

            this.SourcePath = Path.GetFullPath(path);

            this.SourceRelativePath = this.SourcePath.Substring(actualRootPath.Length + 1);

            this.SourceRelativeFolder = Path.GetDirectoryName(this.SourceRelativePath);
        }

        protected SourceFile(SourceFile original)
        {
            this.Date = original.Date;
            this.FileName = original.FileName;
            this.Name = original.Name;
            this.Modified = original.Modified;
            this.Extension = original.Extension;
            this.SourcePath = original.SourcePath;
            this.SourceRelativeFolder = original.SourceRelativeFolder;
            this.SourceRelativePath = original.SourceRelativePath;
        }

        internal List<SourceFile> ContributingFiles { get; } = new List<SourceFile>();

        public DateTime Date { get; set; }

        public DateTime DateUtc => this.Date.ToUniversalTime();

        public string FriendlyDate => this.Date.ToString("D");

        public string StandardUtcDate => this.DateUtc.ToString("yyyy-MM-ddThh:mm:ssZ");

        public string FileName { get; set; }

        public string Name { get; set; }

        public DateTime Modified { get; set; }

        public string Extension { get; set; }

        public string SourcePath { get; set; }

        public string SourceRelativeFolder { get; set; }

        public string SourceRelativePath { get; set; }

        public SourceFile AddContributingFile(SourceFile contributor)
        {
            if (contributor != null && contributor != this)
            {
                this.ContributingFiles.Add(contributor);
            }

            return this;
        }

        public SourceFile AddContributingFiles(IEnumerable<SourceFile> contributors)
        {
            foreach (var file in contributors)
            {
                this.AddContributingFile(file);
            }

            return this;
        }

        public IEnumerable<SourceFile> AllContributingFiles()
        {
            var set = new HashSet<SourceFile>();

            var all = new Queue<SourceFile>(this.ContributingFiles);

            while (all.Count > 0)
            {
                var file = all.Dequeue();

                if (set.Add(file))
                {
                    all.EnqueueRange(file.ContributingFiles);
                }
            }

            return set;
        }

        public DateTime LatestModifiedOfContributingFiles()
        {
            var latest = this.Modified;

            foreach (var contributor in this.AllContributingFiles())
            {
                if (latest < contributor.Modified)
                {
                    latest = contributor.Modified;
                }
            }

            return latest;
        }
    }
}
