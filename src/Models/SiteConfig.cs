using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TinySite.Models
{
    public class SiteConfig
    {
        public SiteConfig()
        {
            this.DefaultLayoutForExtension = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            this.IgnoreFiles = Enumerable.Empty<Regex>();

            this.Metadata = new MetadataCollection();
        }

        public Author Author { get; set; }

        public string SitePath { get; set; }

        public string DataPath { get; set; }

        public string DocumentsPath { get; set; }

        public string FilesPath { get; set; }

        public string LayoutsPath { get; set; }

        public string LiveReloadScript { get; set; }

        public string OutputPath { get; set; }

        public IEnumerable<AdditionalMetadataConfig> AdditionalMetadataForFiles { get; set; }

        public IEnumerable<Regex> IgnoreFiles { get; set; }

        public SiteConfig[] SubsiteConfigs { get; set; }

        public string Url { get; set; }

        public string RootUrl { get; set; }

        public SiteConfig Parent { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public IDictionary<string, string> DefaultLayoutForExtension { get; set; }

        public MetadataCollection Metadata { get; set; }
    }
}
