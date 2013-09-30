using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TinySite.Models
{
    public class LayoutFileCollection : KeyedCollection<string, LayoutFile>
    {
        public static LayoutFileCollection Create(IEnumerable<LayoutFile> layouts)
        {
            var collection = new LayoutFileCollection();

            foreach (var layout in layouts)
            {
                collection.Add(layout);
            }

            return collection;
        }

        protected override string GetKeyForItem(LayoutFile item)
        {
            return item.Id;
        }
    }
}
