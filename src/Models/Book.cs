using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TinySite.Extensions;

namespace TinySite.Models
{
    [DebuggerDisplay("Book: {Id}")]
    public class Book
    {
        public string Id { get; set; }

        public IEnumerable<BookChapter> Chapters { get; set; }

        public dynamic GetAsDynamic(DocumentFile activeDocument)
        {
            var data = new CaseInsensitiveExpando();

            var chapters = this.Chapters.Select(c => c.GetAsDynamic(activeDocument)).ToList();

            data.Add("Id", this.Id);
            data.Add("Chapters", chapters);

            return data;
        }
    }
}
