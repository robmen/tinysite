using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
                try
                {
                    foreach (var token in JObject.Parse(template))
                    {
                        if (token.Key.Equals("Content", StringComparison.OrdinalIgnoreCase))
                        {
                            content = (string)token.Value;
                        }
                        else if (token.Value.Type == JTokenType.Array)
                        {
                            var array = (JArray)token.Value;
                            document[token.Key] = array.Values().Select(t => (string)t).ToArray();
                        }
                        else
                        {
                            document[token.Key] = (string)token.Value;
                        }
                    }
                }
                catch (JsonReaderException e)
                {
                    Console.Error.WriteLine("{0}({1},{2}): error JSON1 : {3}", path, e.LineNumber, e.LinePosition, e.Message);
                    return null;
                }
            }

            return content;
        }

        public void Unload(IEnumerable<string> paths)
        {
        }
    }
}
