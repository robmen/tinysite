using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TinySite.Models
{
    public class LayoutFileCollection : KeyedCollection<string, LayoutFile>
    {
        public LayoutFileCollection(IEnumerable<LayoutFile> layouts)
        {
            foreach (var layout in layouts)
            {
                this.Add(layout);
            }
        }

        protected override string GetKeyForItem(LayoutFile item)
        {
            return item.Id;
        }
    }
}
