using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TinySite.Models
{
    public class AdditionalMetadataConfig
    {
        public AdditionalMetadataConfig(Regex match, IDictionary<string, object> metadata)
        {
            this.Match = match;
            this.Metadata = metadata;
        }

        public Regex Match { get; }

        public IDictionary<string, object> Metadata { get; }
    }
}
