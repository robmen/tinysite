using System;
using System.IO;
using System.Linq;
using TinySite.Commands;
using TinySite.Models;
using TinySite.Models.Dynamic;
using Xunit;

namespace RobMensching.TinySite.Test
{
    public class SiteFixture
    {
        [Fact]
        public void CanLoadSiteConfig()
        {
            //TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var dataFolder = Path.GetFullPath(@"data\");

            var command = new LoadSiteConfigCommand() { ConfigPath = dataFolder + "site.config" };
            var config = command.Execute();

            Assert.Empty(config.SubsiteConfigs);
            Assert.Equal(dataFolder + @"build\here\", config.OutputPath);
            //Assert.Equal(tzi, config.TimeZone);
        }

        [Fact]
        public void CanLoadSiteConfigWithSubsites()
        {
            var tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var dataFolder = Path.GetFullPath(@"data\");

            var command = new LoadSiteConfigCommand() { ConfigPath = dataFolder + "parent.config" };
            var config = command.Execute();

            Assert.Equal(dataFolder + @"parent_build\", config.OutputPath);
            Assert.NotEmpty(config.SubsiteConfigs);
            Assert.Single(config.SubsiteConfigs);
            foreach (var subsite in config.SubsiteConfigs)
            {
                Assert.Equal(config, subsite.Parent);
            }
        }

        [Fact]
        public void CanGetDefaultUrlFromData()
        {
            var command = new LoadSiteConfigCommand() { ConfigPath = "data\\site.config" };
            var config = command.Execute();
            var site = new Site(config, Enumerable.Empty<DataFile>(), Enumerable.Empty<DocumentFile>(), Enumerable.Empty<StaticFile>(), Enumerable.Empty<LayoutFile>());

            dynamic data = site; //site.GetAsDynamic();

            Assert.Equal("/blog/", data.Url);
        }

        [Fact]
        public void CanGetFullUrlFromData()
        {
            var command = new LoadSiteConfigCommand() { ConfigPath = "data\\site.config" };
            var config = command.Execute();
            var site = new Site(config, Enumerable.Empty<DataFile>(), Enumerable.Empty<DocumentFile>(), Enumerable.Empty<StaticFile>(), Enumerable.Empty<LayoutFile>());

            dynamic data = site; //site.GetAsDynamic();

            Assert.Equal("http://www.example.com/blog/", data.FullUrl);
        }

        [Fact]
        public void CanGetTitle()
        {
            var command = new LoadSiteConfigCommand() { ConfigPath = "data\\site.config" };
            var config = command.Execute();

            var site = new Site(config, Enumerable.Empty<DataFile>(), Enumerable.Empty<DocumentFile>(), Enumerable.Empty<StaticFile>(), Enumerable.Empty<LayoutFile>());

            dynamic data = new DynamicSite(null, site);

            Assert.Equal("Test Blog.", (string)data.tiTle);
        }

        [Fact]
        public void CanGetDefaultLayout()
        {
            var command = new LoadSiteConfigCommand() { ConfigPath = "data\\site.config" };
            var config = command.Execute();

            var site = new Site(config, Enumerable.Empty<DataFile>(), Enumerable.Empty<DocumentFile>(), Enumerable.Empty<StaticFile>(), Enumerable.Empty<LayoutFile>());

            Assert.Equal("test", site.DefaultLayoutForExtension["html"]);
        }

        [Fact]
        public void CanGetFilesToIgnore()
        {
            //TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var dataFolder = Path.GetFullPath(@"data\");

            var command = new LoadSiteConfigCommand() { ConfigPath = dataFolder + "site.config" };
            var config = command.Execute();

            Assert.Equal(2, config.IgnoreFiles.Count());

            var match = config.IgnoreFiles.First();
            Assert.Matches(match, "foo.abc~");
            Assert.Matches(match, "a.b~");
            Assert.DoesNotMatch(match, "foo.abc");
            Assert.DoesNotMatch(match, "a.b");

            match = config.IgnoreFiles.Skip(1).Single();
            Assert.Matches(match, "bar.tmp");
            Assert.Matches(match, "foo.TMP");
        }
    }
}
