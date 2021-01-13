using System;
using System.Collections.Generic;

namespace TinySite.Models.Dynamic
{
    public class DynamicRenderData : DynamicFileBase
    {
        // These are fields because as properties their typical name (e.g. "Site" 
        // for "_site") would hide the dynamic properties we are trying to expose
        // via the dictionary created by GetData().
        //
        private readonly DataFile _data;
        private readonly Site _site;

        public DynamicRenderData(DataFile dataFile, Site site)
            : base(dataFile.SourceRelativePath)
        {
            _data = dataFile;
            _site = site;
        }

        protected override IDictionary<string, object> GetData()
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "Document", new Lazy<object>(this.GetDataFile) }, // "Document" is a synonmym for "Data".
                { "Data", new Lazy<object>(this.GetDataFile) },
                { "Site", new Lazy<object>(this.GetSite) },
            };
        }

        private object GetDataFile()
        {
            return new DynamicDataFile(null, _data, _site);
        }

        private object GetSite()
        {
            return new DynamicSite(null, _site);
        }
    }
}
