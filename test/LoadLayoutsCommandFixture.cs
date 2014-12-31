using System.IO;
using TinySite.Commands;
using Xunit;

namespace RobMensching.TinySite.Test
{
    public class LoadLayoutsCommandFixture
    {
        [Fact]
        public void CanLoadLayouts()
        {
            var path = Path.GetFullPath(@"data\layouts\");

            var command = new LoadLayoutsCommand();
            command.LayoutsPath = path;
            var layouts = command.ExecuteAsync().Result;

            Assert.NotEmpty(command.Layouts);
            Assert.NotEmpty(layouts);
        }

        [Fact]
        public void CanLoadNoLayouts()
        {
            var path = Path.GetFullPath(@"data\doesnotexist\");

            var command = new LoadLayoutsCommand();
            command.LayoutsPath = path;
            var layouts = command.ExecuteAsync().Result;

            Assert.Empty(command.Layouts);
            Assert.Empty(layouts);
        }
    }
}
