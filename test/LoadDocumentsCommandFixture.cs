using System.IO;
using System.Linq;
using TinySite.Commands;
using TinySite.Models;
using TinySite.Models.Dynamic;
using Xunit;

namespace RobMensching.TinySite.Test
{
    public class LoadDocumentCommandFixture
    {
        [Fact]
        public void CanSpecifyOutputPath()
        {
            var path = Path.GetFullPath(@"data\test-documents\explicit-output");
            var outputPath = Path.GetFullPath("output");
            var expectedOutput = Path.Combine(outputPath, @"put-that\over\here.txt");
            var expectedUrl = "http://www.example.com/app/sub/put-that/over/here.txt";

            var command = new LoadDocumentsCommand();
            command.Author = new Author();
            command.DocumentsPath = path;
            command.OutputRootPath = outputPath;
            command.RenderedExtensions = new[] { "md" };
            command.RootUrl = "http://www.example.com/";
            command.ApplicationUrl = "/app/sub";
            command.ExecuteAsync().Wait();

            var document = command.Documents.Single();

            Assert.Equal(expectedOutput, document.OutputPath);
            Assert.Equal(expectedUrl, document.Url);
        }

        [Fact]
        public void CanLoadOrderedDocuments()
        {
            var path = Path.GetFullPath(@"data\ordered-documents\");

            var command = new LoadDocumentsCommand();
            command.Author = new Author();
            command.DocumentsPath = path;
            command.OutputRootPath = Path.GetFullPath("output");
            command.RenderedExtensions = new[] { "md" };
            command.RootUrl = "http://www.example.com/";
            command.ApplicationUrl = "/foo";
            command.ExecuteAsync().Wait();

            var documents = command.Documents.OrderBy(d => d.Order).ToList();

            Assert.Equal(0, documents[0].Order);
            Assert.Equal("parent", documents[0].Metadata.Get<string>("title"));
            Assert.Equal("parent.txt", documents[0].OutputRelativePath);

            Assert.Equal(1, documents[1].Order);
            Assert.Equal("First Ordered Document", documents[1].Metadata.Get<string>("title"));
            Assert.Equal("parent\\first-ordered-document.txt", documents[1].OutputRelativePath);
            Assert.Equal("parent", documents[1].ParentId);

            Assert.Equal(1, documents[2].Order);
            Assert.Equal("Sub-Second Document", documents[2].Metadata.Get<string>("title"));
            Assert.Equal("parent\\second-document\\sub-second-document.txt", documents[2].OutputRelativePath);
            Assert.Equal("parent\\second-document", documents[2].ParentId);

            Assert.Equal(2, documents[3].Order);
            Assert.Equal("Second Document", documents[3].Metadata.Get<string>("title"));
            Assert.Equal("parent\\second-document.txt", documents[3].OutputRelativePath);
            Assert.Equal("parent", documents[3].ParentId);

            Assert.Equal(2, documents[4].Order);
            Assert.Equal("Another Sub-Second Document", documents[4].Metadata.Get<string>("title"));
            Assert.Equal("parent\\second-document\\another-sub-second-document.txt", documents[4].OutputRelativePath);
            Assert.Equal("parent\\second-document", documents[4].ParentId);

            Assert.Equal(3, documents[5].Order);
            Assert.Equal("Third Document From Metadata", documents[5].Metadata.Get<string>("title"));
            Assert.Equal("parent\\third-document-from-metadata.txt", documents[5].OutputRelativePath);
            Assert.Equal("parent", documents[5].ParentId);
        }

        [Fact]
        public void CanLoadComplexMetadata()
        {
            var path = Path.GetFullPath(@"data\test-documents\complex-metadata");
            var outputPath = Path.GetFullPath("output");

            var command = new LoadDocumentsCommand();
            command.Author = new Author();
            command.DocumentsPath = path;
            command.OutputRootPath = outputPath;
            command.RenderedExtensions = new[] { "md" };
            command.RootUrl = "http://www.example.com/";
            command.ApplicationUrl = "/app/sub";
            command.ExecuteAsync().Wait();

            var document = command.Documents.Single();

            dynamic dynamicDoc = new DynamicDocumentFile(document, document, null);

            Assert.Equal("Complex Metadata is Fun!", dynamicDoc.Subtitle);
            Assert.Equal("/item1/", dynamicDoc.ComplexItems[0].Url);
            Assert.Equal("Item 1", dynamicDoc.ComplexItems[0].Text);
            Assert.Equal("bar", dynamicDoc.ComplexItems[0].Zzz.Foo);
            Assert.Equal("/item2/subitem/", dynamicDoc.ComplexItems[1].Url);
            Assert.Equal("Item 2 Subitem", dynamicDoc.ComplexItems[1].Text);
            Assert.Equal("g-value", dynamicDoc.ComplexItems[1].Ttt.Ooo.Ggg);
        }

        [Fact]
        public void CanLoadWithAdditionalMetadata()
        {
            var dataFolder = Path.GetFullPath(@"data\additional-metadata\");
            var outputPath = Path.GetFullPath("output");

            var loadConfig = new LoadSiteConfigCommand() { ConfigPath = dataFolder + "site.json" };
            var config = loadConfig.ExecuteAsync().Result;

            var loadData = new LoadDataFilesCommand(config.DataPath, config.AdditionalMetadataForFiles, config.IgnoreFiles);
            loadData.ExecuteAsync().Wait();

            var loadDocuments = new LoadDocumentsCommand();
            loadDocuments.Author = new Author();
            loadDocuments.DocumentsPath = config.DocumentsPath;
            loadDocuments.OutputRootPath = config.OutputPath;
            loadDocuments.AdditionalMetadataForFiles = config.AdditionalMetadataForFiles;
            loadDocuments.IgnoreFiles = config.IgnoreFiles;
            loadDocuments.RenderedExtensions = new[] { "md" };
            loadDocuments.RootUrl = config.RootUrl;
            loadDocuments.ApplicationUrl = config.Url;
            loadDocuments.ExecuteAsync().Wait();

            var data = loadData.DataFiles.Single();
            var document = loadDocuments.Documents.Single();

            Assert.Equal("bar", data.Metadata.Get<string>("foo"));
            Assert.Equal("quux", document.Metadata.Get<string>("baz"));
        }
    }
}
