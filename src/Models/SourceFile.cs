using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinySite.Extensions;

namespace TinySite.Models
{
    public abstract class SourceFile : CaseInsensitiveExpando
    {
        public SourceFile()
        {
        }

        public SourceFile(string path, string rootPath)
        {
            var actualRootPath = Path.GetDirectoryName(rootPath.TrimEnd('\\'));

            var info = new FileInfo(path);

            this.Date = info.CreationTime;

            this.Modified = (info.LastWriteTime < info.CreationTime) ? info.CreationTime : info.LastWriteTime;

            this.Name = Path.GetFileName(path);

            this.Extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();

            this.SourcePath = Path.GetFullPath(path);

            this.SourceRelativePath = this.SourcePath.Substring(actualRootPath.Length + 1);
        }

        protected SourceFile(SourceFile original) :
            base(original)
        {
        }

        internal List<SourceFile> ContributingFiles { get; } = new List<SourceFile>();

        public DateTime Date { get { return this.Get<DateTime>(); } set { this.SetTimes(null, value); } }

        public DateTime Modified { get { return this.Get<DateTime>(); } set { this.Set<DateTime>(value); } }

        public string Name { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string Extension { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string SourcePath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string SourceRelativePath { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public SourceFile AddContributingFile(SourceFile contributor)
        {
            if (contributor != this)
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

        protected void SetTimes(string prefix, DateTime time)
        {
            this.Set<DateTime>(time, prefix ?? "Date");
            this.Set<DateTime>(time.ToUniversalTime(), prefix ?? "Date" + "Utc");
            this.Set<string>(time.ToString("D"), prefix + "FriendlyDate");
            this.Set<string>(time.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ"), prefix + "StandardUtcDate");
        }
    }
}
