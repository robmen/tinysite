
namespace TinySite.Models
{
    public class DynamicRenderingStaticFile : DynamicRenderingOutputFile
    {
        public DynamicRenderingStaticFile(DocumentFile activeDocument, StaticFile file)
            : base(file)
        {
        }
    }
}