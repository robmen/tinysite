using TinySite.Extensions;

namespace TinySite.Models
{
    public class BookPage
    {
        public DocumentFile Document { get; set; }

        public virtual dynamic GetAsDynamic(DocumentFile activeDocument)
        {
            dynamic data = new CaseInsenstiveExpando();

            data.Document = this.Document.GetAsDynamic();
            data.Active = (this.Document == activeDocument);

            return data;
        }
    }
}
