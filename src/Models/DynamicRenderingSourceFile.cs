using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public abstract class DynamicRenderingSourceFile : DynamicRenderingObject
    {
        protected DynamicRenderingSourceFile(SourceFile file)
            : base(file.SourceRelativePath)
        {
            this.File = file;
        }

        private SourceFile File { get; }

        protected override IDictionary<string, object> GetData()
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(this.File.Date), this.File.Date },
                { nameof(this.File.DateUtc), this.File.DateUtc },
                { nameof(this.File.FriendlyDate), this.File.FriendlyDate },
                { nameof(this.File.StandardUtcDate), this.File.StandardUtcDate },
                { nameof(this.File.Modified), this.File.Modified},
                { nameof(this.File.Name), this.File.Name},
                { nameof(this.File.Extension), this.File.Extension },
                { nameof(this.File.SourcePath), this.File.SourcePath },
                { nameof(this.File.SourceRelativePath), this.File.SourceRelativePath },
            };
        }
    }
}