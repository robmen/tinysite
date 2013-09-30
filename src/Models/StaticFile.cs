
namespace TinySite.Models
{
    public class StaticFile : OutputFile
    {
        public StaticFile(string path, string rootPath, string outputRootPath, string url, string rootUrl)
            : base(path, rootPath, outputRootPath, url, rootUrl)
        {
        }

        //public DateTime CreatedAt { get; set; }

        //public DateTime LastModified { get; set; }

        //public string OutputPath { get; set; }

        //public string RelativePath { get; set; }

        //public string SourcePath { get; set; }

        //public string Url { get; set; }

        //public static StaticFile Create(string rootPath, string output, string rootUrl, string path)
        //{
        //    path = Extension.GetFullPath(path);

        //    var relativePath = path.Substring(rootPath.Length);

        //    var url = rootUrl.EnsureEndsWith("/") + relativePath.Replace('\\', '/');

        //    var info = new FileInfo(path);

        //    return new StaticFile
        //    {
        //        CreatedAt = info.CreationTime,
        //        LastModified = info.LastWriteTime,
        //        OutputPath = Extension.Combine(output, relativePath),
        //        RelativePath = relativePath,
        //        SourcePath = path,
        //        Url = url,
        //    };
        //}
    }
}
