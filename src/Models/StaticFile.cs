using System.Diagnostics;

namespace TinySite.Models
{
    [DebuggerDisplay("StaticFile: {SourceRelativePath}")]
    public class StaticFile : OutputFile
    {
        public StaticFile(string path, string rootPath, string outputRootPath, string url, string rootUrl)
            : base(path, rootPath, null, outputRootPath, rootUrl, url)
        {
        }
    }
}
