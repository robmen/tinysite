namespace TinySite.Models.Dynamic
{
    public class DynamicStaticFile : DynamicOutputFile
    {
        public DynamicStaticFile(DocumentFile activeDocument, StaticFile file)
            : base(file)
        {
        }
    }
}