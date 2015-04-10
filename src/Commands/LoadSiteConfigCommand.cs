using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinySite.Extensions;
using TinySite.Models;

namespace TinySite.Commands
{
    public class LoadSiteConfigCommand
    {
        public string ConfigPath { private get; set; }

        public SiteConfig Parent { private get; set; }

        public string OutputPath { private get; set; }

        public SiteConfig SiteConfig { get; private set; }

        public async Task<SiteConfig> ExecuteAsync()
        {
            var root = Path.GetFullPath(Path.GetDirectoryName(this.ConfigPath));

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new JsonTimeZoneConverter());

            string json;
            using (var reader = new StreamReader(this.ConfigPath))
            {
                json = await reader.ReadToEndAsync();
            }

            var config = new SiteConfig();
            config.Parent = this.Parent;

            var ignoreFiles = new string[0];
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

                    case "defaultlayoutforextension":
                        this.AssignDefaultLayouts(config, value);
                        break;

                    case "ignorefiles":
                        config.IgnoreFiles = this.ParseIgnoreFiles(value.Values<string>()).ToArray();
                        break;

                    default:
                        config.Metadata.Add(key, value);
                        break;
                }
            }

            config.SitePath = root;
            config.DocumentsPath = Path.Combine(root, "documents\\");
            config.FilesPath = Path.Combine(root, "files\\");
            config.LayoutsPath = Path.Combine(root, "layouts\\");

            config.OutputPath = config.OutputPath ?? Path.Combine(root, "build\\");
            config.Url = config.Url.EnsureStartsWith("/");
            config.RootUrl = config.RootUrl ?? "http://localhost/";

            // If override output path was provided use that.
            config.OutputPath = String.IsNullOrEmpty(this.OutputPath) ? Path.GetFullPath(config.OutputPath) : Path.GetFullPath(this.OutputPath);

            var subsiteLoadTasks = new List<Task<SiteConfig>>(subsites.Length);

            foreach (var subsite in subsites)
            {
                var command = new LoadSiteConfigCommand();
                command.Parent = config;
                command.ConfigPath = Path.Combine(root, subsite);
                var task = command.ExecuteAsync();

                subsiteLoadTasks.Add(task);
            }

            config.SubsiteConfigs = await Task.WhenAll(subsiteLoadTasks);

            return this.SiteConfig = config;
        }

        private void AssignDefaultLayouts(SiteConfig config, JToken token)
        {
            var layoutDefaults = token as JObject;

            if (layoutDefaults == null)
            {
                config.DefaultLayoutForExtension.Add("*", (string)token);
            }
            else
            {
                foreach (var layoutDefault in layoutDefaults)
                {
                    config.DefaultLayoutForExtension.Add(layoutDefault.Key, (string)layoutDefault.Value);
                }
            }
        }

        private IEnumerable<Regex> ParseIgnoreFiles(IEnumerable<string> ignoreFilePatterns)
        {
            foreach (var pattern in ignoreFilePatterns)
            {
                var regex = "^" + pattern.Replace(".", @"\.").Replace("?", ".").Replace("*", ".*?") + "$";

                yield return new Regex(regex, RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
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
