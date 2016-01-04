using System;
using System.Collections.Generic;

namespace TinySite.Models.Dynamic
{
    public abstract class DynamicSourceFile : DynamicBase
    {
        private SourceFile _sourceFile;

        protected DynamicSourceFile(SourceFile file, MetadataCollection persistedMetadata = null)
            : base(file.SourceRelativePath, persistedMetadata)
        {
            _sourceFile = file;
        }

        protected override IDictionary<string, object> GetData()
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(_sourceFile.Date), _sourceFile.Date },
                { nameof(_sourceFile.DateUtc), _sourceFile.DateUtc },
                { nameof(_sourceFile.FriendlyDate), _sourceFile.FriendlyDate },
                { nameof(_sourceFile.StandardUtcDate), _sourceFile.StandardUtcDate },
                { nameof(_sourceFile.Modified), _sourceFile.Modified},
                { nameof(_sourceFile.FileName), _sourceFile.FileName},
                { nameof(_sourceFile.Name), _sourceFile.Name},
                { nameof(_sourceFile.Extension), _sourceFile.Extension },
                { nameof(_sourceFile.SourcePath), _sourceFile.SourcePath },
                { nameof(_sourceFile.SourceRelativeFolder), _sourceFile.SourceRelativeFolder },
                { nameof(_sourceFile.SourceRelativePath), _sourceFile.SourceRelativePath }
            };
        }
    }
}