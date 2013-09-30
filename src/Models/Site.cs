using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class Site
    {
        private Site()
        {
        }

        public Author Author { get; set; }

        public string DocumentsPath { get; set; }

        public string FilesPath { get; set; }

        public string LayoutsPath { get; set; }

        public string OutputPath { get; set; }

        public Site Parent { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public string Url { get; set; }

        public string RootUrl { get; set; }

        public IList<DocumentFile> Documents { get; set; }

        public IList<StaticFile> Files { get; set; }

        public LayoutFileCollection Layouts { get; set; }

        public MetadataCollection Metadata { get; set; }

        public dynamic GetAsDynamic()
        {
            dynamic data = new CaseInsenstiveExpando();

            this.Metadata.Assign(data as IDictionary<string, object>);

            data.Author = this.Author;
            data.Output = this.OutputPath;
            data.Url = this.Url;
            data.RootUrl = this.RootUrl;
            data.FullUrl = this.RootUrl.EnsureEndsWith("/") + this.Url.TrimStart('/');
            data.Parent = this.Parent;
            data.TimeZoneInfo = this.TimeZone;
            return data;
        }

        public static Site Load(SiteConfig config, IEnumerable<string> renderedExtensions, Site parent = null)
        {
            var site = new Site();
            site.Author = config.Author;
            site.DocumentsPath = config.DocumentsPath;
            site.FilesPath = config.FilesPath;
            site.LayoutsPath = config.LayoutsPath;
            site.OutputPath = config.OutputPath;
            site.Parent = parent;
            site.TimeZone = config.TimeZone;
            site.Url = config.Url.EnsureEndsWith("/");
            site.RootUrl = config.RootUrl.EnsureEndsWith("/");
            site.Metadata = new MetadataCollection(config.Metadata);

            site.Documents = LoadDocuments(site.DocumentsPath, site.OutputPath, site.Url, site.RootUrl, site.Author, renderedExtensions).ToList();
            site.Files = LoadFiles(site.FilesPath, site.OutputPath, site.Url, site.RootUrl).ToList();

            var layoutList = LoadLayouts(site.LayoutsPath);
            site.Layouts = LayoutFileCollection.Create(layoutList);

            return site;
        }

        private static IEnumerable<DocumentFile> LoadDocuments(string rootPath, string outputPath, string url, string rootUrl, Author author, IEnumerable<string> renderedExtensions)
        {
            if (!Directory.Exists(rootPath))
            {
                return Enumerable.Empty<DocumentFile>();
            }

            return Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories)
                .AsParallel()
                .Select(
                    file =>
                    {
                        var documentFile = new DocumentFile(file, rootPath, outputPath, url, rootUrl, author);

                        documentFile.Load(LoadDocumentFlags.DateFromFileName | LoadDocumentFlags.DateInPath | LoadDocumentFlags.CleanUrls, renderedExtensions);

                        return documentFile;
                    });

            //foreach (var file in Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories))
            //{
            //    var documentFile = new DocumentFile(file, rootPath, outputPath, url, rootUrl, author);

            //    documentFile.Load(LoadDocumentFlags.DateFromFileName | LoadDocumentFlags.DateInPath | LoadDocumentFlags.CleanUrls, renderedExtensions);

            //    yield return documentFile;
            //}
        }

        private static IEnumerable<StaticFile> LoadFiles(string rootPath, string outputPath, string url, string rootUrl)
        {
            if (!Directory.Exists(rootPath))
            {
                return Enumerable.Empty<StaticFile>();
            }

            return Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories)
                .AsParallel()
                .Select(file => new StaticFile(file, rootPath, outputPath, url, rootUrl));

            //if (Directory.Exists(rootPath))
            //{
            //    foreach (var file in Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories))
            //    {
            //        var staticFile = new StaticFile(file, rootPath, outputPath, url, rootUrl);

            //        yield return staticFile;
            //    }
            //}
        }

        private static IEnumerable<LayoutFile> LoadLayouts(string rootPath)
        {
            if (!Directory.Exists(rootPath))
            {
                return Enumerable.Empty<LayoutFile>();
            }

            return Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories)
                .AsParallel()
                .Select(
                    file =>
                    {
                        var layoutFile = new LayoutFile(file, rootPath);

                        layoutFile.Load();

                        return layoutFile;
                    });

            //if (Directory.Exists(rootPath))
            //{
            //    foreach (var file in Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories))
            //    {
            //        var layoutFile = new LayoutFile(file, rootPath);

            //        layoutFile.Load();

            //        yield return layoutFile;
            //    }
            //}
        }
    }
}
