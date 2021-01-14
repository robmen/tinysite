﻿using System.IO;
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

            var command = new LoadLayoutsCommand(path, null, null);
            var layouts = command.Execute();

            Assert.NotEmpty(command.Layouts);
            Assert.NotEmpty(layouts);
        }

        [Fact]
        public void CanLoadNoLayouts()
        {
            var path = Path.GetFullPath(@"data\doesnotexist\");

            var command = new LoadLayoutsCommand(path, null, null);
            var layouts = command.Execute();

            Assert.Empty(command.Layouts);
            Assert.Empty(layouts);
        }
    }
}
