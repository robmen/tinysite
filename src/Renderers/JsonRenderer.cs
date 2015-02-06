using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("json")]
    public class JsonRenderer : IRenderer
    {
        private object sync = new object();

        public string Render(string path, string template, dynamic data)
        {
            var document = data.Document;

            var content = String.Empty;

            lock (sync)
            {
                foreach (var token in JObject.Parse(template))
                {
                    if (token.Key.Equals("Content", StringComparison.OrdinalIgnoreCase))
                    {
                        content = (string)token.Value;
                    }
                    else
                    {
                        document[token.Key] = (string)token.Value;
                    }
                }
            }

            return content;
        }
    }
}
