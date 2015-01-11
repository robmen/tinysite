using System.IO;
using System.Linq;
using TinySite.Commands;
using TinySite.Models;
using Xunit;

namespace RobMensching.TinySite.Test
{
    public class OrderCommandFixture
    {
        [Fact]
        public void CanOrderOrderedDocuments()
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

            var order = new OrderCommand();
            order.Documents = command.Documents;
            order.Execute();

            Assert.Equal(1, order.Books.Count());

            var doc = command.Documents.Skip(3).Take(1).Single();

            var data = order.Books.First().GetAsDynamic(doc);
        }

        [Fact]
        public void CanOrderUnorderedDocuments()
        {
            var path = Path.GetFullPath(@"data\dated-documents\");

            var command = new LoadDocumentsCommand();
            command.Author = new Author();
            command.DocumentsPath = path;
            command.OutputRootPath = Path.GetFullPath("output");
            command.RenderedExtensions = new[] { "md" };
            command.RootUrl = "http://www.example.com/";
            command.ApplicationUrl = "/foo";
            command.ExecuteAsync().Wait();

            var order = new OrderCommand();
            order.Documents = command.Documents;
            order.Execute();

            Assert.Equal(0, order.Books.Count());
        }
    }
}
