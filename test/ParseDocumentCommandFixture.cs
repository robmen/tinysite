using System.IO;
using TinySite.Commands;
using Xunit;

namespace RobMensching.TinySite.Test
{
    public class ParseDocumentCommandFixture
    {
        [Fact]
        public void CanParseNoMetadata()
        {
            var path = Path.GetFullPath(@"data\documents\nometa.txt");
            var expected = "This is text.\n   It has no metadata.";

            var command = new ParseDocumentCommand(path);
            command.Execute();

            Assert.Null(command.Date);
            Assert.False(command.Draft);
            Assert.Empty(command.Metadata);
            Assert.Equal(expected, command.Content.Replace("\r\n", "\n"));
        }

        [Fact]
        public void CanParseMetadata()
        {
            var path = Path.GetFullPath(@"data\documents\hasmeta.txt");
            var expected = "This is text.\r\n   It has title metadata.";

            var command = new ParseDocumentCommand(path);
            command.Execute();

            Assert.Null(command.Date);
            Assert.NotEmpty(command.Metadata);
            Assert.Equal("Title from the metadata.", command.Metadata.Get<string>("title"));
            Assert.Equal(expected.Replace("\r\n", "\n"), command.Content.Replace("\r\n", "\n"));
        }

        [Fact]
        public void CanParseDraft()
        {
            var path = Path.GetFullPath(@"data\documents\draft.txt");
            string expected = "This is a draft.\r\n   It has metadata.";

            var command = new ParseDocumentCommand(path);
            command.Execute();

            Assert.Null(command.Date);
            Assert.True(command.Draft);
            Assert.Empty(command.Metadata);
            Assert.Equal(expected.Replace("\r\n", "\n"), command.Content.Replace("\r\n", "\n"));
        }

        [Fact]
        public void CanParseTag()
        {
            var path = Path.GetFullPath(@"data\documents\hastag.txt");

            var command = new ParseDocumentCommand(path);
            command.Execute();

            Assert.Equal(new[] { "foo" }, command.Metadata.Get<string[]>("tags"));
        }

        [Fact]
        public void CanParseTags()
        {
            var path = Path.GetFullPath(@"data\documents\hastags.txt");

            var command = new ParseDocumentCommand(path);
            command.Execute();

            Assert.Equal(new[] { "foo", "bar", "baz" }, command.Metadata.Get<string[]>("tags"));
        }
    }
}
