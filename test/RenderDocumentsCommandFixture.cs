using System;
using System.IO;
using System.Linq;
using TinySite.Commands;
using TinySite.Models;
using TinySite.Renderers;
using TinySite.Services;
using Xunit;

namespace RobMensching.TinySite.Test
{
    public class RenderDocumentsCommandFixture
    {
        [Fact]
        public void CanRenderDocument()
        {
            var tinySiteAssembly = typeof(RazorRenderer).Assembly;

            var basePath = Path.GetFullPath(@"data\RenderDocumentsCommand\");

            var outputPath = Path.Combine(Path.GetTempPath(), @"tinysite\");

            var loadLayouts = new LoadLayoutsCommand(Path.Combine(basePath, @"layouts\"), null, null);
            var layouts = loadLayouts.Execute();
            var collection = new LayoutFileCollection(layouts);

            var loadDocuments = new LoadDocumentsCommand();
            loadDocuments.Author = new Author();
            loadDocuments.DocumentsPath = Path.Combine(basePath, @"documents\");
            loadDocuments.Layouts = collection;
            loadDocuments.OutputRootPath = outputPath;
            loadDocuments.RenderedExtensions = new[] { "md", "cshtml" };
            loadDocuments.RootUrl = "http://www.example.com/";
            loadDocuments.ApplicationUrl = "/app/sub";
            var documents = loadDocuments.Execute().OrderBy(d => d.Name).ToList();

            var config = new SiteConfig() { OutputPath = outputPath, Url = "http://example.com", RootUrl = String.Empty, };

            var site = new Site(config, Enumerable.Empty<DataFile>(), documents, Enumerable.Empty<StaticFile>(), collection);

            var engines = RenderingEngine.Load(config, tinySiteAssembly);

            Statistics.Current = new Statistics();

            var command = new RenderDocumentsCommand(engines, site);
            command.Execute();

            var description = "This is the summary of the document with a link to example.com.";
            var content = "<p>This is the summary of the document with a link to <a href=\"http://example.com\">example.com</a>.</p>\n<p>This is additional content in the document.</p>";
            var summary = "<p>This is the summary of the document with a link to <a href=\"http://example.com\">example.com</a>.</p>";
            var title = "test";

            Assert.Equal(0, command.RenderedData);
            Assert.Equal(3, command.RenderedDocuments);
            Assert.Equal(String.Empty, documents[0].RenderedContent.Trim());

            Assert.Equal("This is the summary of the document with a link to [example.com](http://example.com).\n\nThis is additional content in the document.", documents[2].SourceContent.Replace("\r\n", "\n"));
            Assert.Equal(content, documents[2].Content);
            Assert.Equal(description, documents[2].Description);
            Assert.Equal(summary, documents[2].Summary);
            Assert.Equal(title, documents[2].Metadata.Get<string>("title"));

            Assert.Equal(
                $"<title>{title}</title>\n" +
                $"<description>{description}</description>\n" +
                $"<summary>{summary}</summary>\n" +
                $"<content>{content}</content>",
                documents[2].RenderedContent.Replace("\r\n", "\n"));
        }
    }
}
