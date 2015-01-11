using TinySite.Extensions;

namespace TinySite.Models
{
    public class BookPage
    {
        public DocumentFile Document { get; set; }

        public virtual dynamic GetAsDynamic(DocumentFile activeDocument)
        {
            dynamic data = new CaseInsenstiveExpando();

            data.Active = (this.Document == activeDocument);
            data.Document = this.Document.GetAsDynamic();
            data.Page = true;

            return data;
        }
    }
}
