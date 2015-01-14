using System.Collections.Generic;
using System.Linq;
using TinySite.Extensions;

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
            var data = base.GetAsDynamic(activeDocument) as CaseInsensitiveExpando;

            data.Add("Chapter", true);
            data["Page"] = false;

            var children = this.PagesOrSubChapters.Select(p => p.GetAsDynamic(activeDocument)).ToList();
            var childActive = this.PagesOrSubChapters.Any(c => c.Document == activeDocument);

            data.Add("ChildActive", childActive);
            data.Add("Children", children);

            return data;
        }
    }
}
