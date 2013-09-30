using System;
using System.IO;
using System.Linq;
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

            var config = SiteConfig.Load(dataFolder + "site.config");
            Assert.Empty(config.SubsiteConfigs);
            Assert.Equal(dataFolder + @"build\here\", config.OutputPath);
            //Assert.Equal(tzi, config.TimeZone);
        }

        [Fact]
        public void CanLoadSiteConfigWithSubsites()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var dataFolder = Path.GetFullPath(@"data\");

            var config = SiteConfig.Load(dataFolder + "parent.config");
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
            var config = SiteConfig.Load("data\\site.config");
            //var data = config.GetAsDyamic();
            //Assert.Equal("/", data.Url);
        }

        //[Fact]
        //public void CanGetDynamicFunctionFromData()
        //{
        //    var config = SiteConfig.Load("data\\site.config");
        //    var data = config.GetAsData();

        //    Type t = data.Get2.GetType();
        //    Type d = data.GetType();
        //    MemberInfo[] ms = d.GetMember("Get2", MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        //    IDictionary<string, object> dic = (IDictionary<string, object>)data;
        //    var m = dic["Get2"];

        //    //var x = data.Get(config);
        //    var x = data.Get;
        //    Assert.Equal("/", x);

        //    config.Test = new string[] { "a", "b", "c" };

        //    var y = data.Get2();
        //    foreach (string s in y)
        //    {
        //        Assert.Equal(1, s.Length);
        //    }

        //    config.Test = new string[] { "aa", "bb", "cc" };
        //    y = data.Get2();
        //    foreach (string s in y)
        //    {
        //        Assert.Equal(2, s.Length);
        //    }
        //    //var y = x.Count;
        //    //Assert.Equal(3, y);
        //}

        [Fact]
        public void CanGetTitle()
        {
            var config = SiteConfig.Load("data\\site.config");
            var site = Site.Load(config, Enumerable.Empty<string>());

            var data = site.GetAsDynamic();

            Assert.Equal("Test Blog.", (string)data.Title);
        }
    }
}
