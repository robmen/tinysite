using System;
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
            command.Execute();

            var order = new OrderCommand();
            order.Documents = command.Documents;
            order.Execute();

            Assert.Single(order.Books);

            var documents = command.Documents.OrderBy(d => d.Order).ToList();

            Assert.Equal(0, documents[0].Order);
            Assert.Equal("parent.txt", documents[0].OutputRelativePath);
            Assert.Equal(String.Empty, documents[0].ParentId);
            Assert.Null(documents[0].ParentDocument);

            Assert.Equal(1, documents[1].Order);
            Assert.Equal("parent\\first-ordered-document.txt", documents[1].OutputRelativePath);
            Assert.Equal("parent", documents[1].ParentId);
            Assert.Equal(documents[0], documents[1].ParentDocument);

            Assert.Equal(1, documents[2].Order);
            Assert.Equal("parent\\second-document\\sub-second-document.txt", documents[2].OutputRelativePath);
            Assert.Equal("parent\\second-document", documents[2].ParentId);
            Assert.Equal(documents[3], documents[2].ParentDocument);

            Assert.Equal(2, documents[3].Order);
            Assert.Equal("parent\\second-document.txt", documents[3].OutputRelativePath);
            Assert.Equal("parent", documents[3].ParentId);
            Assert.Equal(documents[0], documents[3].ParentDocument);

            Assert.Equal(2, documents[4].Order);
            Assert.Equal("parent\\second-document\\another-sub-second-document.txt", documents[4].OutputRelativePath);
            Assert.Equal("parent\\second-document", documents[4].ParentId);
            Assert.Equal(documents[3], documents[4].ParentDocument);

            Assert.Equal(3, documents[5].Order);
            Assert.Equal("parent\\third-document-from-metadata.txt", documents[5].OutputRelativePath);
            Assert.Equal("parent", documents[5].ParentId);
            Assert.Equal(documents[0], documents[5].ParentDocument);
        }

        [Fact]
        public void CanLoadDatedDocuments()
        {
            var path = Path.GetFullPath(@"data\dated-documents\");

            var command = new LoadDocumentsCommand();
            command.Author = new Author();
            command.DocumentsPath = path;
            command.OutputRootPath = Path.GetFullPath("output");
            command.RenderedExtensions = new[] { "md" };
            command.RootUrl = "http://www.example.com/";
            command.ApplicationUrl = "/foo";
            command.Execute();

            var documents = command.Documents.OrderByDescending(d => d.OutputRelativePath).ToList();
            Assert.Equal(4, documents.Count);

            Assert.Equal(@"parent\index.html", documents[0].OutputRelativePath);

            Assert.Equal(@"2013-12-18", documents[1].Date.ToString("yyyy-MM-dd"));
            Assert.Equal(@"parent\2013\12\18\c\index.html", documents[1].OutputRelativePath);

            Assert.Equal(@"2013-12-17", documents[2].Date.ToString("yyyy-MM-dd"));
            Assert.Equal(@"parent\2013\12\17\b\index.html", documents[2].OutputRelativePath);

            Assert.Equal(@"parent\2011\11\5\a\index.html", documents[3].OutputRelativePath);
            Assert.Equal(@"2011-11-05", documents[3].Date.ToString("yyyy-MM-dd"));
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
            command.Execute();

            var order = new OrderCommand();
            order.Documents = command.Documents;
            order.Execute();

            Assert.Empty(order.Books);
        }
    }
}
