using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TinySite.Models
{
    [DebuggerDisplay("LastRunDocument: {Path}, Modified: {Modified}")]
    public class LastRunDocument
    {
        public LastRunDocument()
        {
        }

        public LastRunDocument(string sourceRelativePath, DateTime modified, IEnumerable<LastRunContributingFile> contributors)
        {
            this.Path = sourceRelativePath;
            this.Modified = modified;
            this.Contributors = contributors.ToArray();
        }

        public string Path { get; set; }

        public DateTime Modified { get; set; }

        public LastRunContributingFile[] Contributors { get; set; }
    }
}
