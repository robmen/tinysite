using System;
using System.IO;
using System.Linq;
using TinySite.Commands;
using TinySite.Models;
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
            var config = command.ExecuteAsync().Result;

            Assert.Empty(config.SubsiteConfigs);
            Assert.Equal(dataFolder + @"build\here\", config.OutputPath);
            //Assert.Equal(tzi, config.TimeZone);
        }

        [Fact]
        public void CanLoadSiteConfigWithSubsites()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var dataFolder = Path.GetFullPath(@"data\");

            var command = new LoadSiteConfigCommand() { ConfigPath = dataFolder + "parent.config" };
            var config = command.ExecuteAsync().Result;

            Assert.Equal(dataFolder + @"parent_build\", config.OutputPath);
            Assert.NotEmpty(config.SubsiteConfigs);
            Assert.Equal(1, config.SubsiteConfigs.Length);
            foreach (var subsite in config.SubsiteConfigs)
            {
                Assert.Equal(config, subsite.Parent);
            }
        }

        [Fact]
        public void CanGetDefaultUrlFromData()
        {
            var command = new LoadSiteConfigCommand() { ConfigPath = "data\\site.config" };
            var config = command.ExecuteAsync().Result;
            var site = new Site(config, Enumerable.Empty<DocumentFile>(), Enumerable.Empty<StaticFile>(), Enumerable.Empty<LayoutFile>());

            dynamic data = site; //site.GetAsDynamic();

            Assert.Equal("/blog/", data.urL);
        }

        [Fact]
        public void CanGetFullUrlFromData()
        {
            var command = new LoadSiteConfigCommand() { ConfigPath = "data\\site.config" };
            var config = command.ExecuteAsync().Result;
            var site = new Site(config, Enumerable.Empty<DocumentFile>(), Enumerable.Empty<StaticFile>(), Enumerable.Empty<LayoutFile>());

            dynamic data = site; //site.GetAsDynamic();

            Assert.Equal("http://www.example.com/blog/", data.fullurl);
        }

        [Fact]
        public void CanGetTitle()
        {
            var command = new LoadSiteConfigCommand() { ConfigPath = "data\\site.config" };
            var config = command.ExecuteAsync().Result;

            var site = new Site(config, Enumerable.Empty<DocumentFile>(), Enumerable.Empty<StaticFile>(), Enumerable.Empty<LayoutFile>());

            dynamic data = site; //.GetAsDynamic();

            Assert.Equal("Test Blog.", (string)data.tiTle);
        }
    }
}
