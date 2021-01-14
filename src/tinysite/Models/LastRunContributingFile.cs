using System;

namespace TinySite.Models
{
    public class LastRunContributingFile
    {
        public LastRunContributingFile(string sourceRelativePath, DateTime modified)
        {
            this.Path = sourceRelativePath;
            this.Modified = modified;
        }

        public string Path { get; set; }

        public DateTime Modified { get; set; }
    }
}