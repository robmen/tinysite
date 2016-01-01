using System;
using System.IO;
using System.Linq;
using TinySite.Models;
using TinySite.Models.Query;
using TinySite.Services;
using Xunit;

namespace RobMensching.TinySite.Test
{
    public class QueryProcessorFixture
    {
        [Fact]
        public void CanParseQuery()
        {
            var config = new SiteConfig()
            {
                RootUrl = String.Empty,
                Url = "http://www.example.com"
            };

            var documents = new[] 
            {
            new DocumentFile("bar.html.md", Path.GetFullPath("documents"), "documents", "documents", "bar", "bar", null, new MetadataCollection(), null),
            new DocumentFile("foo.html.md", Path.GetFullPath("documents"), "documents", "documents", "foo", "foo", null, new MetadataCollection(), null),
            };

            var site = new Site(config, Enumerable.Empty<DataFile>(), documents, Enumerable.Empty<StaticFile>(), Enumerable.Empty<LayoutFile>());

            var query = @"query documents every 10 where sourcerelativepath startswith ""documents\posts\"" descending date formaturl ""posts/page/{0}""";

            var result = QueryProcessor.Parse(site, query);

            Assert.Equal(2, result.Source.Count());
            Assert.Equal(10, result.PageEvery);
            Assert.Equal(WhereOperator.StartsWith, result.Where.Operator);
            Assert.Equal("sourcerelativepath", result.Where.Property);
            Assert.Equal(@"documents\posts\", result.Where.Value);
            Assert.Equal(OrderOperator.Descending, result.Order.Operator);
            Assert.Equal("date", result.Order.Property);
            Assert.Equal(@"posts/page/{0}", result.FormatUrl);

            var q = result.Results.ToList();
            Assert.Empty(q);
        }

        [Fact]
        public void CanDoQueryable()
        {
            var config = new SiteConfig()
            {
                RootUrl = String.Empty,
                Url = "http://www.example.com"
            };

            var documents = new[] 
            {
            new DocumentFile("bar.html.md", Path.GetFullPath("documents"), "documents", "documents", "bar", "bar", null, new MetadataCollection(), null),
            new DocumentFile("foo.html.md", Path.GetFullPath("documents"), "documents", "documents", "foo", "foo", null, new MetadataCollection(), null),
            };

            var site = new Site(config, Enumerable.Empty<DataFile>(), documents, Enumerable.Empty<StaticFile>(), Enumerable.Empty<LayoutFile>());

            var query = @"query documents where outputpath startswith ""doc"" descending url";

            var p = QueryProcessor.Parse(site, query);

            var q = p.Results.ToList();
            Assert.Equal("foo/foo", q[0].Url);
            Assert.Equal("bar/bar", q[1].Url);
        }

        [Fact]
        public void CanDoQueryableNumeric()
        {
            var config = new SiteConfig()
            {
                RootUrl = String.Empty,
                Url = "http://www.example.com"
            };

            var meta1 = new MetadataCollection();
            meta1.Add("number", 1);

            var meta2 = new MetadataCollection();
            meta2.Add("number", 20);

            var meta3 = new MetadataCollection();
            meta3.Add("number", 3);

            var documents = new[]
            {
                new DocumentFile("1.html.md", Path.GetFullPath("documents"), "documents", "1", "1", String.Empty, null, meta1, null),
                new DocumentFile("20.html.md", Path.GetFullPath("documents"), "documents", "20", "20", String.Empty, null, meta2, null),
                new DocumentFile("3.html.md", Path.GetFullPath("documents"), "documents", "3", "3", String.Empty, null, meta3, null),
            };

            var site = new Site(config, Enumerable.Empty<DataFile>(), documents, Enumerable.Empty<StaticFile>(), Enumerable.Empty<LayoutFile>());

            var query = @"query documents where number gt 1 ascending number";

            var p = QueryProcessor.Parse(site, query);

            var q = p.Results.ToList();
            Assert.Equal(3, q[0].Number);
            Assert.Equal(20, q[1].Number);
        }
    }
}