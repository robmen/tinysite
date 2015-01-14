using TinySite.Extensions;

namespace TinySite.Models
{
    public class BookPage
    {
        public DocumentFile Document { get; set; }

        public virtual dynamic GetAsDynamic(DocumentFile activeDocument)
        {
            var data = new CaseInsensitiveExpando();

            data.Add("Active", (this.Document == activeDocument));
            data.Add("Document", this.Document.GetAsDynamic());
            data.Add("Page", true);

            return data;
        }
    }
}
