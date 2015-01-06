using System.IO;
using System.Linq;
using TinySite.Commands;
using TinySite.Models;
using Xunit;

namespace RobMensching.TinySite.Test
{
    public class LoadDocumentCommandFixture
    {
        [Fact]
        public void CanLoadOrderedDocuments()
        {
            var path = Path.GetFullPath(@"data\ordered-documents\");

            var command = new LoadDocumentsCommand();
            command.Author = new Author();
            command.DocumentsPath = path;
            command.OutputPath = Path.GetFullPath("output");
            command.RenderedExtensions = new[] { "md" };
            command.RootUrl = "http://www.example.com/";
            command.Url = "foo";
            command.ExecuteAsync().Wait();

            var documents = command.Documents.OrderBy(d => d.Order).ToList();

            Assert.Equal(1, documents[0].Order);
            Assert.Equal("First Ordered Document", documents[0].Metadata.Get<string>("title"));
            Assert.Equal("first-ordered-document.txt", documents[0].RelativePath);

            Assert.Equal(2, documents[1].Order);
            Assert.Equal("Second Document", documents[1].Metadata.Get<string>("title"));
            Assert.Equal("second-document.txt", documents[1].RelativePath);

            Assert.Equal(3, documents[2].Order);
            Assert.Equal("Third Document From Metadata", documents[2].Metadata.Get<string>("title"));
            Assert.Equal("third-document-from-metadata.txt", documents[2].RelativePath);
        }
    }
}
