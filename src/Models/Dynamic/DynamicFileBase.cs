using System;

namespace TinySite.Models.Dynamic
{
    public abstract class DynamicFileBase : DynamicBase
    {
        protected DynamicFileBase(string sourceRelativePath, MetadataCollection persistedMetadata = null)
        {
            this.SourceRelativePath = sourceRelativePath;

            this.PersistedMetadata = persistedMetadata;
        }

        private MetadataCollection PersistedMetadata { get; }

        private string SourceRelativePath { get; }

        protected override bool TrySetValue(string key, object value)
        {
            if (base.TrySetValue(key, value))
            {
                try
                {
                    this.PersistedMetadata?.Add(key, value);
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            return false;
        }

        public override void Add(string key, object value)
        {
            if (!this.TrySetValue(key, value))
            {
                Console.WriteLine("Document metadata in: {0} cannot overwrite built in or existing metadata: \"{1}\" with value: \"{2}\"", this.SourceRelativePath, key, value);
            }
        }
    }
}
