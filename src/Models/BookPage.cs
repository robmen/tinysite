using TinySite.Extensions;

namespace TinySite.Models
{
    public class BookPage
    {
        public DocumentFile Document { get; set; }

        public virtual dynamic GetAsDynamic(DocumentFile activeDocument)
        {
            var data = new CaseInsensitiveExpando();

            data["Active"] = (this.Document == activeDocument);
            data["Document"] = this.Document.GetAsDynamic();

            data["Chapter"] = false;
            data["Page"] = true;

            data["ChildActive"] = false;
            data["Children"] = null;

            return data;
        }
    }
}
