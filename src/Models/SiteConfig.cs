using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinySite.Extensions;

namespace TinySite.Models
{
    public class SiteConfig
    {
        public SiteConfig()
        {
            this.Metadata = new MetadataCollection();
        }

        public Author Author { get; set; }

        public string DocumentsPath { get; set; }

        public string FilesPath { get; set; }

        public string LayoutsPath { get; set; }

        public string OutputPath { get; set; }

        public SiteConfig[] SubsiteConfigs { get; private set; }

        public string Url { get; set; }

        public string RootUrl { get; set; }

        public SiteConfig Parent { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public MetadataCollection Metadata { get; set; }

        //public dynamic GetAsDynamic()
        //{
        //    dynamic data = new CaseInsenstiveExpando();

        //    this.Metadata.Assign(data as IDictionary<string, object>);

        //    data.OutputPath = this.OutputPath;
        //    data.Url = this.Url;
        //    data.RootUrl = this.RootUrl;
        //    data.FullUrl = this.RootUrl.EnsureEndsWith("/") + this.Url.TrimStart('/');

        //    return data;
        //}

        public static SiteConfig Load(string path, SiteConfig parent = null)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new JsonTimeZoneConverter());

            var root = Path.GetFullPath(Path.GetDirectoryName(path));
            var json = File.ReadAllText(path);

            var config = new SiteConfig();
            config.Parent = parent;

            var subsites = new string[0];

            //var config = JsonConvert.DeserializeObject<SiteConfig>(json, settings);
            foreach (var token in JObject.Parse(json))
            {
                var key = token.Key.ToLowerInvariant();
                var value = token.Value;

                switch (key)
                {
                    case "author":
                        config.Author = value.ToObject<Author>();
                        break;

                    case "output":
                    case "outputpath":
                        config.OutputPath = Path.Combine(root, (string)value).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).EnsureBackslashTerminated();
                        break;

                    case "url":
                        config.Url = (string)value;
                        break;

                    case "rooturl":
                        config.RootUrl = (string)value;
                        break;

                    case "subsites":
                        subsites = value.Values<string>().ToArray();
                        break;

                    default:
                        config.Metadata.Add(key, value);
                        break;
                }
            }

            config.DocumentsPath = Path.Combine(root, "documents\\");
            config.FilesPath = Path.Combine(root, "files\\");
            config.LayoutsPath = Path.Combine(root, "layouts\\");

            config.OutputPath = config.OutputPath ?? Path.Combine(root, "build\\");
            config.Url = config.Url ?? "/";
            config.RootUrl = config.RootUrl ?? "http://localhost/";

            config.SubsiteConfigs = new SiteConfig[subsites.Length];
            for (int i = 0; i < subsites.Length; ++i)
            {
                string subPath = Path.Combine(Path.GetDirectoryName(path), subsites[i]);
                config.SubsiteConfigs[i] = SiteConfig.Load(subPath, config);
            }

            return config;
        }

        private class JsonTimeZoneConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType.Equals(typeof(TimeZoneInfo));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return TimeZoneInfo.FindSystemTimeZoneById((string)reader.Value);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
