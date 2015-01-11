using System.Linq;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class BookChapter : BookPage
    {
        public BookChapter()
        {
            this.PagesOrSubChapters = new List<BookPage>();
        }

        public IList<BookPage> PagesOrSubChapters { get; private set; }

        public override dynamic GetAsDynamic(DocumentFile activeDocument)
        {
            var data = base.GetAsDynamic(activeDocument);

            //var children = new List<dynamic>();
            //var childActive = false;

            //foreach (var pageOrSubChapter in this.PagesOrSubChapters)
            //{
            //    var child = pageOrSubChapter.GetAsDynamic(activeDocument);

            //    children.Add(child);
            //    childActive |= child.Active;
            //}

            //data.Children = children;
            //data.ChildActive = childActive;

            var children = this.PagesOrSubChapters.Select(p => p.GetAsDynamic(activeDocument)).ToList();

            data.Children = children;
            data.ChildActive = children.Any(c => c.Active);

            return data;
        }
    }
}
